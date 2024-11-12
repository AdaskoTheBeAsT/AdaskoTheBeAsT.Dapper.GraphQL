using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest.Models;

[PascalCase]
public enum PhoneType
{
    Unknown = 0,
    Home = 1,
    Work = 2,
    Mobile = 3,
    Other = 4,
}
