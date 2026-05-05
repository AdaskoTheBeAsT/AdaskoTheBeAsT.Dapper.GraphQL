using System.Data;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public interface IDapperGraphQlConnection : IDbConnection
    {
        SqlBuilderOptions Options { get; }
    }
}
