using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL
{
    public class PersonQuery :
        ObjectGraphType
    {
        private const int MaxPageSize = 10;
        private readonly IPersonRepository _personRepository;

        public PersonQuery(
            IQueryBuilder<Person> personQueryBuilder,
            IServiceProvider serviceProvider,
            IPersonRepository personRepository)
        {
            _personRepository = personRepository;

            Field<ListGraphType<PersonType>>("people")
                .Description("A list of people.")
                .Resolve(context =>
                    {
                        var alias = "person";
                        var query = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                            .From<Person>(alias)
                            .OrderBy($"{alias}.Id");
                        query = personQueryBuilder.Build(query, context.FieldAst, alias);

                        // Create a mapper that understands how to uniquely identify the 'Person' class,
                        // and will deduplicate people as they pass through it
                        var personMapper = new PersonEntityMapper();

                        using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                        {
                            var results = query
                                .Execute(connection, context.FieldAst, personMapper)
                                .Distinct();
                            return results;
                        }
                    });

            Field<ListGraphType<PersonType>>("peopleAsync")
                .Description("A list of people fetched asynchronously.")
                .ResolveAsync(async context =>
                {
                    var alias = "person";
                    var query = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From($"Person {alias}")
                        .OrderBy($"{alias}.Id");
                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class,
                    // and will deduplicate people as they pass through it
                    var personMapper = new PersonEntityMapper();

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        connection.Open();

                        var results = await query.ExecuteAsync(connection, context.FieldAst, personMapper);
                        return results.Distinct();
                    }
                });

            Field<PersonType>("person")
                .Description("Gets a person by ID.")
                .Arguments(new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id", Description = "The ID of the person." }))
                .Resolve(context =>
                {
                    var id = context.Arguments["id"].Value;
                    var alias = "person";
                    var query = AdaskoTheBeAsT.Dapper.GraphQL.SqlBuilder
                        .From($"Person {alias}")
                        .Where($"{alias}.Id = @id", new { id })

                        // Even though we're only getting a single person, the process of deduplication
                        // may return several entities, so we sort by ID here for consistency
                        // with test results.
                        .OrderBy($"{alias}.Id");

                    query = personQueryBuilder.Build(query, context.FieldAst, alias);

                    // Create a mapper that understands how to uniquely identify the 'Person' class,
                    // and will deduplicate people as they pass through it
                    var personMapper = new PersonEntityMapper();

                    using (var connection = serviceProvider.GetRequiredService<IDbConnection>())
                    {
                        var results = query
                            .Execute(connection, context.FieldAst, personMapper)
                            .Distinct();
                        return results.FirstOrDefault();
                    }
                });

            Connection<PersonType>()
                .Name("personConnection")
                .Description("Gets pages of Person objects.")
                // Enable the last and before arguments to do paging in reverse.
                .Bidirectional()
                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .PageSize(MaxPageSize)
                .ResolveAsync(context => ResolveConnection(context, personQueryBuilder));
        }

        private async Task<object> ResolveConnection(ResolveConnectionContext<object> context, IQueryBuilder<Person> personQueryBuilder)
        {
            _personRepository.Context = context;

            var first = context.First;
            var afterCursor = Cursor.FromCursor<DateTime?>(context.After);
            var last = context.Last;
            var beforeCursor = Cursor.FromCursor<DateTime?>(context.Before);
            var cancellationToken = context.CancellationToken;

            var getPersonTask = GetPeople(first, afterCursor, last, beforeCursor, cancellationToken);
            var getHasNextPageTask = GetHasNextPage(first, afterCursor, cancellationToken);
            var getHasPreviousPageTask = GetHasPreviousPage(last, beforeCursor, cancellationToken);
            var totalCountTask = _personRepository.GetTotalCount(cancellationToken);

            await Task.WhenAll(getPersonTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);
            var people = getPersonTask.Result;
            var hasNextPage = getHasNextPageTask.Result;
            var hasPreviousPage = getHasPreviousPageTask.Result;
            var totalCount = totalCountTask.Result;
            var (firstCursor, lastCursor) = Cursor.GetFirstAndLastCursor(people, x => x.CreateDate);

            return new Connection<Person>()
            {
                Edges = people
                    .Select(x =>
                        new Edge<Person>()
                        {
                            Cursor = Cursor.ToCursor(x.CreateDate),
                            Node = x
                        })
                    .ToList(),
                PageInfo = new PageInfo()
                {
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage,
                    StartCursor = firstCursor,
                    EndCursor = lastCursor,
                },
                TotalCount = totalCount,
            };
        }

        private async Task<bool> GetHasNextPage(
            int? first,
            DateTime? afterCursor,
            CancellationToken cancellationToken)
        {
            return first.HasValue ? await _personRepository.GetHasNextPage(first, afterCursor, cancellationToken) : false;
        }

        private async Task<bool> GetHasPreviousPage(
            int? last,
            DateTime? beforeCursor,
            CancellationToken cancellationToken)
        {
            return last.HasValue ? await _personRepository.GetHasPreviousPage(last, beforeCursor, cancellationToken) : false;
        }

        private Task<List<Person>> GetPeople(
           int? first,
           DateTime? afterCursor,
           int? last,
           DateTime? beforeCursor,
           CancellationToken cancellationToken)
        {
            return first.HasValue ? _personRepository.GetPeople(first, afterCursor, cancellationToken) :
                                    _personRepository.GetPeopleReversed(last, beforeCursor, cancellationToken);
        }
    }
}
