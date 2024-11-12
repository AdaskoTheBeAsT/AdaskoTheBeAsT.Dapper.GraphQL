using System;
using System.Linq;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders
{
    public class EmailQueryBuilder :
        IQueryBuilder<Email>
    {
        public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode context, string alias)
        {
            // Always get the ID of the email
            query.Select($"{alias}.Id");

            // Tell Dapper where the Email class begins (at the Id we just selected)
            query.SplitOn<Email>("Id");

            var fields = context.GetSelectedFields();
            if (fields == null)
            {
                return query;
            }

            if (fields.Keys.Any(k => k.StringValue.Equals("address", StringComparison.OrdinalIgnoreCase)))
            {
                query.Select($"{alias}.Address");
            }

            return query;
        }
    }
}
