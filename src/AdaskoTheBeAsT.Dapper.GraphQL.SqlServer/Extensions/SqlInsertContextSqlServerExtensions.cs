using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions;

public static class SqlInsertContextSqlServerExtensions
{
    public static TIdentityType ExecuteWithSqlServerIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection)
        where TEntityType : class
    {
        return ExecuteWithSqlServerIdentity<TIdentityType>(context, dbConnection);
    }

    public static TIdentityType ExecuteWithSqlServerIdentity<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
    {
        var sb = BuildSqlServerIdentityQuery<TIdentityType>(context);

        return dbConnection
            .Query<TIdentityType>(sb.ToString(), context.Parameters)
            .Single();
    }

    public static async Task<TIdentityType> ExecuteWithSqlServerIdentityAsync<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
    {
        var sb = BuildSqlServerIdentityQuery<TIdentityType>(context);

        var task = dbConnection
            .QueryAsync<TIdentityType>(sb.ToString(), context.Parameters);
        return (await task.ConfigureAwait(false)).Single();
    }

    private static StringBuilder BuildSqlServerIdentityQuery<TIdentityType>(SqlInsertContext context)
    {
        var sb = new StringBuilder();

        if (typeof(TIdentityType) == typeof(int))
        {
            sb.Append(context).AppendLine().AppendLine("SELECT CAST(SCOPE_IDENTITY() AS INT)");
        }
        else if (typeof(TIdentityType) == typeof(long))
        {
            sb.Append(context).AppendLine().AppendLine("SELECT CAST(SCOPE_IDENTITY() AS BIGINT)");
        }
        else
        {
            throw new InvalidCastException($"Type {typeof(TIdentityType).Name} is not supported in this SQL Server context.");
        }

        return sb;
    }
}
