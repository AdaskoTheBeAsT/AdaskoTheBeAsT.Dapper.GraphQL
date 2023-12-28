using System;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Builders;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;

public static class IPersonRepositoryExtensions
{
    public static SqlQueryContext GetQuery(
        this IPersonRepository personRepository,
        IResolveConnectionContext<object?>? context,
        IQueryBuilder<Person> personQueryBuilder,
        string sWhere = "")
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

#pragma warning disable CC0021 // Use nameof
        const string alias = "Person";
#pragma warning restore CC0021 // Use nameof

        var query = SqlBuilder
            .From<Person>(alias)
            .OrderBy($"{alias}.CreateDate");

        query = !string.IsNullOrEmpty(sWhere) ? query.Where(sWhere) : query;

        return personQueryBuilder.Build(query, context.FieldAst, alias);
    }
}
