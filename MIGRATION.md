# 🚨 Migration guide: 1.x → 2.0.0

> ⚠️ `2.0.0` is a **hard break**. There is no `[Obsolete]` shim period: the
> old APIs have been removed at the same time the new ones land.

This guide walks you through every change in the order you'll hit them when
you bump the package version. ✈️

---

## 📚 Table of contents

- [⚡ TL;DR](#-tldr)
- [🔍 What changed and why](#-what-changed-and-why)
- [1️⃣ Update your package references](#1️⃣-step-1--update-your-package-references)
- [2️⃣ Update your DI registration](#2️⃣-step-2--update-your-di-registration)
- [3️⃣ Update your `using` directives](#3️⃣-step-3--update-your-using-directives)
- [4️⃣ Rename / re-locate identity helpers](#4️⃣-step-4--rename--re-locate-identity-helpers)
- [5️⃣ Wrap your `DbConnection` with vendor options](#5️⃣-step-5--wrap-your-dbconnection-with-vendor-options)
- [🔁 Side-by-side examples](#-side-by-side-examples)
- [✅ Verifying the upgrade](#-verifying-the-upgrade)
- [❓ FAQ](#-faq)

---

## ⚡ TL;DR

1. 🔄 **Replace one package with two.** Drop
   `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection`, install one
   vendor-specific package per database.
2. 🏷️ **Rename your DI registration** from `AddDapperGraphQL` to the
   vendor-specific extension.
3. 📦 **Switch to vendor namespaces** for identity helpers.
4. ✏️ **Rename one method:** `ExecuteWithSqlIdentity` →
   `ExecuteWithSqlServerIdentity`.
5. 🔌 **Wrap your `DbConnection`** with `WithDapperGraphQlOptions(...)` so the
   right parameter prefix is used (mainly matters for Oracle).
6. ✅ **Recompile.** That's it.

If you never used the vendor-specific helpers (e.g. you only ever called
`SqlBuilder`, `SqlQueryContext`, `EntityMapper`), bumping the version is
sufficient — no source changes needed. 🎉

---

## 🔍 What changed and why

`1.x` shipped a single `AdaskoTheBeAsT.Dapper.GraphQL` assembly that contained
helpers for every database vendor (`PostgreSql`, `SqlServer`, `Sqlite`),
plus a separate `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection` package
holding the `AddDapperGraphQL` DI extension.

`2.0.0` reshapes the layout into:

```
🧱 AdaskoTheBeAsT.Dapper.GraphQL                  (core, vendor-agnostic)
🐘 AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql       (vendor)
🗄️ AdaskoTheBeAsT.Dapper.GraphQL.SqlServer        (vendor)
🐬 AdaskoTheBeAsT.Dapper.GraphQL.MySql            (vendor, NEW)
🪶 AdaskoTheBeAsT.Dapper.GraphQL.Sqlite           (vendor)
🦅 AdaskoTheBeAsT.Dapper.GraphQL.Oracle           (vendor, NEW)
```

**Why?**

- 🪶 Avoid pulling in three vendors' worth of static helpers when you only
  use one.
- 🎯 Each vendor package can carry its own `*SqlBuilderOptions` (parameter
  prefix), DI extension, and identity logic without polluting the core
  surface.
- ➕ New vendors (MySQL, Oracle) become genuinely additive instead of monkey-
  patching the core.

---

## 1️⃣ Step 1 — Update your package references

### ❌ Remove

```xml
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection" Version="1.*" />
```

### ✅ Add the vendor package(s) you need

```xml
<!-- Pick the vendor(s) for your database(s). The vendor package
     transitively brings in the core. -->
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql" Version="2.0.0" />
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.SqlServer"  Version="2.0.0" />
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.MySql"      Version="2.0.0" />
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.Sqlite"     Version="2.0.0" />
<PackageReference Include="AdaskoTheBeAsT.Dapper.GraphQL.Oracle"     Version="2.0.0" />
```

You can keep an explicit reference to the core
(`AdaskoTheBeAsT.Dapper.GraphQL`) if you want — it's harmless — but it isn't
required.

> ⚠️ **Important:** there is no longer an
> `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection` package. The DI
> registration moves into each vendor package.

---

## 2️⃣ Step 2 — Update your DI registration

### Before (1.x) ⛔

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection;

services.AddDapperGraphQL(options =>
{
    options.AddType<PersonType>();
    options.AddSchema<PersonSchema>();
    options.AddQueryBuilder<Person, PersonQueryBuilder>();
});
```

### After (2.x) ✅

Pick the extension that matches your database:

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql;

services.AddDapperGraphQlPostgreSql(options =>
{
    options.AddType<PersonType>();
    options.AddSchema<PersonSchema>();
    options.AddQueryBuilder<Person, PersonQueryBuilder>();
});
```

| 1.x                                                                           | 2.x                                                                                                   |
|-------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------|
| `services.AddDapperGraphQL(...)` (PostgreSQL was the implicit default)        | 🐘 `services.AddDapperGraphQlPostgreSql(...)` (`AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql`)            |
| —                                                                             | 🗄️ `services.AddDapperGraphQlSqlServer(...)`  (`AdaskoTheBeAsT.Dapper.GraphQL.SqlServer`)             |
| —                                                                             | 🐬 `services.AddDapperGraphQlMySql(...)`      (`AdaskoTheBeAsT.Dapper.GraphQL.MySql`)                 |
| —                                                                             | 🪶 `services.AddDapperGraphQlSqlite(...)`     (`AdaskoTheBeAsT.Dapper.GraphQL.Sqlite`)                |
| —                                                                             | 🦅 `services.AddDapperGraphQlOracle(...)`     (`AdaskoTheBeAsT.Dapper.GraphQL.Oracle`)                |

The `options` callback is identical — `AddType`, `AddSchema`,
`AddQueryBuilder` work the same way. 👌

Each `AddDapperGraphQl<Vendor>` extension also registers a
`<Vendor>SqlBuilderOptions` singleton holding the correct parameter prefix
(`@` for PostgreSQL, SQL Server, MySQL, SQLite; `:` for Oracle). Resolve it
from DI when you need to wrap a connection — see Step 5.

---

## 3️⃣ Step 3 — Update your `using` directives

The vendor-specific helpers moved out of the core's
`AdaskoTheBeAsT.Dapper.GraphQL.Extensions` namespace into a vendor namespace.

| 1.x using                                                | 2.x using                                                            |
|----------------------------------------------------------|----------------------------------------------------------------------|
| `using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;` ⛔     | 🐘 `using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;`     |
|                                                          | 🗄️ `using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions;`       |
|                                                          | 🐬 `using AdaskoTheBeAsT.Dapper.GraphQL.MySql.Extensions;`           |
|                                                          | 🪶 `using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Extensions;`          |
|                                                          | 🦅 `using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions;`          |

The vendor-agnostic namespaces are unchanged: ✅

- `AdaskoTheBeAsT.Dapper.GraphQL` — `SqlBuilder`, `EntityMapper`, etc.
- `AdaskoTheBeAsT.Dapper.GraphQL.Contexts` — `SqlQueryContext`,
  `SqlInsertContext`, `SqlUpdateContext`, `SqlDeleteContext`.
- `AdaskoTheBeAsT.Dapper.GraphQL.Interfaces` — `IQueryBuilder<T>`.

---

## 4️⃣ Step 4 — Rename / re-locate identity helpers

This is the bulk of the manual work. ⚙️ Below is the complete API map.

### Identity sequence helpers (called *before* the insert)

| 1.x symbol                                                                  | 2.x symbol                                                                                                                                  |
|------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| `AdaskoTheBeAsT.Dapper.GraphQL.Extensions.PostgreSql.NextIdentity`           | 🐘 `AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions.PostgreSqlIdentity.NextIdentity`                                                    |
| `AdaskoTheBeAsT.Dapper.GraphQL.Extensions.PostgreSql.NextIdentityAsync`      | 🐘 `AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions.PostgreSqlIdentity.NextIdentityAsync`                                               |
| _not available in 1.x_                                                       | 🦅 `AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions.OracleIdentity.NextIdentity[Async]` (**✨ new**)                                         |

### Insert + identity-retrieval extensions (called *with* the insert)

| 1.x extension                                                              | 2.x extension                                                                                                                                |
|-----------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `SqlInsertContextExtensions.ExecuteWithPostgreSqlIdentity[Async]`           | 🐘 `AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions.SqlInsertContextPostgreSqlExtensions.ExecuteWithPostgreSqlIdentity[Async]`           |
| `SqlInsertContextExtensions.ExecuteWithSqlIdentity[Async]`                  | 🗄️ `AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions.SqlInsertContextSqlServerExtensions.ExecuteWithSqlServerIdentity[Async]` (**✏️ renamed**) |
| `SqlInsertContextExtensions.ExecuteWithSqliteIdentity[Async]`               | 🪶 `AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Extensions.SqlInsertContextSqliteExtensions.ExecuteWithSqliteIdentity[Async]`                       |
| _not available in 1.x_                                                      | 🐬 `AdaskoTheBeAsT.Dapper.GraphQL.MySql.Extensions.SqlInsertContextMySqlExtensions.ExecuteWithMySqlIdentity[Async]` (**✨ new**)              |
| _not available in 1.x_                                                      | 🦅 `AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions.SqlInsertContextOracleExtensions.ExecuteWithOracleIdentity[Async]` (**✨ new**)           |

### ✏️ The one rename you can't avoid

`ExecuteWithSqlIdentity` → `ExecuteWithSqlServerIdentity`

This name was ambiguous in 1.x ("which SQL?"). 2.x consistently names every
vendor extension after the vendor. 🏷️

---

## 5️⃣ Step 5 — Wrap your `DbConnection` with vendor options

`SqlBuilderOptions.ParameterPrefix` controls whether generated SQL uses
`@param` or `:param`. To pick the right one:

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle;

public sealed class OracleConnectionFactory(OracleSqlBuilderOptions options)
{
    public IDbConnection Create()
    {
        var raw = new OracleConnection(_connectionString);
        return raw.WithDapperGraphQlOptions(options); // : prefix
    }
}
```

For vendors that use `@` (PostgreSQL, SQL Server, MySQL, SQLite) wrapping is
optional but recommended for consistency. 👍 For Oracle it's required,
otherwise the parameter prefix in generated SQL will not match what the
Oracle ADO provider expects. ⚠️

`OracleSqlBuilderOptions`, `PostgreSqlSqlBuilderOptions`,
`SqlServerSqlBuilderOptions`, `MySqlSqlBuilderOptions`, and
`SqliteSqlBuilderOptions` are registered as singletons by the matching
`AddDapperGraphQl<Vendor>` extension.

> 💡 **Oracle bonus:** if you use named parameters with Dapper on Oracle, also
> consider `BindByNameOracleConnection` shipped in
> `AdaskoTheBeAsT.Dapper.GraphQL.Oracle` which forces named-parameter binding
> on every `OracleCommand`.

---

## 🔁 Side-by-side examples

### 🐘 PostgreSQL

**1.x** ⛔

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;
using AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection;

services.AddDapperGraphQL(o =>
{
    o.AddQueryBuilder<Person, PersonQueryBuilder>();
});

var nextId = PostgreSql.NextIdentity(db, (Person p) => p.Id);

var personId = SqlBuilder
    .Insert(person)
    .ExecuteWithPostgreSqlIdentity(db, p => p.Id);
```

**2.x** ✅

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql;
using AdaskoTheBeAsT.Dapper.GraphQL.PostgreSql.Extensions;

services.AddDapperGraphQlPostgreSql(o =>
{
    o.AddQueryBuilder<Person, PersonQueryBuilder>();
});

var nextId = PostgreSqlIdentity.NextIdentity(db, (Person p) => p.Id);

var personId = SqlBuilder
    .Insert(person)
    .ExecuteWithPostgreSqlIdentity(db, p => p.Id);
```

### 🗄️ SQL Server

**1.x** ⛔

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;

var rowId = SqlBuilder
    .Insert(person)
    .ExecuteWithSqlIdentity<int>(db);
```

**2.x** ✅

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer;
using AdaskoTheBeAsT.Dapper.GraphQL.SqlServer.Extensions;

services.AddDapperGraphQlSqlServer(o => { /* … */ });

var rowId = SqlBuilder
    .Insert(person)
    .ExecuteWithSqlServerIdentity<int>(db);   // ✏️ renamed
```

### 🪶 SQLite

**1.x** ⛔

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.Extensions;

var rowId = SqlBuilder
    .Insert(person)
    .ExecuteWithSqliteIdentity(db);
```

**2.x** ✅

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite;
using AdaskoTheBeAsT.Dapper.GraphQL.Sqlite.Extensions;

services.AddDapperGraphQlSqlite(o => { /* … */ });

var rowId = SqlBuilder
    .Insert(person)
    .ExecuteWithSqliteIdentity(db);
```

### 🐬 MySQL — ✨ new in 2.0

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL.MySql;
using AdaskoTheBeAsT.Dapper.GraphQL.MySql.Extensions;

services.AddDapperGraphQlMySql(o => { /* … */ });

var rowId = SqlBuilder
    .Insert(person)
    .ExecuteWithMySqlIdentity<int>(db);
```

### 🦅 Oracle — ✨ new in 2.0

```csharp
using AdaskoTheBeAsT.Dapper.GraphQL;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle;
using AdaskoTheBeAsT.Dapper.GraphQL.Oracle.Extensions;

services.AddDapperGraphQlOracle(o => { /* … */ });

// Wrap with options so the ":" parameter prefix is used.
using var conn = new OracleConnection(connStr)
    .WithDapperGraphQlOptions(serviceProvider.GetRequiredService<OracleSqlBuilderOptions>());

var personId = OracleIdentity.NextIdentity(conn, (Person p) => p.Id);

var personId2 = SqlBuilder
    .Insert(person)
    .ExecuteWithOracleIdentity(conn, p => p.Id);
```

> 🦅 Oracle uses sequence naming convention `<TYPENAME>_<COLUMN>_SEQ`
> (e.g. `PERSON_ID_SEQ`). Make sure the sequence exists; the helper just
> emits `SELECT <SEQ>.NEXTVAL FROM DUAL` (or `.CURRVAL` after an insert).

---

## ✅ Verifying the upgrade

After updating package references:

1. 🔨 **Compile.** Any remaining 1.x call sites surface as
   `CS0234 / CS1061` errors with a clear pointer to which symbol moved.
2. 🧪 **Run unit tests** to confirm SQL generation still matches.
3. 🐳 **Run the integration tests** (`docker` required) to confirm identity
   retrieval works end-to-end with your real database.

If you hit something this guide doesn't cover, please open an issue at
[github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.GraphQL/issues](https://github.com/AdaskoTheBeAsT/AdaskoTheBeAsT.Dapper.GraphQL/issues)
with a minimal repro — we'll add the case to this guide. 🙏

---

## ❓ FAQ

**❔ Can I keep using `AddDapperGraphQL` for backwards compatibility?**
No. The `AdaskoTheBeAsT.Dapper.GraphQL.ServiceCollection` package is gone in
2.0. The vendor-specific extensions take its place, and there is no compat
shim. ⛔

**❔ Can I install more than one vendor package side-by-side (e.g. SQL Server
+ PostgreSQL in the same app)?**
Yes! ✅ Each vendor package contributes its own DI extension, options class,
and namespace. You can call multiple `AddDapperGraphQl<Vendor>` registrations
on the same `IServiceCollection`. Just resolve the right
`*SqlBuilderOptions` when wrapping a connection.

**❔ Why was `ExecuteWithSqlIdentity` renamed to
`ExecuteWithSqlServerIdentity`?**
With five vendors in the family, `Sql` was no longer a meaningful identifier.
The new name follows the consistent
`ExecuteWith<Vendor>Identity` convention (PostgreSql, SqlServer, MySql,
Sqlite, Oracle). 🏷️

**❔ Do I need the core package as an explicit reference?**
No — every vendor package brings the core in transitively. A direct
reference is harmless if you prefer it for clarity. 🤷

**❔ What .NET versions are supported in 2.0?**
`net8.0`, `net9.0`, `net10.0`, plus `net472`, `net48`, `net481` for legacy
.NET Framework apps. 🟣

---

**Made with ❤️ — happy upgrading!** 🚀
