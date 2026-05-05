using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperGraphQlSqlite(
            this IServiceCollection serviceCollection,
            Action<DapperGraphQlOptions> setup)
        {
            serviceCollection.AddSingleton<SqliteSqlBuilderOptions>();
            var options = new DapperGraphQlOptions(serviceCollection);
            setup?.Invoke(options);
            return serviceCollection;
        }
    }
}
