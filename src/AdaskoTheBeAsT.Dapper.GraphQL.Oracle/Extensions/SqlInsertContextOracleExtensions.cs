using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions
{
    public static class SqlInsertContextOracleExtensions
    {
        public static TIdentityType ExecuteWithOracleIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            ValidateExpression(identityNameSelector);
            var memberExpression = identityNameSelector.Body as MemberExpression;
            var sb = BuildOracleIdentityQuery<TEntityType>(context, memberExpression?.Member.Name);

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<TIdentityType> ExecuteWithOracleIdentityAsync<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            ValidateExpression(identityNameSelector);
            var memberExpression = identityNameSelector.Body as MemberExpression;
            var sb = BuildOracleIdentityQuery<TEntityType>(context, memberExpression?.Member.Name);

            var result = await dbConnection.QueryAsync<TIdentityType>(sb.ToString(), context.Parameters).ConfigureAwait(false);
            return result.Single();
        }

        private static void ValidateExpression<TEntityType, TIdentityType>(Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute an Oracle identity with an expression of type " + identityNameSelector.Body.NodeType);
            }
        }

        private static StringBuilder BuildOracleIdentityQuery<TEntityType>(SqlInsertContext context, string? memberName)
        {
            var sequenceName = string.Concat(
                typeof(TEntityType).Name.ToUpper(CultureInfo.InvariantCulture),
                "_",
                memberName?.ToUpper(CultureInfo.InvariantCulture),
                "_SEQ");

            var sb = new StringBuilder();
            sb.Append(context).AppendLine();
            sb.Append("SELECT ").Append(sequenceName).AppendLine(".CURRVAL FROM DUAL");
            return sb;
        }
    }
}
