using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public class GraphQLInsertTests
        : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GraphQLInsertTests(
            TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Simple person insert should succeed")]
        public async Task SimplePersonInsertAsync()
        {
            var graphQuery = new GraphQlQuery
            {
                OperationName = "addPerson",
                Variables = JObject.Parse(@"{""person"":{""firstName"":""Joe"",""lastName"":""Doe""}}"),
                Query = @"
mutation ($person: PersonInput!) {
  addPerson(person: $person) {
    firstName
    lastName
  }
}",
            };

            var json = await _fixture.QueryGraphQlAsync(graphQuery);

            var expectedJson = @"
            {
                data: {
                    addPerson: {
                        firstName: 'Joe',
                        lastName: 'Doe'
                    }
                }
            }";

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
        }
    }
}
