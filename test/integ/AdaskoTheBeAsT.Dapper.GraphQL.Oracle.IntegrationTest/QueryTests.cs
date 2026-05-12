using System;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.Models;
using AwesomeAssertions;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Sdk;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest
{
    public class QueryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public QueryTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "ORDER BY should work")]
        public void OrderByShouldWork()
        {
            var query = SqlBuilder
                .From("Person person")
                .Select("person.Id")
                .SplitOn<Person>("Id")
                .OrderBy("LastName");

            Assert.Contains("ORDER BY", query.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "SELECT without matching alias should throw")]
        public void SelectWithoutMatchingAliasShouldThrow()
        {
            var action = () =>
            {
                var query = SqlBuilder
                    .From("Person person")
                    .Select(new[] { "person.Id", "notAnAlias.Id" })
                    .SplitOn<Person>("Id");

                const string graphql = "{ person { id } }";
                var selectionSet = _fixture.BuildGraphQlSelection(graphql);
                if (selectionSet == null)
                {
                    throw new XunitException("Selection set is null");
                }

                using (var db = _fixture.GetDbConnection())
                {
                    query.Execute<Person>(db, selectionSet);
                }
            };

            action.Should().ThrowExactly<OracleException>();
        }
    }
}
