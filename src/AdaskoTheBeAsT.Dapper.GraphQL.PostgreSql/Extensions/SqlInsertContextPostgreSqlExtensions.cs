using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;

public static class SqlInsertContextPostgreSqlExtensions
{
    public static TIdentityType ExecuteWithPostgreSqlIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
        where TEntityType : class
    {
        if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
        {
            throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
        }

        var memberExpression = identityNameSelector.Body as MemberExpression;

        var sb = BuildPostgreSqlIdentityQuery(context, memberExpression?.Member.Name.ToLower(CultureInfo.InvariantCulture));

        return dbConnection
            .Query<TIdentityType>(sb.ToString(), context.Parameters)
            .Single();
    }

    public static async Task<TIdentityType> ExecuteWithPostgreSqlIdentityAsync<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
        where TEntityType : class
    {
        if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
        {
            throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
        }

        var memberExpression = identityNameSelector.Body as MemberExpression;

        var sb = BuildPostgreSqlIdentityQuery(context, memberExpression?.Member.Name.ToLower(CultureInfo.InvariantCulture));

        var result = await dbConnection.QueryAsync<TIdentityType>(sb.ToString(), context.Parameters).ConfigureAwait(false);
        return result.Single();
    }

    private static StringBuilder BuildPostgreSqlIdentityQuery(SqlInsertContext context, string? idName)
    {
        var sb = new StringBuilder();
        sb.Append(context)
            .AppendLine()
            .Append("SELECT currval(pg_get_serial_sequence('")
            .Append(context.Table.ToLower(CultureInfo.InvariantCulture))
            .Append("', '")
            .Append(idName?.ToLower(CultureInfo.InvariantCulture))
            .AppendLine("'));");
        return sb;
    }
}
