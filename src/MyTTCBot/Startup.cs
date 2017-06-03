using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyTTCBot.Bot;
using MyTTCBot.Commands;
using MyTTCBot.Services;
using NetTelegram.Bot.Framework;
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
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTelegramBot<MyTtcBot>(_configuration)
                .AddUpdateHandler<BusCommand>()
                .AddUpdateHandler<LocationHanlder>()
                .AddUpdateHandler<HelpCommand>()
                .AddUpdateHandler<StartCommand>()
                .Configure();

            services.AddTask<BotUpdateGetterTask<MyTtcBot>>();

            services.AddTransient<INextBusService, NextBusService>();

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

                app.StartTask<BotUpdateGetterTask<MyTtcBot>>(TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(3));
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
