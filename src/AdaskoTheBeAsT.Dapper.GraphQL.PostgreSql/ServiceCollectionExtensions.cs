using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperGraphQlPostgreSql(
        this IServiceCollection serviceCollection,
        Action<DapperGraphQlOptions> setup)
    {
        serviceCollection.AddSingleton<PostgreSqlSqlBuilderOptions>();
        var options = new DapperGraphQlOptions(serviceCollection);
        setup?.Invoke(options);
        return serviceCollection;
    }
}
