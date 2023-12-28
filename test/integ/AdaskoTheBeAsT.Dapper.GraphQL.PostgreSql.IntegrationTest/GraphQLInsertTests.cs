using System.Threading.Tasks;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.GraphQL;
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
        public async Task SimplePersonInsert()
        {
            var graphQuery = new GraphQlQuery();
            graphQuery.OperationName = "addPerson";
            graphQuery.Variables = JObject.Parse(@"{""person"":{""firstName"":""Joe"",""lastName"":""Doe""}}");

            graphQuery.Query = @"
mutation ($person: PersonInput!) {
  addPerson(person: $person) {
    firstName
    lastName
  }
}";

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

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }
    }
}
