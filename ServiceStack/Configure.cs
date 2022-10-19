using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;
using ServiceStack.Web;
using ServiceStack.Authentication.MongoDb;
using Microsoft.Extensions.Configuration;
using ServiceStack.Admin;

namespace SlaytonNichols.Common.ServiceStack;

public static class Configure
{
    public static void ConfigureApplication(this IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.ConfigureNonBreakingSameSiteCookies(context.HostingEnvironment);
        }).ConfigureLogging(logginBuilder =>
        {
            logginBuilder.ClearProviders();
            logginBuilder.AddJsonConsole(jsonConsoleFormatterOptions =>
            {
                jsonConsoleFormatterOptions.JsonWriterOptions = new()
                {
                    Indented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
            });
        });

        builder.ConfigureServices(services =>
        {
            //services.AddSingleton<ICacheClient>(new MemoryCacheClient()); //Store User Sessions in Memory Cache (default)
        }).ConfigureAppHost(appHost =>
            {
                var appSettings = appHost.AppSettings;
                appHost.Plugins.Add(new AuthFeature(() => new CustomUserSession(),
                    new IAuthProvider[] {
                        new CredentialsAuthProvider(appSettings),
                        new FacebookAuthProvider(appSettings),
                        new GoogleAuthProvider(appSettings),
                        new MicrosoftGraphAuthProvider(appSettings),
                    }));

                appHost.Plugins.Add(new RegistrationFeature());
                appHost.RegisterAs<CustomRegistrationValidator, IValidator<Register>>();
            });

        builder.ConfigureServices((context, services) => services.AddSingleton<IAuthRepository>(c =>
                new MongoDbAuthRepository(c.Resolve<IMongoDatabase>(), createMissingCollections: true)))
            .ConfigureAppHost(appHost =>
            {
                var authRepo = appHost.Resolve<IAuthRepository>();
                authRepo.InitSchema();
                // CreateUser(authRepo, "admin@email.com", "Admin User", "p@55wOrd", roles: new[] { RoleNames.Admin });
            }, afterConfigure: appHost =>
                appHost.AssertPlugin<AuthFeature>().AuthEvents.Add(new AppUserAuthEvents()));

        builder.ConfigureServices((context, services) =>
        {
            var mongoClient = new MongoClient(context.Configuration.GetConnectionString("Mongo"));
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase("SlaytonNichols");
            services.AddSingleton(mongoDatabase);
        });

        builder.ConfigureAppHost(host =>
        {
            // host.Plugins.Add(new RequestLogsFeature
            // {
            //     EnableResponseTracking = true,
            // });

            // host.Plugins.Add(new ProfilingFeature
            // {
            //     IncludeStackTrace = true,
            // });
        });

        builder.ConfigureAppHost(host =>
        {
            host.Plugins.Add(new OpenApiFeature());
            host.Plugins.Add(new PostmanFeature());
            host.SetConfig(new HostConfig
            {
            });
            host.Plugins.Add(new SpaFeature
            {
                EnableSpaFallback = true
            });
            host.ConfigurePlugin<UiFeature>(feature =>
            {
            });
            host.Plugins.Add(new AdminUsersFeature());

            host.Plugins.Add(new CorsFeature(allowOriginWhitelist: new[]{
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
        });
    }
}
public class CustomUserSession : AuthUserSession
{
}

public class CustomRegistrationValidator : RegistrationValidator
{
    public CustomRegistrationValidator()
    {
        RuleSet(ApplyTo.Post, () =>
        {
            RuleFor(x => x.DisplayName).NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotEmpty();
        });
    }
}

public class AppUser : UserAuth
{
}

public class AppUserAuthEvents : AuthEvents
{
    public override void OnAuthenticated(IRequest req, IAuthSession session, IServiceBase authService,
        IAuthTokens tokens, Dictionary<string, string> authInfo)
    {
        var authRepo = HostContext.AppHost.GetAuthRepository(req);
        using (authRepo as IDisposable)
        {
            var userAuth = authRepo.GetUserAuth(session.UserAuthId);
            authRepo.SaveUserAuth(userAuth);
        }
    }
}