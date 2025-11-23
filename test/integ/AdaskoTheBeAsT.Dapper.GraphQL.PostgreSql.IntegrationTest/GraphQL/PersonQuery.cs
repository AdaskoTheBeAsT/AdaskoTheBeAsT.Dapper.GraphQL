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

#pragma warning disable MA0051 // Method is too long
        public PersonQuery(
            IQueryBuilder<Person> personQueryBuilder,
            IServiceProvider serviceProvider,
            IPersonRepository personRepository)
#pragma warning restore MA0051 // Method is too long
        {
            _personRepository = personRepository;

#pragma warning disable MA0056 // Do not call overridable members in constructor
            Field<ListGraphType<PersonType>>("people")
                .Description("A list of people.")
                .Resolve(context =>
                    {
                        var alias = "person";
                        var query = SqlBuilder
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
                    var query = SqlBuilder
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
                    var id = context!.Arguments!["id"].Value;
                    var alias = "person";
                    var query = SqlBuilder
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
#pragma warning restore MA0056 // Do not call overridable members in constructor

            Connection<PersonType>("personConnection")
                .Description("Gets pages of Person objects.")

                // Enable the last and before arguments to do paging in reverse.
                .Bidirectional()

                // Set the maximum size of a page, use .ReturnAll() to set no maximum size.
                .PageSize(MaxPageSize)
                .ResolveAsync(context => ResolveConnectionAsync(context));
        }

        private async Task<object?> ResolveConnectionAsync(IResolveConnectionContext<object?> context)
        {
            _personRepository.Context = context;

            var first = context.First;
#if NET6_0_OR_GREATER
            var afterCursor = Cursor.FromCursor<DateOnly?>(context.After);
#else
            var afterCursor = Cursor.FromCursor<DateTime?>(context.After);
#endif
            var last = context.Last;
#if NET6_0_OR_GREATER
            var beforeCursor = Cursor.FromCursor<DateOnly?>(context.Before);
#else
            var beforeCursor = Cursor.FromCursor<DateTime?>(context.Before);
#endif
            var cancellationToken = context.CancellationToken;

            var getPersonTask = GetPeopleAsync(first, afterCursor, last, beforeCursor, cancellationToken);
            var getHasNextPageTask = GetHasNextPageAsync(first, afterCursor, cancellationToken);
            var getHasPreviousPageTask = GetHasPreviousPageAsync(last, beforeCursor, cancellationToken);
            var totalCountTask = _personRepository.GetTotalCountAsync(cancellationToken);

            await Task.WhenAll(getPersonTask, getHasNextPageTask, getHasPreviousPageTask, totalCountTask);
#pragma warning disable VSTHRD103 // Call async methods when in an async method
#pragma warning disable AsyncifyVariable // Use Task Async
            var people = getPersonTask.Result;
            var hasNextPage = getHasNextPageTask.Result;
            var hasPreviousPage = getHasPreviousPageTask.Result;
            var totalCount = totalCountTask.Result;
#pragma warning restore AsyncifyVariable // Use Task Async
#pragma warning restore VSTHRD103 // Call async methods when in an async method
            var (firstCursor, lastCursor) = Cursor.GetFirstAndLastCursor(people, x => x.CreateDate);

            return new Connection<Person>
            {
                Edges = people
                    .Select(x =>
                        new Edge<Person>
                        {
                            Cursor = Cursor.ToCursor(x.CreateDate),
                            Node = x,
                        })
                    .ToList(),
                PageInfo = new PageInfo
                {
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage,
                    StartCursor = firstCursor,
                    EndCursor = lastCursor,
                },
                TotalCount = totalCount,
            };
        }

        private async Task<bool> GetHasNextPageAsync(
            int? first,
#if NET6_0_OR_GREATER
            DateOnly? afterCursor,
#else
            DateTime? afterCursor,
#endif
            CancellationToken cancellationToken)
        {
            return first.HasValue && await _personRepository.GetHasNextPageAsync(first, afterCursor, cancellationToken);
        }

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        private async Task<bool> GetHasPreviousPageAsync(
            int? last,
#if NET6_0_OR_GREATER
            DateOnly? beforeCursor,
#else
            DateTime? beforeCursor,
#endif
            CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            return last.HasValue && await _personRepository.GetHasPreviousPageAsync(last, beforeCursor, cancellationToken);
#endif
#if NET462_OR_GREATER
            return await Task.Run(() => false).ConfigureAwait(false);
#endif
        }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
#pragma warning restore S1172 // Unused method parameters should be removed
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage

#pragma warning disable S1172 // Unused method parameters should be removed
        private Task<IList<Person>> GetPeopleAsync(
           int? first,
#if NET6_0_OR_GREATER
           DateOnly? afterCursor,
#else
           DateTime? afterCursor,
#endif
           int? last,
#if NET6_0_OR_GREATER
           DateOnly? beforeCursor,
#else
           DateTime? beforeCursor,
#endif
           CancellationToken cancellationToken)
#pragma warning restore S1172 // Unused method parameters should be removed
        {
#if NET6_0_OR_GREATER
            return first.HasValue ? _personRepository.GetPeopleAsync(first, afterCursor, cancellationToken) :
                                    _personRepository.GetPeopleReversedAsync(last, beforeCursor, cancellationToken);
#endif

#if NET462_OR_GREATER
            return _personRepository.GetPeopleAsync(first, afterCursor, cancellationToken);
#endif
        }
    }
}
