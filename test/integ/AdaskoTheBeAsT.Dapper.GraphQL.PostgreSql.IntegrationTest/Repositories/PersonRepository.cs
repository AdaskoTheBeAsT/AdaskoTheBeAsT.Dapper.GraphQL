using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.EntityMappers;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IQueryBuilder<Person> _personQueryBuilder;
        private readonly PersonEntityMapper _personMapper = new PersonEntityMapper();
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

        public Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
        {
            if (Context == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var query = this.GetQuery(Context, _personQueryBuilder);

            using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
            {
                var results = query
                    .Execute(connection, Context.FieldAst, _personMapper)
                    .Distinct()
                    .Count();

                return Task.FromResult(results);
            }
        }

        public Task<IList<Person>> GetPeopleAsync(
            int? first,
            DateTime? createdAfter,
            CancellationToken cancellationToken)
        {
            if (Context == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var sWhere = createdAfter != null ? $"{_alias}.CreateDate > '{createdAfter}'" : string.Empty;
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
            {
                var results = query
                    .Execute(connection, Context.FieldAst, _personMapper)
                    .Distinct()
                    .If(first.HasValue, x => x.Take(first!.Value))
                    .ToList();

                return Task.FromResult(results as IList<Person>);
            }
        }

#if NET6_0_OR_GREATER
        public Task<IList<Person>> GetPeopleReversedAsync(
            int? last,
            DateTime? createdBefore,
            CancellationToken cancellationToken)
        {
            if (Context == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var sWhere = createdBefore != null ? $"{_alias}.CreateDate < @{createdBefore}" : string.Empty;
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
            {
                var results = query
                    .Execute(connection, Context.FieldAst, _personMapper)
                    .Distinct()
                    .If(last.HasValue, x => x.TakeLast(last ?? 0))
                    .ToList();

                return Task.FromResult(results as IList<Person>);
            }
        }
#endif

        public Task<bool> GetHasNextPageAsync(
            int? first,
            DateTime? createdAfter,
            CancellationToken cancellationToken)
        {
            if (Context == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var sWhere = createdAfter != null ? $"{_alias}.CreateDate > '{createdAfter}'" : string.Empty;
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
            {
                return Task.FromResult(
                    query
                        .Execute(connection, Context.FieldAst, _personMapper)
                        .Distinct()
                        .Skip(first ?? 0)
                        .Any());
            }
        }

#if NET6_0_OR_GREATER
        public Task<bool> GetHasPreviousPageAsync(
            int? last,
            DateTime? createdBefore,
            CancellationToken cancellationToken)
        {
            if (Context == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            var sWhere = createdBefore != null ? $"{_alias}.CreateDate < '{createdBefore}'" : string.Empty;
            var query = this.GetQuery(Context, _personQueryBuilder, sWhere);

            using (var connection = _serviceProvider.GetRequiredService<IDbConnection>())
            {
                return Task.FromResult(
                    query
                        .Execute(connection, Context.FieldAst, _personMapper)
                        .Distinct()
                        .SkipLast(last ?? 0)
                        .Any());
            }
        }
#endif
    }
}
