using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest
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

            const string expectedJson = @"
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
                ""firstName"": ""Douglas"",
                ""lastName"": ""Day"",
                ""emails"": [{
                        ""id"": 3,
                        ""address"": ""dougrday@gmail.com""
                    },
                    {
                        ""id"": 2,
                        ""address"": ""dday@landmarkhw.com""
                    }
                ],
                ""phones"": [{
                    ""id"": 1,
                    ""number"": ""8011234567"",
                    ""type"": ""Mobile""
                }],
                ""companies"": [{
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
                    },
                    {
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
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
                        ""id"": 3,
                        ""number"": ""8011111111"",
                        ""type"": ""Home""
                    },
                    {
                        ""id"": 2,
                        ""number"": ""8019876543"",
                        ""type"": ""Mobile""
                    }
                ],
                ""companies"": [{
                        ""id"": 1,
                        ""name"": ""Landmark Home Warranty, LLC""
                    },
                    {
                        ""id"": 2,
                        ""name"": ""Navitaire, LLC""
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
                    ""firstName"": ""Douglas"",
                    ""lastName"": ""Day"",
                    ""emails"": [{
                            ""id"": 3,
                            ""address"": ""dougrday@gmail.com""
                        },
                        {
                            ""id"": 2,
                            ""address"": ""dday@landmarkhw.com""
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

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
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

            const string expectedJson = @"
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
        ""firstName"": ""Douglas"",
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

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
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

            const string expectedJson = @"
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

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
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

            const string expectedJson = @"
{
  data: {
    people: [
      {
        firstName: 'Hyrum',
        lastName: 'Clyde'
      },
      {
        firstName: 'Douglas',
        lastName: 'Day'
      },
      {
        firstName: 'Kevin',
        lastName: 'Russon'
      }
    ]
  }
}";

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
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

            const string expectedJson = @"
{
    data: {
        person: {
            id: 2,
            firstName: 'Doug',
            lastName: 'Day'
        }
    }
}";

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
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
            // Cursor encodes DateOnly as ISO yyyy-MM-dd via InvariantCulture
            const string expectedJson = @"
{
  'data': {
    'personConnection': {
      'edges': [
        {
          'node': {
            'firstName': 'Hyrum',
            'lastName': 'Clyde'
          },
          'cursor': 'MjAxOS0wMS0wMQ=='
        },
        {
          'node': {
            'firstName': 'Douglas',
            'lastName': 'Day'
          },
          'cursor': 'MjAxOS0wMS0wMg=='
        }
      ],
      'pageInfo': {
        'hasNextPage': true,
        'hasPreviousPage': false,
        'endCursor': 'MjAxOS0wMS0wMg==',
        'startCursor': 'MjAxOS0wMS0wMQ=='
      }
    }
  }
}";
#else
            // Cursor encodes DateTime as yyyy-MM-dd HH:mm:ss via InvariantCulture
            const string expectedJson = @"
{
  'data': {
    'personConnection': {
      'edges': [
        {
          'node': {
            'firstName': 'Hyrum',
            'lastName': 'Clyde'
          },
          'cursor': 'MjAxOS0wMS0wMSAwMDowMDowMA=='
        },
        {
          'node': {
            'firstName': 'Douglas',
            'lastName': 'Day'
          },
          'cursor': 'MjAxOS0wMS0wMiAwMDowMDowMA=='
        }
      ],
      'pageInfo': {
        'hasNextPage': true,
        'hasPreviousPage': false,
        'endCursor': 'MjAxOS0wMS0wMiAwMDowMDowMA==',
        'startCursor': 'MjAxOS0wMS0wMSAwMDowMDowMA=='
      }
    }
  }
}";
#endif

            _fixture.JsonEquals(expectedJson, json).Should().BeTrue();
        }
    }
}
