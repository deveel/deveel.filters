[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/deveel/deveel.filters/cicd.yml?logo=github&label=build)](https://github.com/deveel/deveel.filters/actions)
[![Codecov](https://img.shields.io/codecov/c/github/deveel/deveel.filters?logo=codecov&token=37K687Z96N)](https://codecov.io/gh/deveel/deveel.filters)
[![NuGet Version](https://img.shields.io/nuget/v/Deveel.Filters?logo=nuget&label=nuget)](https://www.nuget.org/packages/Deveel.Filters)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Deveel.Filters?logo=nuget&label=downloads)](https://www.nuget.org/packages/Deveel.Filters)
[![GitHub License](https://img.shields.io/github/license/deveel/deveel.filters)](https://github.com/deveel/deveel.filters/blob/main/LICENSE)

<img src="deveel-logo.png" alt="Deveel Logo" width="64" align="right" />

# Deveel Filters

A .NET library that provides a **structured, serializable, and extensible** model for building dynamic filter expressions. Filters can be evaluated in-memory, compiled to LINQ expressions, converted to Dynamic LINQ queries, or serialized to BSON for MongoDB — all from a single, unified API.

## Scope

The library addresses the need for a **portable, technology-agnostic filter representation** that can be:

- **Built** programmatically using a fluent factory API
- **Evaluated** synchronously or asynchronously against any .NET object
- **Compiled** to strongly-typed `Expression<Func<T, bool>>` lambda expressions
- **Converted** to Dynamic LINQ strings for use with Entity Framework or other LINQ providers
- **Serialized** to JSON, BSON, or other formats for network transmission and storage
- **Validated** via web-friendly DTOs for API scenarios

## Motivation

In many applications — web APIs, data pipelines, reporting engines, multi-tenant SaaS platforms — filters need to be **defined at runtime** rather than compiled into the code. Common approaches include:

- **Hand-rolled string parsing**: error-prone, hard to validate, and not type-safe
- **`System.Linq.Expressions` directly**: powerful but verbose for simple predicates, not serializable, and tightly coupled to LINQ semantics
- **OData / GraphQL**: heavyweight protocol-level solutions when you just need a lightweight filter model

**Deveel Filters** fills the gap by providing a **minimal, composable expression tree** purpose-built for filtering:

| Concern | `System.Linq.Expressions` | Deveel Filters |
|---|---|---|
| Serializable to JSON/BSON | ❌ | ✅ |
| Built-in async evaluation | ❌ | ✅ |
| Web DTO + validation layer | ❌ | ✅ (WebModel package) |
| Dynamic LINQ integration | Manual | ✅ (DynamicLinq package) |
| MongoDB BSON support | ❌ | ✅ (MongoBson package) |
| Concise factory API | ❌ | ✅ |

## Supported Frameworks

- .NET 6.0
- .NET 7.0
- .NET 8.0

## Packages

| Package | Description | NuGet |
|---|---|---|
| **Deveel.Filters** | Core filter expression model, factory API, evaluation, and lambda compilation | [![NuGet](https://img.shields.io/nuget/v/Deveel.Filters?logo=nuget)](https://www.nuget.org/packages/Deveel.Filters) |
| **Deveel.Filters.WebModel** | Web DTOs and validation for API scenarios | [![NuGet](https://img.shields.io/nuget/v/Deveel.Filters.WebModel?logo=nuget)](https://www.nuget.org/packages/Deveel.Filters.WebModel) |
| **Deveel.Filters.DynamicLinq** | Conversion of filter expressions to Dynamic LINQ queries | [![NuGet](https://img.shields.io/nuget/v/Deveel.Filters.DynamicLinq?logo=nuget)](https://www.nuget.org/packages/Deveel.Filters.DynamicLinq) |
| **Deveel.Filters.MongoBson** | Serialization/deserialization of filters to/from MongoDB BSON documents | [![NuGet](https://img.shields.io/nuget/v/Deveel.Filters.MongoBson?logo=nuget)](https://www.nuget.org/packages/Deveel.Filters.MongoBson) |

## Installation

Install the core package (and any extension packages you need) via the .NET CLI or the NuGet Package Manager.

### .NET CLI

```bash
# Core package
dotnet add package Deveel.Filters

# Optional extensions
dotnet add package Deveel.Filters.WebModel
dotnet add package Deveel.Filters.DynamicLinq
dotnet add package Deveel.Filters.MongoBson
```

### Package Manager Console

```powershell
Install-Package Deveel.Filters

# Optional extensions
Install-Package Deveel.Filters.WebModel
Install-Package Deveel.Filters.DynamicLinq
Install-Package Deveel.Filters.MongoBson
```

### PackageReference (MSBuild)

```xml
<PackageReference Include="Deveel.Filters" Version="1.0.*" />
```

## Usage

All filter expressions are created through static factory methods on the `FilterExpression` class (namespace `Deveel.Filters`). The API uses three building blocks: **variables** (references to object properties), **constants** (literal values), and **operators** (equality, comparison, logical, etc.) that combine them into a filter tree.

### Building a Simple Equality Filter

The simplest filter compares a single property to a constant value. Use `FilterExpression.Variable` to reference a property path and `FilterExpression.Constant` to wrap a literal value, then combine them with an operator such as `Equal`.

```csharp
using Deveel.Filters;

// "x.Name == "Antonello""
var filter = FilterExpression.Equal(
    FilterExpression.Variable("x.Name"),
    FilterExpression.Constant("Antonello"));
```

The variable name is prefixed with `x.` because the default parameter name used during evaluation is `x`. The dot-separated path after the prefix maps to property access on the target object.

### Evaluating a Filter In-Memory

Once a filter is built, you can evaluate it directly against an object using the `Evaluate` extension method. The method compiles the filter into a lambda, invokes it, and returns `true` or `false`.

```csharp
var filter = FilterExpression.Equal(
    FilterExpression.Variable("x.Name"),
    FilterExpression.Constant("Antonello"));

bool match = filter.Evaluate(new { Name = "Antonello" }); // true
```

The generic overload `Evaluate<T>` infers the type from the argument, so you can pass anonymous objects, POCOs, or records without specifying the type explicitly.

### Combining Filters with Logical Operators

Multiple conditions can be joined with `And` or `Or` to form compound filters. The example below matches objects where _both_ the `Name` equals `"Antonello"` **and** the `Age` is greater than or equal to `30`.

```csharp
var filter = FilterExpression.And(
    FilterExpression.Equal(
        FilterExpression.Variable("x.Name"),
        FilterExpression.Constant("Antonello")),
    FilterExpression.GreaterThanOrEqual(
        FilterExpression.Variable("x.Age"),
        FilterExpression.Constant(30)));

bool match = filter.Evaluate(new { Name = "Antonello", Age = 35 }); // true
```

You can nest `And` / `Or` nodes arbitrarily to express complex boolean logic.

### Navigating Nested Properties

Variable paths support dot notation to reach into nested objects. In the following example, `x.Address.City` navigates from the root object through the `Address` property to its `City` sub-property.

```csharp
var filter = FilterExpression.And(
    FilterExpression.Equal(
        FilterExpression.Variable("x.Name"),
        FilterExpression.Constant("Antonello")),
    FilterExpression.Equal(
        FilterExpression.Variable("x.Address.City"),
        FilterExpression.Constant("Rome")));

var person = new { Name = "Antonello", Address = new { City = "Rome" } };
bool match = filter.Evaluate(person); // true
```

There is no depth limit — you can traverse as many levels as your object graph requires.

### Using Function Filters

Function filters invoke a named method on a variable's value. This is useful for string operations like `StartsWith`, `EndsWith`, or `Contains` that don't map to simple comparison operators.

```csharp
var filter = FilterExpression.Function(
    FilterExpression.Variable("x.Name"),
    "StartsWith",
    new[] { FilterExpression.Constant("Anto") });

bool match = filter.Evaluate(new { Name = "Antonello" }); // true
```

The function name must match a public instance method on the runtime type of the property. Arguments are passed as an array of filter expressions (typically constants).

### Negating a Filter

Wrap any filter in `FilterExpression.Not` to invert its result. The example below matches all objects whose `Status` is _not_ `"Archived"`.

```csharp
var filter = FilterExpression.Not(
    FilterExpression.Equal(
        FilterExpression.Variable("x.Status"),
        FilterExpression.Constant("Archived")));

bool match = filter.Evaluate(new { Status = "Active" }); // true
```

Negation can be applied to any expression node — simple comparisons, logical combinations, or function calls.

### Asynchronous Evaluation

For I/O-bound or high-throughput scenarios you can evaluate filters asynchronously with `EvaluateAsync`. This compiles the filter into an `async` lambda that returns `Task<bool>`.

```csharp
var filter = FilterExpression.Equal(
    FilterExpression.Variable("x.Name"),
    FilterExpression.Constant("Antonello"));

bool match = await filter.EvaluateAsync(new { Name = "Antonello" });
```

The async path avoids blocking the calling thread and integrates naturally with `async` / `await` pipelines.

### Compiling to a Lambda Expression

When you need to pass a filter to a LINQ provider (e.g., Entity Framework Core), compile it to a strongly-typed `Expression<Func<T, bool>>` using `AsLambda<T>`. The resulting expression tree can be translated to SQL or any other query language by the provider.

```csharp
var filter = FilterExpression.Equal(
    FilterExpression.Variable("x.Name"),
    FilterExpression.Constant("Antonello"));

// Compile to Expression<Func<Person, bool>> for use with LINQ / EF Core
Expression<Func<Person, bool>> predicate = filter.AsLambda<Person>();

// Use it in a LINQ query
var results = dbContext.People.Where(predicate).ToList();
```

Because the result is a standard `Expression<Func<T, bool>>`, it is fully compatible with any library that accepts LINQ expression trees.

### Converting to Dynamic LINQ (requires `Deveel.Filters.DynamicLinq`)

If you prefer string-based dynamic queries (useful when the target type is not known at compile time), the **DynamicLinq** extension package converts a filter into a Dynamic LINQ predicate string that can be applied to any `IQueryable`.

```csharp
using Deveel.Filters;
using Deveel.Filters.DynamicLinq;

var filter = FilterExpression.Equal(
    FilterExpression.Variable("x.Name"),
    FilterExpression.Constant("Antonello"));

// Convert to a Dynamic LINQ string and apply to an IQueryable
var results = dbContext.People
    .Where(filter.ToDynamicLinq())
    .ToList();
```

This is particularly helpful in multi-tenant or plugin-based architectures where filter definitions are stored externally (e.g., in a database or configuration file) and applied at runtime against varying entity types.

## Performance

The library is designed for high throughput with minimal allocations. Below are representative benchmarks from `BenchmarkDotNet`:

_Deveel.Filters v1.0.0-c18 / 2025-07-21_

| Method | Runtime | Mean | Allocated |
|---|---|---:|---:|
| BuildSimpleEqual | .NET 7.0 | 846 ns | 824 B |
| BuildSimpleEqual | .NET 6.0 | 1,784 ns | 824 B |
| BuildLogicalAndOfComplexObject | .NET 7.0 | 2,180 ns | 1,592 B |
| BuildAsyncSimpleEqual | .NET 7.0 | 4,489 ns | 3,616 B |
| BuildSimpleEqualDynamicLinq | .NET 7.0 | 34,458 ns | 25,238 B |
| BuildLogicalAndOfComplexObjectDynamicLinq | .NET 7.0 | 49,006 ns | 28,331 B |

> Full benchmark results are available in the [`test/FiltersBenchmark`](test/FiltersBenchmark) project.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on [GitHub](https://github.com/deveel/deveel.filters).

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

Copyright © 2023–2026 Antonello Provenzano.
