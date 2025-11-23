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
