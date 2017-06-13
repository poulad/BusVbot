using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyTTCBot.Bot;
using MyTTCBot.Handlers;
using MyTTCBot.Handlers.Commands;
using MyTTCBot.Models;
using MyTTCBot.Services;
using NetTelegram.Bot.Framework;
using NextBus.NET;
using RecurrentTasks;

namespace MyTTCBot
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables("MyTTCBot_")
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            {
                var connStr = _configuration["ConnectionString"];
                var migrationsAssembly = typeof(MyTtcDbContext).GetTypeInfo().Assembly.GetName().Name;

                services.AddDbContext<MyTtcDbContext>(builder =>
                    builder.UseNpgsql(connStr, options => options.MigrationsAssembly(migrationsAssembly)));
            }

            services.AddTelegramBot<MyTtcBot>(_configuration)
                .AddUpdateHandler<BusCommand>()
                .AddUpdateHandler<LocationHanlder>()
                .AddUpdateHandler<CallbackQueryHandler>()
                .AddUpdateHandler<SaveCommand>()
                .AddUpdateHandler<SavedLocationHandler>()
                .AddUpdateHandler<DeleteCommand>()
                .AddUpdateHandler<HelpCommand>()
                .AddUpdateHandler<StartCommand>()
                .Configure();

            services.AddTask<BotUpdateGetterTask<MyTtcBot>>();

            services.AddTransient<ILocationsManager, LocationsManager>();
            services.AddTransient<IPredictionsManager, PredictionsManager>();

            services.AddTransient<INextBusDataParser, NextBusDataParser>();
            services.AddTransient<INextBusHttpClient, NextBusHttpClient>();
            services.AddTransient<INextBusClient, NextBusClient>();
            services.AddTransient<ITtcBusService, TtcBusService>();

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                app.StartTask<BotUpdateGetterTask<MyTtcBot>>(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1));
            }
            else
            {
                app.UseExceptionHandler(builder =>
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentLength = 0;
                        await context.Response.WriteAsync(string.Empty)
                            .ConfigureAwait(false);
                    }));

                app.UseTelegramBotWebhook<MyTtcBot>(true);
            }
        }
    }
}
