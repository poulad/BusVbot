using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using BusV.Telegram.Extensions;
using BusV.Telegram.Handlers;
using BusV.Telegram.Handlers.Commands;
using BusV.Telegram.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace BusV.Telegram
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongoDb(Configuration.GetSection("Mongo"));
            services.AddRedisCache(Configuration.GetSection("Redis"));

            services.AddOperationServices();

            services.AddTransient<BusVbot>()
                .Configure<BotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .Configure<CustomBotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .AddScoped<WebhookResponse>()
                .AddScoped<StartCommand>()
                .AddScoped<HelpCommand>()
                .AddScoped<UserProfileSetupHandler>()
                .AddScoped<UserProfileSetupMenuHandler>()
                .AddScoped<UserProfileRemovalHandler>()
                .AddScoped<ProfileCommand>()
                .AddScoped<BusCommand>()
//                .AddScoped<BusDirectionCallbackQueryHandler>()
//                .AddScoped<PredictionRefreshCqHandler>()
//                .AddScoped<LocationHandler>()
//                .AddScoped<SaveCommand>()
//                .AddScoped<SavedLocationHandler>()
//                .AddScoped<DeleteCommand>()
                ;

            services.AddMessageFormattingServices(Configuration.GetSection("Agencies"));

//
//            services.AddTransient<ICachingService, CachingService>();
//
//            services.AddTransient<ILocationsManager, LocationsManager>();
//            services.AddTransient<IPredictionsManager, PredictionsManager>();
//
//            services.AddTransient<UserContextManager>();
//
//            services.AddMemoryCache();
//
//            services.AddScoped<DataSeeder>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.EnsureRedisConnection();

                app.EnsureDatabaseSchema();
                app.EnsureDatabaseSeeded();

                app.UseTelegramBotLongPolling<BusVbot>(ConfigureBot(), startAfter: TimeSpan.FromSeconds(2));
            }
            else
            {
                app.UseTelegramBotWebhook<BusVbot>(ConfigureBot());
                app.EnsureWebhookSet<BusVbot>();
            }

            app.Run(async context => { await context.Response.WriteAsync("Hello World!"); });
        }

        private IBotBuilder ConfigureBot() => // ToDo .Use<ExceptionHandler>()
            new BotBuilder()
                // respond to the webhook with a request, if available
                .Use<WebhookResponse>()
                // global commands. these don't require loading the user profile
                // ToDo remove this branch: https://github.com/TelegramBots/Telegram.Bot.Framework/issues/17
                .UseWhen(When.NewTextMessage, txtBranch => txtBranch
                    .UseCommand<StartCommand>("start")
                    .UseCommand<HelpCommand>("help")
                )
                // ensure the user has a profile loaded for the rest of the handlers
                .UseWhen<UserProfileSetupHandler>(UserProfileSetupHandler.CanHandle)
                // update the "Set User Agency" inline keyboard menu
                .MapWhen<UserProfileSetupMenuHandler>(UserProfileSetupMenuHandler.CanHandle)
                // for new messages...
                .MapWhen(When.NewMessage, msgBranch => msgBranch
                    // accept locations as a location or a text coordinates(OSM)
                    .MapWhen<LocationHandler>(LocationHandler.HasLocationOrCoordinates)
                    // for new text messages...
                    .MapWhen(When.NewTextMessage, txtBranch => txtBranch
                        // handle new commands
                        .UseCommand<ProfileCommand>("profile")
                        .UseCommand<BusCommand>("bus")
//                        .UseCommand<SaveCommand>("save")
//                        .UseCommand<DeleteCommand>("del")
                        // try to handle a frequent location (from reply markup keyboard)
//                        .UseWhen<SavedLocationHandler>(When.HasSavedLocationPrefix)
                        .UseWhen<UserProfileRemovalHandler>(UserProfileRemovalHandler.CanHandle)
                    )
                )
        /*
                // for callback queries...
                .MapWhen(When.CallbackQuery, cqBranch => cqBranch
                    .MapWhen<BusDirectionCallbackQueryHandler>(When.IsBusDirectionCq)
                    .MapWhen<PredictionRefreshCqHandler>(When.IsBusPredictionCq)
                )
            */;
    }
}
