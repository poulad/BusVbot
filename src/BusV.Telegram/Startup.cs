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
            services.AddMongoDb(Configuration.GetSection("Data"));

            services.AddTransient<BusVbot>()
                .Configure<BotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .Configure<CustomBotOptions<BusVbot>>(Configuration.GetSection("Bot"))
                .AddScoped<ChannelMessageHandler>()
                .AddScoped<StartCommand>()
                .AddScoped<HelpCommand>()
//                .AddScoped<UserProfileSetupHandler>()
//                .AddScoped<BusCommand>()
//                .AddScoped<BusDirectionCallbackQueryHandler>()
//                .AddScoped<PredictionRefreshCqHandler>()
//                .AddScoped<LocationHandler>()
//                .AddScoped<SaveCommand>()
//                .AddScoped<SavedLocationHandler>()
//                .AddScoped<DeleteCommand>()
                ;
//
//            services.Configure<AgencyTimeZonesAccessor>(Configuration.GetSection("Agencies"));
//
//            // ToDo Use extension method and chained methods for this
//            services.AddTransient<IDefaultAgencyDataParser, DefaultAgencyDataParser>();
//            services.AddTransient<IDefaultAgencyMessageFormatter, DefaultAgencyMessageFormatter>();
//            services.AddTransient<TtcDataParser>();
//            services.AddTransient<TtcMessageFormatter>();
//            services.AddTransient<IAgencyServiceAccessor, AgencyServiceAccessor>(factory =>
//            {
//                var parsers = new IAgencyDataParser[] {factory.GetRequiredService<TtcDataParser>()};
//                var formatters = new IAgencyMessageFormatter[] {factory.GetRequiredService<TtcMessageFormatter>()};
//                return new AgencyServiceAccessor(
//                    factory.GetRequiredService<IDefaultAgencyDataParser>(),
//                    factory.GetRequiredService<IDefaultAgencyMessageFormatter>(),
//                    parsers,
//                    formatters
//                );
//            });
//
//            services.AddTransient<ICachingService, CachingService>();
//
//            services.AddTransient<ILocationsManager, LocationsManager>();
//            services.AddTransient<IPredictionsManager, PredictionsManager>();
//
//            services.AddTransient<INextBusClient, NextBusClient>();
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

                app.UseTelegramBotLongPolling<BusVbot>(ConfigureBot(), startAfter: TimeSpan.FromSeconds(2));
            }
            else
            {
                app.UseTelegramBotWebhook<BusVbot>(ConfigureBot());
                app.EnsureWebhookSet<BusVbot>();
            }

            app.Run(async context => { await context.Response.WriteAsync("Hello World!"); });
        }

        private IBotBuilder ConfigureBot() => // .Use<ExceptionHandler>() // ToDo
            new BotBuilder()
                // ignore channel posts
                .MapWhen<ChannelMessageHandler>(When.ChannelPost)
                // global commands. these don't require loading the user profile
                .MapWhen(When.NewCommand, branch => branch
                    .UseCommand<HelpCommand>("help")
                    .UseCommand<StartCommand>("start")
                )
        /*
                // ensure the user has a profile loaded for the rest of the handlers
                .Use<UserProfileSetupHandler>()
                // for new messages...
                .MapWhen(When.NewMessage, msgBranch => msgBranch
                    // accept locations as a location or a text coordinates(OSM)
                    .MapWhen<LocationHandler>(When.LocationOrCoordinates)
                    // for new text messages...
                    .MapWhen(When.NewTextMessage, txtBranch => txtBranch
                        // handle new commands
                        .MapWhen(When.NewCommand, cmdBranch => cmdBranch
                            .UseCommand<BusCommand>("bus")
                            .UseCommand<SaveCommand>("save")
                            .UseCommand<DeleteCommand>("del")
                        )
                        // try to handle a frequent location (from reply markup keyboard)
                        .UseWhen<SavedLocationHandler>(When.HasSavedLocationPrefix)
                    )
                )
                // for callback queries...
                .MapWhen(When.CallbackQuery, cqBranch => cqBranch
                    .MapWhen<BusDirectionCallbackQueryHandler>(When.IsBusDirectionCq)
                    .MapWhen<PredictionRefreshCqHandler>(When.IsBusPredictionCq)
                )
            */;
    }
}