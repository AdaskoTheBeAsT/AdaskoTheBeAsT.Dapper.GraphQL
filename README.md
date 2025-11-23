# 🚀 Dapper.GraphQL

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/AdaskoTheBeAsT.Dapper.GraphQL.svg)](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL/)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.0%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

> ⚡ Blazing-fast integration between Dapper and GraphQL.NET with **automatic SQL query generation** from GraphQL queries.

Stop writing boilerplate SQL! Dapper.GraphQL automatically generates optimized SQL queries based on what your clients actually request through GraphQL. Only fetch the data you need, nothing more, nothing less.

---

## ✨ Why Dapper.GraphQL?

- **🎯 Smart Query Generation** - Automatically builds SQL `SELECT` statements based on GraphQL field selections
- **🔗 Nested Relationships** - Handles complex object graphs with automatic `JOIN` generation
- **⚡ Performance First** - Built on top of Dapper's lightning-fast micro-ORM
- **🧩 DI-Friendly** - First-class support for ASP.NET Core dependency injection
- **🎨 Type-Safe** - Strongly-typed query builders and entity mappers
- **🗄️ Database Agnostic** - Works with any database Dapper supports (PostgreSQL, SQL Server, MySQL, etc.)
- **📦 Multi-Framework** - Supports .NET Standard 2.0, .NET 8, .NET 9, and .NET 10

---

## 📦 Installation

```bash
dotnet add package AdaskoTheBeAsT.Dapper.GraphQL
```

For ASP.NET Core DI integration:
```bash
dotnet add package AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection
```

---

## 🎯 Quick Start

### 1. Configure Services

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDapperGraphQL(options =>
    {
        // Register your GraphQL types
        options.AddType<PersonType>();
        options.AddType<EmailType>();
        options.AddType<PhoneType>();
        
        // Register your schema
        options.AddSchema<PersonSchema>();
        
        // Register query builders (the magic happens here!)
        options.AddQueryBuilder<Person, PersonQueryBuilder>();
        options.AddQueryBuilder<Email, EmailQueryBuilder>();
        options.AddQueryBuilder<Phone, PhoneQueryBuilder>();
    });
}
```

### 2. Create a Query Builder

Query builders map GraphQL selections to SQL columns:

```csharp
public class EmailQueryBuilder : IQueryBuilder<Email>
{
    public SqlQueryContext Build(
        SqlQueryContext query, 
        IHasSelectionSetNode context, 
        string alias)
    {
        // Always include the ID
        query.Select($"{alias}.Id");
        query.SplitOn<Email>("Id");

        // Only select fields that were requested in GraphQL
        var fields = context.GetSelectedFields();
        
        if (fields.ContainsKey("address"))
            query.Select($"{alias}.Address");
            
        if (fields.ContainsKey("isVerified"))
            query.Select($"{alias}.IsVerified");

        return query;
    }
}
```

### 3. Use in GraphQL Resolvers

```csharp
Field<ListGraphType<PersonType>>(
    "people",
    resolve: context =>
    {
        var query = SqlBuilder
            .From("Person person")
            .Where("person.IsActive = @isActive", new { isActive = true });
            
        // Build the query based on GraphQL selections
        query = personQueryBuilder.Build(query, context.FieldAst, "person");
        
        // Create entity mapper
        var mapper = new PersonEntityMapper();
        
        // Execute and return results
        using var connection = dbConnectionFactory.CreateConnection();
        return query.Execute(connection, mapper, context.FieldAst);
    }
);
```

---

## 🧠 How It Works

Dapper.GraphQL analyzes your GraphQL query and generates the optimal SQL automatically.

### Example: Simple Query

**GraphQL Query:**
```graphql
{
  people {
    id
    firstName
    lastName
  }
}
```

**Generated SQL:**
```sql
SELECT 
  person.Id, 
  person.FirstName, 
  person.LastName
FROM Person person
WHERE person.IsActive = @isActive
```

### Example: Nested Query

**GraphQL Query:**
```graphql
{
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
  }
}
```

**Generated SQL:**
```sql
SELECT 
  person.Id, 
  person.FirstName, 
  person.LastName,
  email.Id,
  email.Address,
  phone.Id,
  phone.Number,
  phone.Type
