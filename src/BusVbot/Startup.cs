using System;
using System.Net;
using System.Reflection;
using BusVbot.Bot;
using BusVbot.Configurations;
using BusVbot.Data;
using BusVbot.Handlers;
using BusVbot.Handlers.Commands;
using BusVbot.Models;
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
using RecurrentTasks;
using Telegram.Bot.Framework;

namespace BusVbot
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = BuildConfiguration(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            {
                var connStr = _configuration["ConnectionString"];
                var migrationsAssembly = typeof(BusVbotDbContext).GetTypeInfo().Assembly.GetName().Name;

                services.AddDbContext<BusVbotDbContext>(builder =>
                    builder.UseNpgsql(connStr, options => options
                            .MigrationsAssembly(migrationsAssembly))
                        .EnableSensitiveDataLogging());
            }

            services.AddTelegramBot<Bot.BusVbot>(_configuration)
                /* Ignore channel messages: */
                .AddUpdateHandler<ChannelMessageHandler>()
                /* Global commands: */
                .AddUpdateHandler<StartCommand>()
                .AddUpdateHandler<HelpCommand>()
                /* Make sure user has a profile: */
                .AddUpdateHandler<UserProfileSetupHandler>()
                /* Other handlers: */
                .AddUpdateHandler<BusCommand>()
                .AddUpdateHandler<BusDirectionCallbackQueryHandler>()
                .AddUpdateHandler<LocationHanlder>()
                .AddUpdateHandler<SaveCommand>()
                .AddUpdateHandler<SavedLocationHandler>()
                .AddUpdateHandler<DeleteCommand>()
                .Configure();

            services.AddTask<BotUpdateGetterTask<Bot.BusVbot>>();

            services.Configure<AgencyTimeZonesAccessor>(_configuration.GetSection("Agencies"));

            // ToDo Use extension method and chained methods for this
            services.AddTransient<IDefaultAgencyDataParser, DefaultAgencyDataParser>();
            services.AddTransient<IDefaultAgencyMessageFormatter, DefaultAgencyMessageFormatter>();
            services.AddTransient<TtcDataParser>();
            services.AddTransient<TtcMessageFormatter>();
            services.AddTransient<IAgencyServiceAccessor, AgencyServiceAccessor>(factory =>
            {
                var parsers = new IAgencyDataParser[] {factory.GetRequiredService<TtcDataParser>()};
                var formatters = new IAgencyMessageFormatter[] {factory.GetRequiredService<TtcMessageFormatter>()};
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
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.EnsureDatabaseSeededAsync(false).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.StartTask<BotUpdateGetterTask<Bot.BusVbot>>(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2));
            }
            else
            {
                app.UseExceptionHandler(builder =>
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        context.Response.ContentLength = 0;
                        await context.Response.WriteAsync(string.Empty)
                            .ConfigureAwait(false);
                    }));

                app.UseTelegramBotWebhook<Bot.BusVbot>();
            }
        }

        public static IConfigurationBuilder BuildConfiguration(string contentRoot) =>
            new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddEnvironmentVariables("BusVbot_")
                .AddJsonFile("appsettings.json");
    }
}