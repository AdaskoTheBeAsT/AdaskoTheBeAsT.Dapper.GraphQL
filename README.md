# 🚀 Dapper.GraphQL

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/AdaskoTheBeAsT.Dapper.GraphQL.svg)](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL/)
[![Downloads](https://img.shields.io/nuget/dt/AdaskoTheBeAsT.Dapper.GraphQL.svg)](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL/)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010%20%7C%20Framework%204.7.2%2B-512BD4)](https://dotnet.microsoft.com/)
[![PRs welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](#-contributing)

> ⚡ **Generate exactly the SQL your GraphQL client asked for — no more, no less.**

`Dapper.GraphQL` bridges [Dapper](https://github.com/DapperLib/Dapper) and
[GraphQL.NET](https://graphql-dotnet.github.io/) so the SQL you execute mirrors
the GraphQL selection set: only requested columns are projected, only required
joins are emitted, and Dapper's `QueryMultiple` machinery materialises the
nested object graph back into your POCOs. 🎯

---

## 📚 Table of contents

- [✨ Why use it](#-why-use-it)
- [📦 Packages](#-packages)
- [⬇️ Install](#-install)
- [⚡ Five-minute quickstart](#-five-minute-quickstart)
- [🧠 How it works](#-how-it-works)
- [🧩 Core concepts](#-core-concepts)
- [🔧 Advanced usage](#-advanced-usage)
- [📅 `DateOnly` / `TimeOnly` per vendor](#-dateonly--timeonly-per-vendor)
- [🚨 Upgrading from 1.x](#-upgrading-from-1x)
- [🛠️ Building & testing](#-building--testing)
- [🤝 Contributing](#-contributing)
- [👏 Credits & license](#-credits--license)

---

## ✨ Why use it

- 🎯 **No over-fetching.** Selected GraphQL fields drive `SELECT` projection
  and `JOIN` emission.
- 🔗 **One round-trip for object graphs.** Nested entities load through
  Dapper's multi-mapping with `splitOn` automatically configured.
- 🗄️ **Vendor-aware identity retrieval.** PostgreSQL `nextval`/`currval`,
  SQL Server `SCOPE_IDENTITY()`, MySQL `LAST_INSERT_ID()`,
  SQLite `last_insert_rowid()`, Oracle sequences — all behind the same
  `ExecuteWith*Identity` extension.
- 🎨 **Strongly typed builders.** `SqlBuilder.Insert(person)` /
  `SqlBuilder.Update(person)` / `SqlBuilder.Delete<Person>()` derive table
  and parameter names from your entity types.
- 🧩 **First-class DI.** Vendor-specific `AddDapperGraphQl<Vendor>` extension
  wires query builders, schemas, and a vendor-aware `SqlBuilderOptions`
  (parameter prefix etc.) into your container.
- ⚡ **Performance first.** Built on Dapper's lightning-fast micro-ORM —
  zero reflection at hot path, minimal allocations.
- 📦 **Multi-target.** `net8.0`, `net9.0`, `net10.0`, `net472`, `net48`,
  `net481`.

---

## 📦 Packages

| Package                                                  | What's inside                                                                                                                          |
|----------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| 🧱 `AdaskoTheBeAsT.Dapper.GraphQL`                       | Vendor-agnostic core: `SqlBuilder`, query / insert / update / delete contexts, entity mappers, `IQueryBuilder<T>`, DI options object.  |
| 🐘 `AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql`            | `AddDapperGraphQlPostgreSql`, `PostgreSqlIdentity.NextIdentity[Async]`, `ExecuteWithPostgreSqlIdentity[Async]`.                         |
| 🗄️ `AdaskoTheBeAsT.Dapper.GraphQL.SqlServer`             | `AddDapperGraphQlSqlServer`, `ExecuteWithSqlServerIdentity[Async]`.                                                                    |
| 🐬 `AdaskoTheBeAsT.Dapper.GraphQL.MySql`                 | `AddDapperGraphQlMySql`, `ExecuteWithMySqlIdentity[Async]`.                                                                            |
| 🪶 `AdaskoTheBeAsT.Dapper.GraphQL.Sqlite`                | `AddDapperGraphQlSqlite`, `ExecuteWithSqliteIdentity[Async]`.                                                                          |
| 🦅 `AdaskoTheBeAsT.Dapper.GraphQL.Oracle`                | `AddDapperGraphQlOracle`, `OracleIdentity.NextIdentity[Async]`, `ExecuteWithOracleIdentity[Async]`, `BindByNameOracleConnection`.      |

> 💡 Pick exactly one vendor package per database. The vendor package
> transitively brings the core in.

---

## ⬇️ Install

```bash
dotnet add package AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql
# or .SqlServer / .MySql / .Sqlite / .Oracle
```

That's it. The vendor package brings in the core. There is **no separate
`ServiceCollection` package** — DI lives in each vendor package. ✅

---

## ⚡ Five-minute quickstart

### 1️⃣ Define your entities

```csharp
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public List<Email> Emails { get; set; } = [];
}

public class Email
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Address { get; set; } = "";
}
```

### 2️⃣ Write query builders that mirror GraphQL → SQL

```csharp
public class EmailQueryBuilder : IQueryBuilder<Email>
{
    public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode ctx, string alias)
    {
        query.Select($"{alias}.Id");
        query.SplitOn<Email>("Id");

        var fields = ctx.GetSelectedFields();
        if (fields.ContainsKey("address")) query.Select($"{alias}.Address");
        return query;
    }
}

public class PersonQueryBuilder(IQueryBuilder<Email> emails) : IQueryBuilder<Person>
{
    public SqlQueryContext Build(SqlQueryContext query, IHasSelectionSetNode ctx, string alias)
    {
        query.Select($"{alias}.Id");
        query.SplitOn<Person>("Id");

        var fields = ctx.GetSelectedFields();
        if (fields.ContainsKey("firstName")) query.Select($"{alias}.FirstName");
        if (fields.ContainsKey("lastName"))  query.Select($"{alias}.LastName");

        if (fields.TryGetValue("emails", out var emailField))
        {
            query.LeftJoin($"Email email ON {alias}.Id = email.PersonId");
            query = emails.Build(query, emailField, "email");
        }

        return query;
    }
}
```

### 3️⃣ Register everything (vendor-specific DI extension)

```csharp
services.AddDapperGraphQlPostgreSql(o =>
{
    o.AddType<PersonType>();
    o.AddType<EmailType>();
    o.AddSchema<PersonSchema>();

    o.AddQueryBuilder<Person, PersonQueryBuilder>();
    o.AddQueryBuilder<Email,  EmailQueryBuilder>();
});
```

For SQL Server use `AddDapperGraphQlSqlServer`, for MySQL
`AddDapperGraphQlMySql`, etc. Each one registers the matching
`*SqlBuilderOptions` (which carries the correct parameter prefix:
`@` for PostgreSQL/SQL Server/MySQL/SQLite, `:` for Oracle).

### 4️⃣ Resolve from a connection wrapped with vendor options

```csharp
var people = Field<ListGraphType<PersonType>>("people")
    .Resolve(ctx =>
    {
        var query = SqlBuilder
            .From("Person person")
            .Where("person.IsActive = @isActive", new { isActive = true });

        query = personQueryBuilder.Build(query, ctx.FieldAst, "person");

        using var raw    = new NpgsqlConnection(_connStr);
        using var conn   = raw.WithDapperGraphQlOptions(_options); // vendor-aware prefix
        var mapper       = new PersonEntityMapper();

        return query.Execute(conn, mapper, ctx.FieldAst);
    });
```

`WithDapperGraphQlOptions` makes the generated SQL match the connection's
parameter prefix — important on Oracle where parameters use `:` instead of
`@`. 🪄

### 5️⃣ Insert with vendor-aware identity retrieval

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;

var newId = SqlBuilder
    .Insert(person)
    .ExecuteWithPostgreSqlIdentity(conn, p => p.Id);
```

| Vendor        | Identity call                                       |
|---------------|-----------------------------------------------------|
| 🐘 PostgreSQL | `ExecuteWithPostgreSqlIdentity(conn, p => p.Id)`    |
| 🗄️ SQL Server  | `ExecuteWithSqlServerIdentity<int>(conn)`           |
| 🐬 MySQL      | `ExecuteWithMySqlIdentity<int>(conn)`               |
| 🪶 SQLite     | `ExecuteWithSqliteIdentity<int>(conn)`              |
| 🦅 Oracle     | `ExecuteWithOracleIdentity(conn, p => p.Id)`        |

---

## 🧠 How it works

**GraphQL request:**

```graphql
{
  people {
    firstName
    emails { address }
  }
}
```

**What `Dapper.GraphQL` runs:**

```sql
SELECT person.Id, person.FirstName,
       email.Id, email.Address
FROM Person person
LEFT JOIN Email email ON person.Id = email.PersonId
WHERE person.IsActive = @isActive;
```

`lastName` is never selected because the client never asked for it. ✂️
Add a `phones { number }` selection to the GraphQL query and a second join +
columns appear automatically — driven by the matching query builder in DI. 🎩✨

The `SqlQueryContext.Execute(...)` overload uses Dapper's multi-mapping with
the `splitOn` columns each builder reported via `SplitOn<T>(...)`, then hands
the row tuples to your `EntityMapper<T>` to assemble the object graph.

---

## 🧩 Core concepts

### `SqlBuilder` 🏗️

Static fluent factory for the four context types:

```csharp
SqlBuilder.From("Person p");
SqlBuilder.Insert(person);
SqlBuilder.Update(person);
SqlBuilder.Delete<Person>();
```

### `IQueryBuilder<T>` 🧱

One per entity. Reads the GraphQL selection set, appends columns / joins to a
`SqlQueryContext`, and returns it. Builders compose: a `PersonQueryBuilder`
asks an injected `IQueryBuilder<Email>` to extend the same context.

### `EntityMapper<T>` and `DeduplicatingEntityMapper<T>` 🪡

Multi-mapping turns one logical row into many physical rows once a `LEFT JOIN`
expands a collection. Entity mappers stitch those rows back into a single
object graph and (with `DeduplicatingEntityMapper<T>`) collapse duplicate
parent rows.

### `SqlBuilderOptions` and `IDapperGraphQlConnection` 🔌

Vendor packages register a `*SqlBuilderOptions` (singleton) that knows the
correct parameter prefix. Wrap your raw `DbConnection` with
`.WithDapperGraphQlOptions(options)` so the generated SQL uses the right
prefix for that vendor.

---

## 🔧 Advanced usage

### Chained inserts ⛓️

```csharp
SqlBuilder
    .Insert(person)
    .Insert("Email", new { Address = "a@b.com", PersonId = personId })
    .Execute(conn);
```

### Async everywhere ⏱️

```csharp
await SqlBuilder.Update(person).ExecuteAsync(conn);
await SqlBuilder.Delete<Person>(new { Id = 1 }).ExecuteAsync(conn);
await PostgreSqlIdentity.NextIdentityAsync<Person, int>(conn, p => p.Id);
```

### Self-references and many-to-many 🕸️

The integration test suite in
[`test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest`](test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest)
demonstrates self-referencing entities (`Person.Supervisor`,
`Person.CareerCounselor`) and many-to-many (`Person ↔ Company`) with composite
join tables — copy those query builders as templates.

### Connection pagination (Relay) 📜

The PostgreSQL test fixture wires up GraphQL.NET v8 `ConnectionType<>` and
`ConnectionType<,>` and uses cursor encoding helpers — see `GraphQLTests.cs`.

---

## 📅 `DateOnly` / `TimeOnly` per vendor

The library targets `net472`/`net48`/`net481` (where `DateOnly` does **not**
exist) and `net8.0`/`net9.0`/`net10.0` (where it does). Use conditional
compilation in your entities:

```csharp
public class Person
{
#if NET6_0_OR_GREATER
    public DateOnly CreateDate { get; set; }
#else
    public DateTime CreateDate { get; set; }
#endif
}
```

| Provider                      | `DateOnly` support                                 | Action required                            |
|-------------------------------|----------------------------------------------------|--------------------------------------------|
| 🐘 **Npgsql (PostgreSQL)**    | ✅ Native since 6.0                                 | None.                                      |
| 🪶 **Microsoft.Data.Sqlite**  | ✅ Native since 6.0                                 | None.                                      |
| 🗄️ **SqlClient (SQL Server)** | ⚠️ Dapper needs help                                | Register `TypeHandler<DateOnly>` etc.      |
| 🐬 **MySqlConnector**         | ⚠️ Dapper needs help; can throw `InvalidCastException` | Register custom `TypeHandler`s.        |
| 🦅 **Oracle**                 | ⚠️ Dapper needs help                                | Register custom `TypeHandler<DateOnly>`.   |

Minimal SQL Server / MySQL / Oracle handler:

```csharp
public sealed class DapperDateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value  = value.ToDateTime(TimeOnly.MinValue);
    }
}

SqlMapper.AddTypeHandler(new DapperDateOnlyHandler());
```

> 💡 **Tip.** Always use strongly typed Dapper queries (`Query<Person>(...)`) —
> dynamic queries can return `DateTime` even where `DateOnly` is registered.

---

## 🚨 Upgrading from 1.x

`2.0.0` splits the previously monolithic `AdaskoTheBeAsT.Dapper.GraphQL`
package into a vendor-agnostic core plus five vendor-specific packages, and
**removes** the standalone `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection`
package — DI moves into each vendor package.

> 📖 See [**MIGRATION.md**](MIGRATION.md) for the full upgrade guide,
> namespace mapping, and copy-pasteable before/after snippets.

**TL;DR:**

1. 🔄 Replace `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection` with the
   matching vendor package (`*.PostgreSql`, `*.SqlServer`, `*.MySql`,
   `*.Sqlite`, `*.Oracle`).
2. 🔄 Replace `services.AddDapperGraphQL(...)` with
   `services.AddDapperGraphQl<Vendor>(...)`.
3. 🔄 Replace `using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;` with the
   vendor namespace (e.g.
   `AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions`).
4. 🔄 Rename `ExecuteWithSqlIdentity` → `ExecuteWithSqlServerIdentity`.

If you only used the vendor-agnostic surface (`SqlBuilder`,
`SqlQueryContext`, `EntityMapper`) bumping the package version is enough. ✅

---

## 🛠️ Building & testing

### Prerequisites

- 🟣 [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 🐳 Docker — required by the integration test suites; each fixture spins up
  its own container via [Testcontainers](https://dotnet.testcontainers.org/)
  for PostgreSQL / SQL Server / MySQL / Oracle. SQLite uses an in-memory
  file.

### Run everything

```bash
dotnet restore
dotnet build  -c Release
dotnet test   -c Release
```

### Run a single integration suite

```bash
dotnet test test/integ/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.IntegrationTest
```

---

## 🤝 Contributing

PRs welcome! Please:

1. 💬 Open an issue first if you're proposing a behaviour change or new
   public API.
2. ✅ Add / extend integration tests for the affected vendor(s).
3. 🧹 Keep `dotnet build /warnaserror` clean — the repo treats warnings as
   errors.

Standard fork → branch → PR flow:

```bash
git checkout -b feature/amazing-thing
git commit -m "feat: amazing thing"
git push origin feature/amazing-thing
```

---

## 👏 Credits & license

Originally created by the **Landmark Home Warranty** team:

- Doug Day
- Kevin Russon
- Ben McCallum
- Natalya Arbit
- Per Liedman
- John Stovin

Maintained by [@AdaskoTheBeAsT](https://github.com/AdaskoTheBeAsT). 💜

Released under the [MIT License](LICENSE). 📄

### 🔗 Links

- 📦 [NuGet — core](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL/)
- 🐘 [NuGet — PostgreSQL](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql/)
- 🗄️ [NuGet — SQL Server](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL.SqlServer/)
- 🐬 [NuGet — MySQL](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL.MySql/)
- 🪶 [NuGet — SQLite](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL.Sqlite/)
- 🦅 [NuGet — Oracle](https://www.nuget.org/packages/AdaskoTheBeAsT.Dapper.GraphQL.Oracle/)
- 📘 [Dapper](https://github.com/DapperLib/Dapper)
- 📗 [GraphQL.NET](https://graphql-dotnet.github.io/)

---

**Made with ❤️ for developers who love clean code and fast queries.**