FROM Person person
LEFT JOIN PersonEmail pe ON person.Id = pe.PersonId
LEFT JOIN Email email ON pe.EmailId = email.Id
LEFT JOIN PersonPhone pp ON person.Id = pp.PersonId
LEFT JOIN Phone phone ON pp.PhoneId = phone.Id
```

**The magic?** Only requested fields are included in the `SELECT` clause! 🎩✨

---

## 🔧 Core Concepts

### Query Builders

Query builders dynamically construct SQL queries based on GraphQL field selections. They:
- Select only requested fields (no over-fetching)
- Generate proper `JOIN` statements for nested entities
- Support recursive/self-referencing relationships
- Are composable and chainable

See [`EmailQueryBuilder.cs`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest/QueryBuilders/EmailQueryBuilder.cs) and [`PersonQueryBuilder.cs`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest/QueryBuilders/PersonQueryBuilder.cs) for real examples.

### Entity Mappers

Entity mappers deserialize SQL result sets into object graphs. Since SQL `JOIN`s produce multiple rows for a single entity, entity mappers intelligently merge rows into the correct object hierarchy.

See [`PersonEntityMapper.cs`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest/EntityMappers/PersonEntityMapper.cs) for an example.

---

## 📖 Advanced Usage

### Chaining Query Builders

Query builders can reference other query builders for nested entities:

```csharp
public class PersonQueryBuilder : IQueryBuilder<Person>
{
    private readonly IQueryBuilder<Email> _emailQueryBuilder;
    private readonly IQueryBuilder<Phone> _phoneQueryBuilder;

    public PersonQueryBuilder(
        IQueryBuilder<Email> emailQueryBuilder,
        IQueryBuilder<Phone> phoneQueryBuilder)
    {
        _emailQueryBuilder = emailQueryBuilder;
        _phoneQueryBuilder = phoneQueryBuilder;
    }

    public SqlQueryContext Build(
        SqlQueryContext query, 
        IHasSelectionSetNode context, 
        string alias)
    {
        query.Select($"{alias}.Id", $"{alias}.FirstName", $"{alias}.LastName");
        query.SplitOn<Person>("Id");

        var fields = context.GetSelectedFields();

        // Handle nested emails
        if (fields.ContainsKey("emails"))
        {
            query.LeftJoin($"Email email ON {alias}.Id = email.PersonId");
            query = _emailQueryBuilder.Build(query, fields["emails"], "email");
        }

        // Handle nested phones
        if (fields.ContainsKey("phones"))
        {
            query.LeftJoin($"Phone phone ON {alias}.Id = phone.PersonId");
            query = _phoneQueryBuilder.Build(query, fields["phones"], "phone");
        }

        return query;
    }
}
```

### Custom Deduplication

Use `DeduplicatingEntityMapper` to handle merged/duplicate entities in your database.

---

## 🧪 Development & Testing

### Prerequisites

- [.NET 8+ SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for integration tests)

### Running Tests

Start a PostgreSQL container:
```bash
docker run --name dapper-graphql-test \
  -e POSTGRES_PASSWORD=dapper-graphql \
  -p 5432:5432 \
  -d postgres
```

Run the test suite:
```bash
dotnet test
```

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📚 Examples

Check out the [`test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest) project for comprehensive examples including:

- Query builders for complex entities
- Entity mappers for nested relationships
- GraphQL type definitions
- Integration with GraphQL.NET
- Handling self-referencing entities (supervisor, career counselor)
- Many-to-many relationships (Person ↔ Company)

---

## 🗺️ Roadmap

- [ ] Fluent-style pagination API
- [ ] Built-in support for filtering and sorting
- [ ] Query result caching
- [ ] Batch loading support

---

## 📅 DateOnly & TimeOnly Support (.NET 6+)

### Overview

This project properly supports `DateOnly` and `TimeOnly` types introduced in .NET 6 while maintaining `DateTime` compatibility for .NET Framework through conditional compilation.

### Current Implementation Status ✅

**Person.CreateDate Example:**
```csharp
public class Person
{
#if NET6_0_OR_GREATER
    // .NET 6+ uses DateOnly for PostgreSQL DATE columns
    public DateOnly CreateDate { get; set; }
#else
    // .NET Framework uses DateTime for PostgreSQL DATE columns
    public DateTime CreateDate { get; set; }
#endif
}
```

