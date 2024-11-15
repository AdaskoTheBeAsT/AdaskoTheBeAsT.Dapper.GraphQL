using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Extensions
{
    public static class HasSelectionSetNodeExtensions
    {
        /// <summary>
        /// Returns a map of selected fields, keyed by the field name.
        /// </summary>
        /// <param name="selectionSet">The GraphQL selection set container.</param>
        /// <returns>A dictionary whose key is the field name, and value is the field contents.</returns>
        public static IDictionary<GraphQLName, GraphQLField>? GetSelectedFields(this IHasSelectionSetNode? selectionSet)
        {
            var fields = selectionSet?
                .SelectionSet?
                .Selections
                .OfType<GraphQLField>()
                .ToDictionary(field => field.Name);

            return fields;
        }

        /// <summary>
        /// Returns the inline fragment for the specified entity within the GraphQL selection.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to retrieve.</typeparam>
        /// <param name="selectionSet">The GraphQL selection set.</param>
        /// <returns>The inline fragment associated with the entity.</returns>
        public static GraphQLInlineFragment? GetInlineFragment<TEntityType>(this IHasSelectionSetNode selectionSet)
        {
            return selectionSet
                .SelectionSet?
                .Selections
                .OfType<GraphQLInlineFragment>()
                .FirstOrDefault(
                    f => f.TypeCondition?.Type.Name.StringValue.Equals(
                        typeof(TEntityType).Name,
                        StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}
