# AdaskoTheBeAsT.Dapper.GraphQL - Coding Conventions for AI Agents

This document captures project-specific coding conventions and analyzer rules enforced by SonarCloud and the local Roslyn analyzer pipeline (StyleCop, Meziantou, Roslynator, CodeCracker, Microsoft.CodeAnalysis.NetAnalyzers, AwesomeAssertions, Moq.Analyzers). Follow these when adding or modifying code to avoid re-introducing the issues that were swept in PR #6.

## Multi-targeting notice

The library targets `net10.0;net9.0;net8.0;net481;net48;net472`. Modern API suggestions from analyzers (CA1510, MA0028, MA0089, RCS1197) must be guarded with `#if NET8_0_OR_GREATER` (or the relevant feature TFM) so the .NET Framework targets still compile. Do not introduce a NuGet polyfill just to silence an analyzer.

```csharp
#if NET8_0_OR_GREATER
ArgumentNullException.ThrowIfNull(arg);
sb.AppendJoin(',', items);
#else
if (arg == null) throw new ArgumentNullException(nameof(arg));
sb.Append(string.Join(',', items));
#endif
```

## Test conventions

| Rule | Required style |
| --- | --- |
| Moq1410 | Always `new Mock<T>(MockBehavior.Strict)`. Never `Loose` or the parameterless ctor. |
| CC0061 | Test methods that return `Task`/`Task<T>` MUST end with `Async` suffix. |
| CC0021 | Use `nameof(Person)` (not the string `"Person"`) when referring to types in scope. |
| CA2263 | Tests that intentionally call the non-generic `(Type, ...)` overload must store the type in a local variable first, e.g. `var modelType = typeof(Foo); sut.AddType(modelType);`. Inline `typeof(...)` triggers the analyzer because it can see the compile-time type. |
| FAA0001 / FAA0002 | Use AwesomeAssertions idioms: `.Should().Be(x)`, `.Should().BePositive()`, `.Should().ContainSingle()`, `.Should().Throw<T>()` etc. Avoid `Assert.Equal/Single/True/Throws` from xUnit. |
| xUnit v3 | The new test stack uses `xunit.v3` (>= .NET 8); .NET Framework still uses `xunit` 2.x. Reference `Xunit.TestContext.Current.CancellationToken` (MA0040) when calling async APIs that accept a `CancellationToken`. |

## Collections

| Rule | Style |
| --- | --- |
| IDE0028 | Prefer collection expressions: `new List<int>()` → `[]`, `new List<T> { a, b }` → `[a, b]`. |
| MA0016 | Public/internal properties and parameters should be typed `IList<T>`, `IReadOnlyList<T>`, `IDictionary<K,V>` etc. — never `List<T>` / `Dictionary<K,V>`. |
| CA1861 | Hoist constant array arguments into `private static readonly` fields when the method is called repeatedly inside a hot path or loop: `private static readonly string[] _names = ["a", "b"];`. |

## Strings, dictionaries, comparers

| Rule | Style |
| --- | --- |
| MA0002 | Always pass `StringComparer.Ordinal` (or `OrdinalIgnoreCase`) to `ToDictionary`, `ToHashSet`, `GroupBy`, `OrderBy`, `Distinct`, `Contains`, `Equals` etc. when the keys are strings. |
| MA0076 | Do not put non-`IFormattable` objects in interpolated strings without explicit `.ToString(CultureInfo.InvariantCulture)`. For numeric or `Guid` formatting in interpolations, use `FormattableString.Invariant($"...")` or call `.ToString(CultureInfo.InvariantCulture)`. |
| MA0028 / RCS1197 | Prefer `StringBuilder.AppendJoin(sep, items)` over `sb.Append(string.Join(sep, items))`. Wrap with `#if NET8_0_OR_GREATER`. |
| MA0089 | Prefer `string.Join(',', items)` (char overload) over `string.Join(",", items)`. Wrap with `#if NET8_0_OR_GREATER` because the char overload is unavailable on .NET Framework. |
| MA0003 | Always pass named arguments to boolean/literal parameters: `Throws(act, isAggregate: false)`. |
| MA0040 | When calling `*Async` methods that accept a `CancellationToken`, always pass one. In tests use `Xunit.TestContext.Current.CancellationToken`. |
| MA0042 | Never use blocking `Execute`/`Query`/`Wait`/`.Result` inside an `async` method or in async test helpers. Use the `*Async` variant and `await` it. |
| RCS1266 | Use raw string literals `"""..."""` for multi-line strings (GraphQL queries, SQL fragments). |

## Method conventions

| Rule | Style |
| --- | --- |
| MA0038 / CA1822 | Mark helper methods `static` when they don't use instance state. Applies especially to `TestFixture` helpers and small extension shims. |
| CA1510 | Use `ArgumentNullException.ThrowIfNull(arg)` instead of `if (arg == null) throw new ArgumentNullException(nameof(arg));`. Wrap with `#if NET8_0_OR_GREATER`. |
| CA2012 | Never call `.Result` / `.GetAwaiter().GetResult()` on a `ValueTask`. Either `await` it or convert via `.AsTask().Result` if the call is on a known-completed instance. |
| RCS1058 | Use compound assignment: `x = x + y` → `x += y`. |
| RCS1163 / RCS1175 | Remove unused parameters (including unused `this` on extension methods); if the parameter is contractually required, move the method to be a normal static helper. |
| RCS1212 | Remove redundant assignments — local variables that are written but never read should be deleted. |
| RCS1196 | Call extension methods as instance methods: `obj.MyExt()` not `MyExt(obj)`. |
| CC0014 | Use a ternary expression where it improves readability: `if (cond) x = a; else x = b;` → `x = cond ? a : b;`. |
| CC0065 | No trailing whitespace, including blank lines inside raw string literals. |
| SA1201 | Order class members: fields → constructors → properties → methods (StyleCop ordering). When you add a private helper to a constructor body, place it after all properties. |

## DB-specific integration tests

The `test/integ/AdaskoTheBeAsT.Dapper.GraphQL.{Db}.IntegrationTest/` projects are near-identical clones (MySql, Oracle, PostgreSql, Sqlite, SqlServer). When fixing an analyzer issue in one of them, **always apply the same fix to the other four**. Models, Repositories, EntityMappers, GraphQL types, and TestFixture follow the same shape across all five.

## Coverage and Sonar workflow

- The Sonar PR analysis is refreshed only when a push lands. Local coverage uses `dotnet-coverage` + `reportgenerator` per `coverage.settings.xml` / `.runsettings`.
- The unit test project uses `xunit.v3` on modern TFMs; integration projects use a mix depending on TFM. Don't reference `xunit.v3` from a `net48`-only project.
- New unit tests for the core library belong in `test/unit/AdaskoTheBeAsT.Dapper.GraphQL.Test/`; use the `FakeDbConnection`/`FakeDbTransaction`/`FakeDbCommand`/`FakeDbParameterCollection` helpers already in `DapperGraphQlConnectionTests.cs` to avoid Dapper hitting a real DB.

## Existing pragmas

Existing `#pragma warning disable` directives in checked-in source were left in place when this convention was authored (e.g. `MA0051` for purposely long test methods, `CS8620` on Dapper's nullable-array argument). Do not add new ones — refactor instead.
