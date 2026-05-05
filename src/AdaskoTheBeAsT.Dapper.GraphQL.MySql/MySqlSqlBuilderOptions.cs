namespace AdaskoTheBeAsT.Dapper.GraphQL.MySql
{
    public class MySqlSqlBuilderOptions : SqlBuilderOptions
    {
        public MySqlSqlBuilderOptions()
        {
            ParameterPrefix = "@";
        }
    }
}
