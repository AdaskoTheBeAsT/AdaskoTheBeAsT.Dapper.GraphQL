#if NET6_0_OR_GREATER
using System;
using System.Data;
using Dapper;

namespace AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.IntegrationTest
{
    public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dt => DateOnly.FromDateTime(dt),
                DateOnly d => d,
                string s => DateOnly.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                _ => throw new InvalidCastException($"Cannot convert {value?.GetType()} to DateOnly"),
            };
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }
    }
}
#endif
