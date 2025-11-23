using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
{
    public class GraphQlTests
        : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GraphQlTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Full people query should succeed")]
#pragma warning disable MA0051 // Method is too long
        public async Task FullPeopleQueryAsync()
#pragma warning restore MA0051 // Method is too long
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    people {
        id
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
        companies {
            id
            name
        }
        supervisor {
            id
            firstName
            lastName
            emails {
                id
                address
            }
            phones {
                id
                number
                type
            }
        }
        careerCounselor {
            id
            firstName
            lastName
            emails {
                id
                address
            }
            phones {
                id
                number
                type
            }
        }
    }
}");

            var expectedJson = @"
{
    ""data"": {
        ""people"": [{
                ""id"": 1,
                ""firstName"": ""Hyrum"",
                ""lastName"": ""Clyde"",
                ""emails"": [{
                    ""id"": 1,
                    ""address"": ""hclyde@landmarkhw.com""
                }],
                ""phones"": [],
                ""companies"": [{
                    ""id"": 1,
                    ""name"": ""Landmark Home Warranty, LLC""
                }],
                ""supervisor"": null,
                ""careerCounselor"": null
            },
            {
                ""id"": 2,
                ""firstName"": ""Doug"",
                ""lastName"": ""Day"",
                ""emails"": [{
                        ""id"": 2,
                        ""address"": ""dday@landmarkhw.com""
                    },
                    {
                        ""id"": 3,
                        ""address"": ""dougrday@gmail.com""
                    }
                ],
                ""phones"": [{
                    ""id"": 1,
                    ""number"": ""8011234567"",
                    ""type"": ""Mobile""
                }],
                ""companies"": [{
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
                    },
                    {
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
                    }
                ],
                ""supervisor"": null,
                ""careerCounselor"": {
                    ""id"": 1,
                    ""firstName"": ""Hyrum"",
                    ""lastName"": ""Clyde"",
                    ""emails"": [{
                        ""id"": 1,
                        ""address"": ""hclyde@landmarkhw.com""
                    }],
                    ""phones"": []
                }
            },
            {
                ""id"": 3,
                ""firstName"": ""Kevin"",
                ""lastName"": ""Russon"",
                ""emails"": [{
                    ""id"": 4,
                    ""address"": ""krusson@landmarkhw.com""
                }],
                ""phones"": [{
                        ""id"": 2,
                        ""number"": ""8019876543"",
                        ""type"": ""Mobile""
                    },
                    {
                        ""id"": 3,
                        ""number"": ""8011111111"",
                        ""type"": ""Home""
                    }
                ],
                ""companies"": [{
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
                    },
                    {
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
                    }
                ],
                ""supervisor"": {
                    ""id"": 1,
                    ""firstName"": ""Hyrum"",
                    ""lastName"": ""Clyde"",
                    ""emails"": [{
                        ""id"": 1,
                        ""address"": ""hclyde@landmarkhw.com""
                    }],
                    ""phones"": []
                },
                ""careerCounselor"": {
                    ""id"": 2,
                    ""firstName"": ""Doug"",
                    ""lastName"": ""Day"",
                    ""emails"": [{
                            ""id"": 2,
                            ""address"": ""dday@landmarkhw.com""
                        },
                        {
                            ""id"": 3,
                            ""address"": ""dougrday@gmail.com""
                        }
                    ],
                    ""phones"": [{
                        ""id"": 1,
                        ""number"": ""8011234567"",
                        ""type"": ""Mobile""
                    }]
                }
            }
        ]
    }
}";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Async query should succeed")]
        public async Task PeopleAsyncQueryAsync()
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    peopleAsync {
        id
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
  ""data"": {
    ""peopleAsync"": [
      {
        ""id"": 1,
        ""firstName"": ""Hyrum"",
        ""lastName"": ""Clyde""
      },
      {
        ""id"": 2,
        ""firstName"": ""Doug"",
        ""lastName"": ""Day""
      },
      {
        ""id"": 3,
        ""firstName"": ""Kevin"",
        ""lastName"": ""Russon""
      }
    ]
  }
}";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Person query should succeed")]
        public async Task PersonQueryAsync()
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    person (id: 2) {
        id
        firstName
        lastName
        emails {
            id
            address
        }
        phones {
            id
            number
            type
        }
    }
}");

            var expectedJson = @"
{
    data: {
        person: {
            id: 2,
            firstName: 'Doug',
            lastName: 'Day',
            emails: [{
                id: 2,
                address: 'dday@landmarkhw.com'
            }, {
                id: 3,
                address: 'dougrday@gmail.com'
            }],
            phones: [{
                id: 1,
                number: '8011234567',
                type: ""Mobile""
            }]
        }
    }
}";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Simple people query should succeed")]
        public async Task SimplePeopleQueryAsync()
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    people {
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
  data: {
    people: [
      {
        firstName: 'Hyrum',
        lastName: 'Clyde'
      },
      {
        firstName: 'Doug',
        lastName: 'Day'
      },
      {
        firstName: 'Kevin',
        lastName: 'Russon'
      }
    ]
  }
}";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "Simple person query should succeed")]
        public async Task SimplePersonQueryAsync()
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    person (id: 2) {
        id
        firstName
        lastName
    }
}");

            var expectedJson = @"
{
    data: {
        person: {
            id: 2,
            firstName: 'Douglas',
            lastName: 'Day'
        }
    }
}";

            Assert.True(_fixture.JsonEquals(expectedJson, json));
        }

        [Fact(DisplayName = "People connection query should succeed")]
