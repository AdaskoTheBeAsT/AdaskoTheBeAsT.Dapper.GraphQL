using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions
{
    public static class OracleIdentity
    {
        public static TIdentityType NextIdentity<TEntityType, TIdentityType>(IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            ValidateExpression(identityNameSelector);
            var sql = BuildNextIdentitySql<TEntityType, TIdentityType>(identityNameSelector);
            return dbConnection.Query<TIdentityType>(sql).Single();
        }

        public static async Task<TIdentityType> NextIdentityAsync<TEntityType, TIdentityType>(IDbConnection dbConnection, Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
            where TEntityType : class
        {
            ValidateExpression(identityNameSelector);
            var sql = BuildNextIdentitySql<TEntityType, TIdentityType>(identityNameSelector);
            var result = await dbConnection.QueryAsync<TIdentityType>(sql).ConfigureAwait(false);
            return result.Single();
        }

        private static void ValidateExpression<TEntityType, TIdentityType>(Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
        {
            if (identityNameSelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Cannot execute an Oracle identity with an expression of type " + identityNameSelector.Body.NodeType);
            }
        }

        private static string BuildNextIdentitySql<TEntityType, TIdentityType>(Expression<Func<TEntityType, TIdentityType>> identityNameSelector)
        {
            var memberExpression = identityNameSelector.Body as MemberExpression;
            var sequenceName = string.Concat(
                typeof(TEntityType).Name.ToUpper(CultureInfo.InvariantCulture),
                "_",
                memberExpression?.Member.Name.ToUpper(CultureInfo.InvariantCulture),
                "_SEQ");

            var sb = new StringBuilder();
            sb.Append("SELECT ").Append(sequenceName).Append(".NEXTVAL FROM DUAL");
            return sb.ToString();
        }
    }
}
