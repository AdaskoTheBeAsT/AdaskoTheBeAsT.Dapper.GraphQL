using GraphQL.Types;

namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql.IntegrationTest.Models;

[PascalCase]
public enum PhoneType
{
    Unknown = 0,
    Home = 1,
    Work = 2,
    Mobile = 3,
    Other = 4,
}
