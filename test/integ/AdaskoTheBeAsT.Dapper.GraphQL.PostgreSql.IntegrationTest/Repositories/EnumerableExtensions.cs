using System;
using System.Collections.Generic;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> If<T>(this IEnumerable<T> enumerable, bool condition, Func<IEnumerable<T>, IEnumerable<T>>? action)
        {
            if (condition)
            {
                return action != null ? action(enumerable) : enumerable;
            }

            return enumerable;
        }
    }
}
