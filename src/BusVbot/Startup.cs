using BusVbot.Configurations;
using BusVbot.Data;
using BusVbot.Handlers;
using BusVbot.Handlers.Commands;
using BusVbot.Models;
using BusVbot.Options;
using BusVbot.Services;
using BusVbot.Services.Agency;
using BusVbot.Services.Agency.TTC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBus.NET;
using System;
using System.Reflection;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace BusVbot
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
            {
                var connStr = Configuration["ConnectionString"];
                var migrationsAssembly = typeof(BusVbotDbContext).GetTypeInfo().Assembly.GetName().Name;

                services.AddDbContext<BusVbotDbContext>(builder =>
                    builder.UseNpgsql(connStr, options => options
                            .MigrationsAssembly(migrationsAssembly))
                        .EnableSensitiveDataLogging());
            }

            services.AddTransient<Bot.BusVbot>()
                .Configure<BotOptions<Bot.BusVbot>>(Configuration.GetSection("Bot"))
                .Configure<CustomBotOptions<Bot.BusVbot>>(Configuration.GetSection("Bot"))
                .AddScoped<ChannelMessageHandler>()
                .AddScoped<StartCommand>()
                .AddScoped<HelpCommand>()
                .AddScoped<UserProfileSetupHandler>()
                .AddScoped<BusCommand>()
                .AddScoped<BusDirectionCallbackQueryHandler>()
                .AddScoped<PredictionRefreshCqHandler>()
                .AddScoped<LocationHandler>()
                .AddScoped<SaveCommand>()
                .AddScoped<SavedLocationHandler>()
                .AddScoped<DeleteCommand>()
            ;

            services.Configure<AgencyTimeZonesAccessor>(Configuration.GetSection("Agencies"));

            // ToDo Use extension method and chained methods for this
            services.AddTransient<IDefaultAgencyDataParser, DefaultAgencyDataParser>();
            services.AddTransient<IDefaultAgencyMessageFormatter, DefaultAgencyMessageFormatter>();
            services.AddTransient<TtcDataParser>();
            services.AddTransient<TtcMessageFormatter>();
            services.AddTransient<IAgencyServiceAccessor, AgencyServiceAccessor>(factory =>
            {
                var parsers = new IAgencyDataParser[] { factory.GetRequiredService<TtcDataParser>() };
                var formatters = new IAgencyMessageFormatter[] { factory.GetRequiredService<TtcMessageFormatter>() };
                return new AgencyServiceAccessor(
                    factory.GetRequiredService<IDefaultAgencyDataParser>(),
                    factory.GetRequiredService<IDefaultAgencyMessageFormatter>(),
                    parsers,
                    formatters
                );
            });

            services.AddTransient<ICachingService, CachingService>();

            services.AddTransient<ILocationsManager, LocationsManager>();
            services.AddTransient<IPredictionsManager, PredictionsManager>();

            services.AddTransient<INextBusClient, NextBusClient>();
            services.AddTransient<UserContextManager>();

            services.AddMemoryCache();

            services.AddScoped<DataSeeder>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.EnsureDatabaseSeededAsync(false).GetAwaiter().GetResult();

                // get bot updates from Telegram via long-polling approach during development
                // this will disable Telegram webhooks
                app.UseTelegramBotLongPolling<Bot.BusVbot>(ConfigureBot(), startAfter: TimeSpan.FromSeconds(2));
            }
            else
            {
                // use Telegram bot webhook middleware in higher environments
                app.UseTelegramBotWebhook<Bot.BusVbot>(ConfigureBot());
                // and make sure webhook is enabled
                app.EnsureWebhookSet<Bot.BusVbot>();
            }

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        //services.AddTelegramBot<Bot.BusVbot>(_configuration)
        //    /* Ignore channel messages: */
        //    .AddUpdateHandler<ChannelMessageHandler>()
        //    /* Global commands: */
        //    .AddUpdateHandler<StartCommand>()
        //    .AddUpdateHandler<HelpCommand>()
        //    /* Make sure user has a profile: */
        //    .AddUpdateHandler<UserProfileSetupHandler>()
        //    /* Other handlers: */
        //    .AddUpdateHandler<BusCommand>()
        //    .AddUpdateHandler<BusDirectionCallbackQueryHandler>()
        //    .AddUpdateHandler<PredictionRefreshCqHandler>()
        //    .AddUpdateHandler<LocationHandler>()
        //    .AddUpdateHandler<SaveCommand>()
        //    .AddUpdateHandler<SavedLocationHandler>()
        //    .AddUpdateHandler<DeleteCommand>()
        //    .Configure();

        //services.AddTask<BotUpdateGetterTask<Bot.BusVbot>>();

        private IBotBuilder ConfigureBot() => // .Use<ExceptionHandler>() // ToDo
            new BotBuilder()
                // ignore channel posts
                .MapWhen<ChannelMessageHandler>(When.ChannelPost)
                // global commands. these don't require loading the user profile
                .MapWhen(When.NewCommand, branch => branch
                    .UseCommand<HelpCommand>("help")
                    .UseCommand<StartCommand>("start")
                )
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
            ;
    }
}