using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.Extensions
{
    public static class SqlInsertContextMySqlExtensions
    {
        public static TIdentityType ExecuteWithMySqlIdentity<TEntityType, TIdentityType>(this SqlInsertContext<TEntityType> context, IDbConnection dbConnection)
            where TEntityType : class
        {
            return ExecuteWithMySqlIdentity<TIdentityType>(context, dbConnection);
        }

        public static TIdentityType ExecuteWithMySqlIdentity<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildMySqlIdentityQuery<TIdentityType>(context);

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<TIdentityType> ExecuteWithMySqlIdentityAsync<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildMySqlIdentityQuery<TIdentityType>(context);

            var task = dbConnection
                .QueryAsync<TIdentityType>(sb.ToString(), context.Parameters);
            return (await task.ConfigureAwait(false)).Single();
        }

        private static StringBuilder BuildMySqlIdentityQuery<TIdentityType>(SqlInsertContext context)
        {
            var sb = new StringBuilder();

            if (typeof(TIdentityType) == typeof(int))
            {
                sb.Append(context).AppendLine().AppendLine("SELECT CAST(LAST_INSERT_ID() AS SIGNED INTEGER);");
            }
            else if (typeof(TIdentityType) == typeof(long))
            {
                sb.Append(context).AppendLine().AppendLine("SELECT CAST(LAST_INSERT_ID() AS SIGNED);");
            }
            else if (typeof(TIdentityType) == typeof(ulong))
            {
                sb.Append(context).AppendLine().AppendLine("SELECT LAST_INSERT_ID();");
            }
            else
            {
                throw new InvalidCastException($"Type {typeof(TIdentityType).Name} in not supported in MySQL context.");
            }

            return sb;
        }
    }
}
