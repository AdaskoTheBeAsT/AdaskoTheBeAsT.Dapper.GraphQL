using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperGraphQlMySql(
            this IServiceCollection serviceCollection,
            Action<DapperGraphQlOptions> setup)
        {
            serviceCollection.AddSingleton<MySqlSqlBuilderOptions>();
            var options = new DapperGraphQlOptions(serviceCollection);
            setup?.Invoke(options);
            return serviceCollection;
        }
    }
}
