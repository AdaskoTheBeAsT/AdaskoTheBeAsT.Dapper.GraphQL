using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;
using GraphQL.Builders;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Repositories;

public interface IPersonRepository
{
    IResolveConnectionContext<object?>? Context { get; set; }

    Task<int> GetTotalCountAsync(CancellationToken cancellationToken);

    Task<IList<Person>> GetPeopleAsync(int? first, DateTime? createdAfter, CancellationToken cancellationToken);

#if NET6_0_OR_GREATER
    Task<IList<Person>> GetPeopleReversedAsync(int? last, DateTime? createdBefore, CancellationToken cancellationToken);
#endif

    Task<bool> GetHasNextPageAsync(int? first, DateTime? createdAfter, CancellationToken cancellationToken);

#if NET6_0_OR_GREATER
    Task<bool> GetHasPreviousPageAsync(int? last, DateTime? createdBefore, CancellationToken cancellationToken);
#endif
}
