using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NASABot.Dialogs;
using NASABot.Services;

namespace NASABot
{
    public class Startup
    {
        private IConfiguration configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
            configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(configuration);
            services.AddBot<NASABot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(configuration);

                IStorage dataStorage = new MemoryStorage(null);
                var userState = new UserState(dataStorage);
                options.State.Add(userState);

                var conversationState = new ConversationState(dataStorage);
                options.State.Add(conversationState);
            });

            // Create and register state accesssors.
            // Acessors created here are passed into the IBot-derived class on every turn.
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                var userState = options.State.OfType<UserState>().FirstOrDefault();
                if (userState == null)
                {
                    throw new InvalidOperationException("UserState must be defined and added before adding user-scoped state accessors.");
                }

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new WelcomeUserStateAccessors(userState)
                {
                    DidBotWelcomeUser = userState.CreateProperty<bool>("DidBotWelcomeState"),
                };

                return accessors;
            });

            services.AddSingleton(q =>
            {
                var options = q.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                    throw new InvalidOperationException("Conversation State must be defined");

                var conversationAccessor = new MultiTurnPromptsAccessor(conversationState)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    UserProfile = options.State.OfType<UserState>().FirstOrDefault().CreateProperty<UserProfile>("UserProfile")
                };

                return conversationAccessor;
            });

            services.AddSingleton<IDataService, DataService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseBotFramework();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
