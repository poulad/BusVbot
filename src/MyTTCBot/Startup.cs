using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyTTCBot.Controllers;
using NetTelegramBotApi;

namespace MyTTCBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        private readonly TelegramBot _bot;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables("MyTTCBot_")
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();

            _bot = new TelegramBot(Configuration["ApiToken"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider => _bot);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var botName = Configuration["BotName"];
            var apiToken = Configuration["ApiToken"];
            var webhookRoute = $"{botName.ToLower()}/{apiToken}";

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(botName, webhookRoute + "/{action}",
                    new
                    {
                        Controller = nameof(BotController).Replace("Controller", ""),
                        Action = nameof(BotController.RequestUpdates)
                    });
            });
        }
    }
}