All repository methods, GraphQL resolvers, and tests have been updated to support both types across all target frameworks.

### Database Provider Support

#### 🐘 PostgreSQL (Npgsql) - ✅ NATIVE SUPPORT

**Status:** Fully supported since Npgsql 6.0+

**Configuration:** None required - automatic mapping!

```csharp
// Npgsql automatically maps:
// PostgreSQL DATE     -> DateOnly (on .NET 6+) or DateTime (on .NET Framework)
// PostgreSQL TIME     -> TimeOnly (on .NET 6+)
// PostgreSQL TIMESTAMP -> DateTime
```

**Version Requirements:**
- Npgsql 6.0+ for .NET 6+ DateOnly support
- Npgsql 4.0+ for .NET Framework DateTime support

**Benefits:**
- Zero configuration needed
- Strongly-typed Dapper queries work out of the box
- No custom type handlers required

---

#### 🗄️ SQL Server - ⚠️ REQUIRES TYPE HANDLERS (Dapper)

**Status:** EF Core 8+ has native support, but Dapper requires custom TypeHandlers

**Configuration Required:**

```csharp
using Dapper;
using System.Data;

// Add these at application startup
SqlMapper.AddTypeHandler(new DapperSqlDateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DapperSqlTimeOnlyTypeHandler());

public class DapperSqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

public class DapperSqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value) => TimeOnly.FromTimeSpan((TimeSpan)value);
    
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}
```

**Database Schema:**
```sql
CREATE TABLE Person (
    Id INT PRIMARY KEY,
    FirstName NVARCHAR(50),
    CreateDate DATE,           -- Maps to DateOnly
    WorkStartTime TIME         -- Maps to TimeOnly
)
```

