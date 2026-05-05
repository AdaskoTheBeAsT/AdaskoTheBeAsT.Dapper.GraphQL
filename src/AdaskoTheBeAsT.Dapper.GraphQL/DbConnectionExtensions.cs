using System.Data.Common;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public static class DbConnectionExtensions
    {
        public static IDapperGraphQlConnection WithDapperGraphQlOptions(
            this DbConnection connection,
            SqlBuilderOptions options)
        {
            return new DapperGraphQlConnection(connection, options);
        }
    }
}
