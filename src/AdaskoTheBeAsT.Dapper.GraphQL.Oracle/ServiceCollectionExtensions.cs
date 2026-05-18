using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperGraphQlOracle(
        this IServiceCollection serviceCollection,
        Action<DapperGraphQlOptions> setup)
    {
        serviceCollection.AddSingleton<OracleSqlBuilderOptions>();
        var options = new DapperGraphQlOptions(serviceCollection);
        setup?.Invoke(options);
        return serviceCollection;
    }
}
