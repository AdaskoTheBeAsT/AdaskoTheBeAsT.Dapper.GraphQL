using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.QueryBuilders
{
    public static class QueryBuilderHelper
    {
        public static IDictionary<GraphQLName, GraphQLField> CollectFields(GraphQLSelectionSet? selectionSet)
        {
            return CollectFields(selectionSet, new Dictionary<GraphQLName, GraphQLField>());
        }

        public static IDictionary<GraphQLName, GraphQLField> CollectFields(GraphQLSelectionSet? selectionSet, IDictionary<GraphQLName, GraphQLField> fields)
        {
            var skipList = new List<string> { "edges", "node", "cursor" };
            selectionSet?.Selections.ForEach(selection =>
            {
                if (selection is not GraphQLField field)
                {
                    return;
                }

                if (!skipList.Exists(name => name.Equals(field.Name.StringValue, StringComparison.OrdinalIgnoreCase)))
                {
                    fields[field.Name] = field;
                }

                CollectFields(field.SelectionSet, fields);
            });

            return fields;
        }

        public static bool IsConnection(GraphQLSelectionSet? selectionSet)
        {
            return IsConnection(selectionSet, new Dictionary<GraphQLName, GraphQLField>());
        }

        public static bool IsConnection(GraphQLSelectionSet? selectionSet, IDictionary<GraphQLName, GraphQLField> fields)
        {
            selectionSet?.Selections.ForEach(selection =>
            {
                if (selection is not GraphQLField field)
                {
                    return;
                }

                if (field.Name.StringValue.Equals("edges", StringComparison.OrdinalIgnoreCase))
                {
                    fields.Add(field.Name, field);
                }

                IsConnection(field.SelectionSet, fields);
            });

            return fields.Any();
        }
    }
}
