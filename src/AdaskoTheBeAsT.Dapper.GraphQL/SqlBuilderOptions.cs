namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    public class SqlBuilderOptions
    {
        public static SqlBuilderOptions Default { get; } = new();

        public string ParameterPrefix { get; set; } = "@";
    }
}
