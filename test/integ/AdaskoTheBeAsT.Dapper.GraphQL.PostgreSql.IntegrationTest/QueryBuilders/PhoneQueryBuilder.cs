using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders
{
    public class PhoneQueryBuilder :
        IQueryBuilder<Phone>
    {
        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
        {
            query.Select($"{alias}.Id");
            query.SplitOn<Phone>("Id");

            var fields = context.GetSelectedFields();
            foreach (var kvp in fields)
            {
                switch (kvp.Key.StringValue)
                {
                    case "number":
                        query.Select($"{alias}.Number");
                        break;
                    case "type":
                        query.Select($"{alias}.Type");
                        break;
                }
            }

            return query;
        }
    }
}
