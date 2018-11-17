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
            services.AddWitAi(Configuration.GetSection("Wit.ai"));
            services.AddOperationServices();

            services.AddTransient<BusVbot>()
                .Configure<BotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .Configure<CustomBotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .AddScoped<WebhookResponse>()
                .AddScoped<StartCommand>()
                .AddScoped<HelpCommand>()
                .AddScoped<CancelCommand>()
                .AddScoped<LocationHandler>()
                .AddScoped<UserProfileSetupHandler>()
                .AddScoped<UserProfileSetupMenuHandler>()
                .AddScoped<UserProfileRemovalHandler>()
                .AddScoped<ProfileCommand>()
                .AddScoped<BusCommand>()
                .AddScoped<BusCQHandler>()
                .AddScoped<BusPredictionsHandler>()
                .AddScoped<PredictionRefreshHandler>()
                .AddScoped<VoiceHandler>()
//                .AddScoped<LocationHandler>()
//                .AddScoped<SaveCommand>()
//                .AddScoped<SavedLocationHandler>()
//                .AddScoped<DeleteCommand>()
                ;

            services.AddMessageFormattingServices(Configuration.GetSection("Agencies"));

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
                .UseCommand<StartCommand>("start")
                .UseCommand<HelpCommand>("help")
                .UseCommand<CancelCommand>("cancel")

                // accept locations as a location or text coordinates(OSM)
                .UseWhen<LocationHandler>(LocationHandler.HasLocationOrCoordinates)

                // update the "Set User Agency" inline keyboard menu
                .UseWhen<UserProfileSetupMenuHandler>(UserProfileSetupMenuHandler.CanHandle)
                // ensure the user has a profile loaded for the rest of the handlers
                .UseWhen<UserProfileSetupHandler>(UserProfileSetupHandler.CanHandle)

                // ToDo comments
                .UseWhen<VoiceHandler>(VoiceHandler.IsVoiceMessage)
                // for new messages...
                .UseWhen(When.NewMessage, msgBranch => msgBranch
                    // for new text messages...
                    .UseWhen(When.NewTextMessage, txtBranch => txtBranch
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
                // for callback queries...
                .UseWhen<BusCQHandler>(BusCQHandler.CanHandle)
//                .MapWhen(When.CallbackQuery, cqBranch => cqBranch
//                    .MapWhen<PredictionRefreshCqHandler>(When.IsBusPredictionCq)
//                )
                // ToDo comments
                .UseWhen<BusPredictionsHandler>(BusPredictionsHandler.CanHandle)
                .UseWhen<PredictionRefreshHandler>(PredictionRefreshHandler.CanHandle);
    }
}