**Resources:**
- [Dapper DateOnly/TimeOnly Tutorial](https://dev.to/karenpayneoregon/dapper-dateonlytimeonly-1ii9)
- [EF Core 8 DateOnly Support](https://erikej.github.io/efcore/sqlserver/2023/09/03/efcore-dateonly-timeonly...)

---

#### 🐬 MySQL - ⚠️ REQUIRES TYPE HANDLERS

**Status:** Limited support - requires custom TypeHandlers

**Known Issues:**
- MySqlConnector may throw `InvalidCastException` with DateOnly
- `MySqlDataReader.GetValue` does not natively support DateOnly

**Configuration Required:**

```csharp
using Dapper;
using System.Data;

// Add at application startup
SqlMapper.AddTypeHandler(new MySqlDateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new MySqlTimeOnlyTypeHandler());

public class MySqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        if (value is MySqlDateTime mySqlDt)
            return DateOnly.FromDateTime(mySqlDt.GetDateTime());
        
        return DateOnly.Parse(value.ToString()!);
    }
    
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

public class MySqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
    {
        if (value is TimeSpan ts)
            return TimeOnly.FromTimeSpan(ts);
        
        return TimeOnly.Parse(value.ToString()!);
    }
    
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}
```

**Database Schema:**
```sql
CREATE TABLE Person (
    Id INT PRIMARY KEY,
    FirstName VARCHAR(50),
    CreateDate DATE,           -- Maps to DateOnly with custom handler
    WorkStartTime TIME         -- Maps to TimeOnly with custom handler
)
```

**Resources:**
- [Stack Overflow: DateOnly with MySQL](https://stackoverflow.com/questions/79067781/using-dateonly-with-entity-framework-core-and-mysql)

---

#### 🦅 Oracle - ⚠️ REQUIRES TYPE HANDLERS

**Status:** Limited support - requires custom TypeHandlers

**Configuration Required:**

```csharp
using Dapper;
using System.Data;

// Add at application startup
SqlMapper.AddTypeHandler(new OracleDateOnlyTypeHandler());

public class OracleDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        
        return DateOnly.Parse(value.ToString()!);
    }
    
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
```

**Database Schema:**
```sql
CREATE TABLE Person (
    Id NUMBER PRIMARY KEY,
    FirstName VARCHAR2(50),
    CreateDate DATE            -- Maps to DateOnly with custom handler
)
```

**Note:** Oracle's `DATE` type includes time, so consider using `TIMESTAMP` for full DateTime and custom date-only columns for DateOnly.

---

#### 🪶 SQLite - ✅ NATIVE SUPPORT

**Status:** Fully supported since Microsoft.Data.Sqlite 6.0+

**Configuration:** None required - automatic mapping!

```csharp
// Microsoft.Data.Sqlite automatically maps:
// SQLite DATE (TEXT) -> DateOnly (on .NET 6+) or DateTime (on .NET Framework)
// SQLite TIME (TEXT) -> TimeOnly (on .NET 6+)
```

**Database Schema:**
```sql
CREATE TABLE Person (
    Id INTEGER PRIMARY KEY,
    FirstName TEXT,
    CreateDate DATE,           -- Stored as TEXT, mapped to DateOnly
    WorkStartTime TIME         -- Stored as TEXT, mapped to TimeOnly
)
```

**Version Requirements:**
- Microsoft.Data.Sqlite 6.0+ for DateOnly/TimeOnly support

---

### Implementation Checklist

When adding new DATE or TIME columns:

- [ ] **Add conditional compilation to model:**
```csharp
#if NET6_0_OR_GREATER
    public DateOnly BirthDate { get; set; }
#else
    public DateTime BirthDate { get; set; }
#endif
```

- [ ] **Update interface method signatures** with conditional types
- [ ] **Update repository implementations** with conditional types
- [ ] **Update GraphQL resolvers** if applicable
- [ ] **Update cursor handling** for GraphQL connections (DateOnly cursors are shorter)
- [ ] **Add conditional test expectations:**
```csharp
#if NET6_0_OR_GREATER
    // DateOnly format: "1.01.2019" (no time)
    var expectedCursor = "MS4wMS4yMDE5";
#else
    // DateTime format: "1.01.2019 00:00:00"
    var expectedCursor = "MS4wMS4yMDE5IDAwOjAwOjAw";
#endif
```

- [ ] **Configure TypeHandlers** if using SQL Server, MySQL, or Oracle
- [ ] **Test on all target frameworks**

### Important Dapper Limitation ⚠️

**Dynamic Queries Issue:**
```csharp
// ❌ May return DateTime instead of DateOnly on .NET 6+
var result = connection.Query("SELECT CreateDate FROM Person").First();
var date = result.CreateDate; // Might be DateTime, not DateOnly!

// ✅ Strongly-typed queries work correctly
var result = connection.Query<Person>("SELECT * FROM Person").First();
var date = result.CreateDate; // Will be DateOnly on .NET 6+
```

**Solution:** Always use strongly-typed Dapper queries (which this project does).

### GraphQL Cursor Format Differences

DateOnly cursors are shorter than DateTime cursors:
- **DateTime cursor:** `MS4wMS4yMDE5IDAwOjAwOjAw` (base64: "1.01.2019 00:00:00")
- **DateOnly cursor:** `MS4wMS4yMDE5` (base64: "1.01.2019")

Use conditional expectations in tests as shown in [`GraphQLTests.cs:364-426`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest/GraphQLTests.cs).

### Benefits of Using DateOnly

1. **Type Safety** - Can't accidentally add time to dates
2. **Clarity** - Intent is explicit (date vs timestamp)
3. **Performance** - Smaller memory footprint (no time component)
4. **Correctness** - Avoids timezone and midnight confusion

### Additional Resources

- [Npgsql Date/Time Documentation](https://www.npgsql.org/doc/types/datetime.html)
- [Dapper DateOnly/TimeOnly Tutorial](https://conradakunga.com/blog/dapper-part-7-adding-dateonly-timeonly-support/)
- [EF Core 8 DateOnly Support](https://erikej.github.io/efcore/sqlserver/2023/09/03/efcore-dateonly-timeonly...)
- [.NET DateOnly Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.dateonly)

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👏 Credits

Originally created by **Landmark Home Warranty** team:
- Doug Day
- Kevin Russon
- Ben McCallum
- Natalya Arbit
- Per Liedman
- John Stovin

Maintained by [AdaskoTheBeAsT](https://github.com/AdaskoTheBeAsT)

---

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL/)
- [GitHub Repository](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.GraphQL)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [GraphQL.NET Documentation](https://graphql-dotnet.github.io/)

---

**Made with ❤️ for developers who love clean code and fast queries**
