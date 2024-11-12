using System;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Builders;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;

public static class PersonRepositoryExtensions
{
    public static SqlQueryContext GetQuery(
        this IPersonRepository personRepository,
        IResolveConnectionContext<object?>? context,
        IQueryBuilder<Person> personQueryBuilder,
        string sWhere = "")
    {
#pragma warning disable RCS1256 // Invalid argument null check
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
#pragma warning restore RCS1256 // Invalid argument null check

        const string alias = nameof(Person);

        var query = SqlBuilder
            .From<Person>(alias)
            .OrderBy($"{alias}.CreateDate");

        query = !string.IsNullOrEmpty(sWhere) ? query.Where(sWhere) : query;

        return personQueryBuilder.Build(query, context.FieldAst, alias);
    }
}
