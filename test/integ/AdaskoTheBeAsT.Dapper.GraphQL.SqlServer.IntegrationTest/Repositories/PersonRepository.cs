using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.Models;
using GraphQL.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.IntegrationTest.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly IQueryBuilder<Person> _personQueryBuilder;
#pragma warning disable CC0021 // Use nameof
    private readonly string _alias = "Person";
#pragma warning restore CC0021 // Use nameof
    private readonly IServiceProvider _serviceProvider;

    public PersonRepository(IQueryBuilder<Person> personQueryBuilder, IServiceProvider serviceProvider)
    {
        _personQueryBuilder = personQueryBuilder;
        _serviceProvider = serviceProvider;
    }

    public IResolveConnectionContext<object?>? Context { get; set; }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
    {
        if (Context == null)
        {
            throw new ArgumentNullException(string.Empty);
        }

        var query = this.GetQuery(Context, _personQueryBuilder);

        using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
        {
            var results = await query
                .ExecuteAsync(connection, Context.FieldAst, new PersonEntityMapper())
                .ConfigureAwait(false);

            return results.Distinct().Count();
        }
    }

    public async Task<IList<Person>> GetPeopleAsync(
        int? first,
#if NET6_0_OR_GREATER
        DateOnly? createdAfter,
#else
        DateTime? createdAfter,
#endif
        CancellationToken cancellationToken)
    {
        if (Context == null)
        {
            throw new ArgumentNullException(string.Empty);
        }

        var sWhere = BuildDateFilter(_alias, "CreateDate", ">", createdAfter);
        var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

        using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
        {
            var results = await query
                .ExecuteAsync(connection, Context.FieldAst, new PersonEntityMapper())
                .ConfigureAwait(false);
            var list = results
                .Distinct()
                .If(first.HasValue, x => x.Take(first!.Value))
                .ToList();

            return list;
        }
    }

    public async Task<IList<Person>> GetPeopleReversedAsync(
        int? last,
#if NET6_0_OR_GREATER
        DateOnly? createdBefore,
#else
        DateTime? createdBefore,
#endif
        CancellationToken cancellationToken)
    {
        if (Context == null)
        {
            throw new ArgumentNullException(string.Empty);
        }

        var sWhere = BuildDateFilter(_alias, "CreateDate", "<", createdBefore);
        var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

        using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
        {
            var results = await query
                .ExecuteAsync(connection, Context.FieldAst, new PersonEntityMapper())
                .ConfigureAwait(false);
            var list = results
                .Distinct()
                .If(last.HasValue, x => x.Reverse().Take(last ?? 0).Reverse())
                .ToList();

            return list;
        }
    }

    public async Task<bool> GetHasNextPageAsync(
        int? first,
#if NET6_0_OR_GREATER
        DateOnly? createdAfter,
#else
        DateTime? createdAfter,
#endif
        CancellationToken cancellationToken)
    {
        if (Context == null)
        {
            throw new ArgumentNullException(string.Empty);
        }

        var sWhere = BuildDateFilter(_alias, "CreateDate", ">", createdAfter);
        var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

        using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
        {
            var results = await query
                .ExecuteAsync(connection, Context.FieldAst, new PersonEntityMapper())
                .ConfigureAwait(false);
            return results
                .Distinct()
                .Skip(first ?? 0)
                .Any();
        }
    }

    public async Task<bool> GetHasPreviousPageAsync(
        int? last,
#if NET6_0_OR_GREATER
        DateOnly? createdBefore,
#else
        DateTime? createdBefore,
#endif
        CancellationToken cancellationToken)
    {
        if (Context == null)
        {
            throw new ArgumentNullException(string.Empty);
        }

        var sWhere = BuildDateFilter(_alias, "CreateDate", "<", createdBefore);
        var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

        using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
        {
            var results = await query
                .ExecuteAsync(connection, Context.FieldAst, new PersonEntityMapper())
                .ConfigureAwait(false);
            var items = results
                .Distinct()
                .ToList();

            return items.Count > (last ?? 0) &&
                   items.Reverse<Person>().Skip(last ?? 0).Any();
        }
    }

#if NET6_0_OR_GREATER
    private static string BuildDateFilter(string alias, string column, string op, DateOnly? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var formatted = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{alias}.{column} {op} '{formatted}'";
    }
#else
    private static string BuildDateFilter(string alias, string column, string op, DateTime? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var formatted = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{alias}.{column} {op} '{formatted}'";
    }
#endif
}
