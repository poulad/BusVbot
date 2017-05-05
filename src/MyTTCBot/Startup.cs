using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyTTCBot.Commands;
using MyTTCBot.Controllers;
using MyTTCBot.Managers;
using MyTTCBot.Services;
using NetTelegramBotApi;

namespace MyTTCBot
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables("MyTTCBot_")
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => new TelegramBot(_configuration["ApiToken"]));
            services.AddTransient<IBotManager, BotManager>();
            services.AddSingleton<IBotUpdatesService, BotUpdatesService>();
            services.AddTransient<IBusCommand, BusCommand>();
            services.AddTransient<INextBusService, NextBusService>();
            services.AddMemoryCache();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var botName = _configuration["BotName"];
            var apiToken = _configuration["ApiToken"];
            var useWebHook = bool.Parse(_configuration["UseWebHook"]);

            var webhookRoute = $"{botName.ToLower()}/{apiToken}";

            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var logger = loggerFactory.CreateLogger(nameof(Startup));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(builder =>
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentLength = 0;
                        await context.Response.WriteAsync(string.Empty).ConfigureAwait(false);
                    }));
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(botName, webhookRoute + "/{action}",
                    new
                    {
                        Controller = nameof(BotController).Replace("Controller", ""),
                        Action = nameof(BotController.ProcessUpdate)
                    });
            });

            var bot = app.ApplicationServices.GetRequiredService<TelegramBot>();
            if (useWebHook)
            {
                logger.LogInformation("Setting webhook");
                var result = bot.MakeRequestAsync(new NetTelegramBotApi.Requests.SetWebhook(webhookRoute))
                    .Result;
                if (result)
                    logger.LogInformation("Webhook set successfully");
                else
                    logger.LogError("Unable to set webhook");
            }
            else
            {
                logger.LogInformation("Disabling webhook");
                var result = bot.MakeRequestAsync(new NetTelegramBotApi.Requests.SetWebhook(string.Empty))
                    .Result;

                if (result)
                    logger.LogInformation("Webhook is disabled");
                else
                    logger.LogError("Unable to disable webhook");

                logger.LogInformation("Starting update polling service");
                app.ApplicationServices.GetRequiredService<IBotUpdatesService>().Start();
                logger.LogInformation("Update polling service is started");
            }
        }
    }
}
