namespace AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql;

public class PostgreSqlSqlBuilderOptions : SqlBuilderOptions
{
    public PostgreSqlSqlBuilderOptions()
    {
        ParameterPrefix = "@";
    }
}
