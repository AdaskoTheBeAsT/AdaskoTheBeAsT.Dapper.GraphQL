using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Extensions;

public static class SqlInsertContextSqliteExtensions
{
    public static int ExecuteWithSqliteIdentity(this SqlInsertContext context, IDbConnection dbConnection)
    {
        var sb = BuildSqliteIdentityQuery(context);

        return dbConnection
            .Query<int>(sb.ToString(), context.Parameters)
            .Single();
    }

    public static async Task<int> ExecuteWithSqliteIdentityAsync(this SqlInsertContext context, IDbConnection dbConnection)
    {
        var sb = BuildSqliteIdentityQuery(context);

        var task = dbConnection
            .QueryAsync<int>(sb.ToString(), context.Parameters);
        return (await task.ConfigureAwait(false)).Single();
    }

    private static StringBuilder BuildSqliteIdentityQuery(SqlInsertContext context)
    {
        var sb = new StringBuilder();

        sb.Append(context).AppendLine().AppendLine("SELECT last_insert_rowid();");

        return sb;
    }
}
