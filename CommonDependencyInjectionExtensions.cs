using Microsoft.Extensions.DependencyInjection;
using SlaytonNichols.Common.Infrastructure.MongoDb.Repositories;

namespace SlaytonNichols.Common;

public static class CommonDependencyInjectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
    }
}