#pragma warning disable MA0051 // Method is too long
        public async Task PeopleConnectionQueryAsync()
#pragma warning restore MA0051 // Method is too long
        {
            var json = await _fixture.QueryGraphQlAsync(@"
query {
    personConnection(first:2) {
    edges {
    node {
            firstName
            lastName
        }
        cursor
    }
    pageInfo {
            hasNextPage
    	    hasPreviousPage
    	    endCursor
    	    startCursor
        }
    }
}");

#if NET6_0_OR_GREATER
            // GraphQL.NET v8 with DateOnly (NET 6+) uses culture-invariant date formatting without time
            var expectedJson = @"
{
  'data': {
    'personConnection': {
      'edges': [
        {
          'node': {
            'firstName': 'Hyrum',
            'lastName': 'Clyde'
          },
          'cursor': 'MS4wMS4yMDE5'
        },
        {
          'node': {
            'firstName': 'Doug',
            'lastName': 'Day'
          },
          'cursor': 'Mi4wMS4yMDE5'
        }
      ],
      'pageInfo': {
        'hasNextPage': true,
        'hasPreviousPage': false,
        'endCursor': 'Mi4wMS4yMDE5',
        'startCursor': 'MS4wMS4yMDE5'
      }
    }
  }
}";
#else
            // GraphQL.NET v8 with DateTime (.NET Framework) uses culture-invariant date formatting with time
            var expectedJson = @"
{
  'data': {
    'personConnection': {
      'edges': [
        {
          'node': {
            'firstName': 'Hyrum',
            'lastName': 'Clyde'
          },
          'cursor': 'MS4wMS4yMDE5IDAwOjAwOjAw'
        },
        {
          'node': {
            'firstName': 'Doug',
            'lastName': 'Day'
          },
          'cursor': 'Mi4wMS4yMDE5IDAwOjAwOjAw'
        }
      ],
      'pageInfo': {
        'hasNextPage': true,
        'hasPreviousPage': false,
        'endCursor': 'Mi4wMS4yMDE5IDAwOjAwOjAw',
        'startCursor': 'MS4wMS4yMDE5IDAwOjAwOjAw'
      }
    }
  }
}";
#endif

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
        }
    }
}
