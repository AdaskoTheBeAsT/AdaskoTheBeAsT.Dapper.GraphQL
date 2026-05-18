namespace AdaskoTheBeAsT.Dapper.GraphQL.SqlServer;

public class SqlServerSqlBuilderOptions : SqlBuilderOptions
{
    public SqlServerSqlBuilderOptions()
    {
        ParameterPrefix = "@";
    }
}
