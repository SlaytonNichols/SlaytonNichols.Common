using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;
using ServiceStack.Authentication.MongoDb;
using Microsoft.Extensions.Configuration;
using ServiceStack.Admin;
using SlaytonNichols.Common.ServiceStack.Auth;

namespace SlaytonNichols.Common.ServiceStack;

public static class Configure
{
    public static void ConfigureApplication(this IWebHostBuilder builder)
    {
        builder
        .ConfigureServices((context, services) =>
        {
            services.ConfigureNonBreakingSameSiteCookies(context.HostingEnvironment);
            //Mongo
            services.AddSingleton<IAuthRepository>(c =>
                new MongoDbAuthRepository(c.Resolve<IMongoDatabase>(), createMissingCollections: true));
            var mongoClient = new MongoClient(context.Configuration.GetConnectionString("Mongo"));
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase("SlaytonNichols");
            services.AddSingleton(mongoDatabase);
        })
        .ConfigureLogging(logginBuilder =>
        {
            logginBuilder.ClearProviders();
            //Console logging for Data Dog Agent
            logginBuilder.AddJsonConsole(jsonConsoleFormatterOptions =>
            {
                jsonConsoleFormatterOptions.JsonWriterOptions = new()
                {
                    Indented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
            });
        })
        .ConfigureAppHost(appHost =>
        {
            var appSettings = appHost.AppSettings;
            appHost.Plugins.Add(new AuthFeature(() => new CustomUserSession(),
                new IAuthProvider[] {
                    new CredentialsAuthProvider(appSettings),
                    new GoogleAuthProvider(appSettings),
                    new MicrosoftGraphAuthProvider(appSettings),
                    new GithubAuthProvider(appSettings)
                }));

            appHost.Plugins.Add(new RegistrationFeature());
            appHost.RegisterAs<CustomRegistrationValidator, IValidator<Register>>();
            var authRepo = appHost.Resolve<IAuthRepository>();
            authRepo.InitSchema();
            // appHost.Plugins.Add(new RequestLogsFeature
            // {
            //     EnableResponseTracking = true,
            // });

            // appHost.Plugins.Add(new ProfilingFeature
            // {
            //     IncludeStackTrace = true,
            // });
            appHost.Plugins.Add(new OpenApiFeature());
            appHost.Plugins.Add(new PostmanFeature());
            appHost.Plugins.Add(new SpaFeature
            {
                EnableSpaFallback = true
            });
            appHost.ConfigurePlugin<UiFeature>(feature => { });
            appHost.Plugins.Add(new AdminUsersFeature());

            appHost.Plugins.Add(new CorsFeature(allowOriginWhitelist: new[]
            {
                "http://localhost:5000",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:5176",
                "http://localhost:5177",
                "http://localhost:5178",
                "https://localhost:5001",
                "https://localhost:5003",
                "https://localhost:5005",
                "https://localhost:5007",
                "https://localhost:5009",
                "https://" + Environment.GetEnvironmentVariable("DEPLOY_CDN"),
                "https://" + Environment.GetEnvironmentVariable("DEPLOY_API")
            }, allowCredentials: true));

            appHost.SetConfig(new HostConfig { });
        }, afterConfigure: appHost =>
        {
            appHost.AssertPlugin<AuthFeature>().AuthEvents.Add(new AppUserAuthEvents());
        });
    }
}