using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Extensions
{
    public static class PostgreSql
    {
        public static TIdentityType NextIdentity<TEntityType, TIdentityType>(IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
            }

            var memberExpression = identityNameSelector.Body as MemberExpression;

            var sb = new StringBuilder();
            sb.AppendLine($"SELECT nextval(pg_get_serial_sequence('{typeof(TEntityType).Name.ToLower(CultureInfo.InvariantCulture)}', '{memberExpression?.Member.Name.ToLower(CultureInfo.InvariantCulture)}'));");

            return dbConnection
                .Query<TIdentityType>(sb.ToString())
                .Single();
        }

        public static async Task<TIdentityType> NextIdentityAsync<TEntityType, TIdentityType>(IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute a PostgreSQL identity with an expression of type " + identityNameSelector.Body.NodeType);
            }

            var memberExpression = identityNameSelector.Body as MemberExpression;

            var sb = new StringBuilder();
            sb.AppendLine($"SELECT nextval(pg_get_serial_sequence('{typeof(TEntityType).Name.ToLower(CultureInfo.InvariantCulture)}', '{memberExpression?.Member.Name.ToLower(CultureInfo.InvariantCulture)}'));");

            var result = await dbConnection.QueryAsync<TIdentityType>(sb.ToString()).ConfigureAwait(false);
            return result.Single();
        }
    }
}
