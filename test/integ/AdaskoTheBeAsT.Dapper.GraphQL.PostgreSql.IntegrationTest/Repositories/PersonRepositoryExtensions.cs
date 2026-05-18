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
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(personRepository);
        ArgumentNullException.ThrowIfNull(context);
#else
        if (personRepository == null)
        {
            throw new ArgumentNullException(nameof(personRepository));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
#endif

        const string alias = nameof(Person);

        var query = SqlBuilder
            .From<Person>(alias)
            .OrderBy($"{alias}.CreateDate");

        query = !string.IsNullOrEmpty(sWhere) ? query.Where(sWhere) : query;

        return personQueryBuilder.Build(query, context.FieldAst, alias);
    }
}
