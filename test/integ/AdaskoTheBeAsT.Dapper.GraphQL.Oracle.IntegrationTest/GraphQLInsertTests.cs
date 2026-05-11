using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest.GraphQL;
using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Oracle.IntegrationTest
{
    public class GraphQlInsertTests
        : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GraphQlInsertTests(
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

            const string expectedJson = @"
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
