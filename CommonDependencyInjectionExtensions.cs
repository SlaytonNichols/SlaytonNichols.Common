using Microsoft.Extensions.DependencyInjection;
using SlaytonNichols.Posts.Service.Infrastructure.MongoDb.Repositories;

namespace SlaytonNichols.Posts.Service;

public static class CommonDependencyInjectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
    }
}