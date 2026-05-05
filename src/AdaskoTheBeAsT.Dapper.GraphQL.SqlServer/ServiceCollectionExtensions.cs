using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperGraphQlSqlServer(
            this IServiceCollection serviceCollection,
            Action<DapperGraphQlOptions> setup)
        {
            serviceCollection.AddSingleton<SqlServerSqlBuilderOptions>();
            var options = new DapperGraphQlOptions(serviceCollection);
            setup?.Invoke(options);
            return serviceCollection;
        }
    }
}
