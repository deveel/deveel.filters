![Codecov](https://img.shields.io/codecov/c/github/deveel/deveel.filters?logo=codecov&token=37K687Z96N)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/deveel/deveel.filters/cicd.yml?logo=github)

# Deveel Filters

Provides a generic and structured approach to create filters to be applied to a data source, supporting multiple target frameworks and flexible filter expressions.

## Features

- **Dynamic Filters**: Create filters dynamically using a fluent API.
- **LINQ Support**: Use LINQ expressions to build complex filters.
- **Asynchronous Evaluation**: Evaluate filters asynchronously for better performance in I/O-bound scenarios.
- **Web Integration**: DTOs and validation for web APIs to ensure data integrity.
- **Extensible**: Easily extendable to support custom filter types and operations.
- **Performance Optimized**: Designed for high performance with minimal allocations.
- **Cross-Platform**: Compatible with .NET 6.0, 7.0, and 8.0.
- **Benchmarked**: Performance benchmarks available to compare different filter implementations.

## Motivation

Deveel Filters is designed to provide a robust and flexible filtering mechanism that can be used across various applications, from web APIs to data processing tasks. It aims to simplify the creation and evaluation of filters while maintaining high performance and extensibility.

### Why Not System.Linq.Expressions?

While it is powerful, it can be complex and verbose for many common filtering scenarios. 

* Also, the set of expressions in the System.Linq.Expressions namespace is limited to LINQ operations, which may not cover all use cases.
* Furthermore, the _System.Linq.Expressions_ framework provides a more extensive set of operations, but it can be cumbersome to use for simple filtering tasks.
* _Deveel Filters_ provides a more user-friendly API and supports a wider range of filter types and operations, making it easier to work with complex data structures and scenarios.
* It also allows for more straightforward integration with web APIs and other data sources, providing a consistent and efficient way to handle filtering across different contexts.
* The Filter objects are serializable to JSON, BSON and other formats, making it easy to transmit filters over the network or store them in a database.

## Supported Frameworks

- .NET 6.0
- .NET 7.0  
- .NET 8.0

## Packages

### Core Package
- **Deveel.Filter.Model** - Core filtering models and utilities

### Extension Packages
- **Deveel.Filter.Model.Web** - Web DTOs and validation for APIs
- **Deveel.Filter.DynamicLinq** - LINQ-based filter implementations

## Installation

### .NET CLI
```bash
dotnet add package Deveel.Filter.Model
```
### Package Manager
```bash
Install-Package Deveel.Filter.Model
```
## Usage

### Creating a Filter
```csharp
var filter = Filter.Equal(
	Filter.Variable("x.name"),
	Filter.Constant("Antonello"),
	FilterType.Equals);

var result = filter.Evaluate(new {name = "antonello"});
	Filter.Equal(
		Filter.Variable("x.age"),
		Filter.Constant(30),
		FilterType.Equals));

var result = filter.Evaluate(new {name = "antonello", age = 30});
var filter = Filter.Function("x.name", "StartsWith", Filter.Constant("Anto"));

var result = filter.Evaluate(new {name = "Antonello"});
```

Navigating through complex objects:

```csharp
var filter = Filter.LogicalAnd(
	Filter.Equal(
		Filter.Variable("x.name"),
		Filter.Constant("Antonello"),
		FilterType.Equals),
	Filter.Equal(
		Filter.Variable("x.age"),
		Filter.Constant(30),
		FilterType.Equals));

var result = filter.Evaluate(new {name = "Antonello", age = 30});

var filter = Filter.LogicalAnd(
	Filter.Equal(
		Filter.Variable("x.name"),
		Filter.Constant("Antonello"),
		FilterType.Equals),
	Filter.Equal(
		Filter.Variable("x.address.city"),
		Filter.Constant("Rome"),
		FilterType.Equals));

var result = filter.Evaluate(new {name = "Antonello", address = new {city = "Rome"}});

```

### Asynchronous Evaluation
```csharp
var filter = Filter.Equal(
	Filter.Variable("x.name"),
	Filter.Constant("Antonello"),
	FilterType.Equals);

	var result = await filter.EvaluateAsync(new {name = "Antonello"});
```

### Convert to LINQ Expression

```csharp
var filter = Filter.Equal(
	Filter.Variable("x.name"),
	Filter.Constant("Antonello"),
	FilterType.Equals);

	var lambda = filter.AsLambda<Person>();
	var result = lambda(new Person { Name = "Antonello" });
```

## Performances

_Deveel.Filters v1.0.0-c18 / 2025-07-21_

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1635/22H2/2022Update/SunValley2)
Intel Core i7-10510U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.100-preview.2.23157.25
  [Host]   : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.16 (6.0.1623.17311), X64 RyuJIT AVX2


|                                    Method |      Job |  Runtime |        Mean |     Error |      StdDev |      Median | Rank |   Gen0 |   Gen1 | Allocated |
|------------------------------------------ |--------- |--------- |------------:|----------:|------------:|------------:|-----:|-------:|-------:|----------:|
|                          BuildSimpleEqual | .NET 7.0 | .NET 7.0 |    846.2 ns |  16.85 ns |    31.24 ns |    848.6 ns |    1 | 0.1965 |      - |     824 B |
|                       BuildSimpleEqualOfT | .NET 7.0 | .NET 7.0 |    866.3 ns |  16.91 ns |    27.78 ns |    870.5 ns |    2 | 0.1965 |      - |     824 B |
|                       BuildSimpleEqualOfT | .NET 6.0 | .NET 6.0 |  1,241.9 ns |  79.50 ns |   216.28 ns |  1,142.7 ns |    3 | 0.1945 |      - |     824 B |
|                          BuildSimpleEqual | .NET 6.0 | .NET 6.0 |  1,784.2 ns | 301.84 ns |   889.98 ns |  1,302.8 ns |    4 | 0.1965 |      - |     824 B |
|            BuildLogicalAndOfComplexObject | .NET 7.0 | .NET 7.0 |  2,180.0 ns |  43.33 ns |    79.23 ns |  2,196.5 ns |    5 | 0.3777 |      - |    1592 B |
|            BuildLogicalAndOfComplexObject | .NET 6.0 | .NET 6.0 |  2,649.2 ns |  52.48 ns |    66.37 ns |  2,648.5 ns |    6 | 0.3777 |      - |    1592 B |
|                     BuildAsyncSimpleEqual | .NET 7.0 | .NET 7.0 |  4,489.1 ns |  89.75 ns |   122.85 ns |  4,469.1 ns |    7 | 0.8621 |      - |    3616 B |
|                  BuildAsyncSimpleEqualOfT | .NET 7.0 | .NET 7.0 |  4,605.1 ns |  91.85 ns |   193.74 ns |  4,623.4 ns |    7 | 0.8621 |      - |    3616 B |
|                     BuildAsyncSimpleEqual | .NET 6.0 | .NET 6.0 |  6,464.4 ns | 164.59 ns |   469.59 ns |  6,404.1 ns |    8 | 1.0223 |      - |    4328 B |
|                  BuildAsyncSimpleEqualOfT | .NET 6.0 | .NET 6.0 |  7,186.7 ns | 274.47 ns |   760.57 ns |  6,899.9 ns |    9 | 1.0300 |      - |    4328 B |
|            BuildSimpleEqualDynamicLinqOfT | .NET 7.0 | .NET 7.0 | 31,883.4 ns | 624.50 ns |   934.72 ns | 31,787.3 ns |   10 | 5.9814 |      - |   25246 B |
|               BuildSimpleEqualDynamicLinq | .NET 7.0 | .NET 7.0 | 34,457.7 ns | 615.90 ns |   756.38 ns | 34,311.7 ns |   11 | 5.9814 |      - |   25238 B |
|               BuildSimpleEqualDynamicLinq | .NET 6.0 | .NET 6.0 | 35,823.3 ns | 712.13 ns | 1,209.25 ns | 35,983.2 ns |   12 | 5.9814 |      - |   25231 B |
|            BuildSimpleEqualDynamicLinqOfT | .NET 6.0 | .NET 6.0 | 36,224.7 ns | 724.19 ns | 1,324.22 ns | 36,154.9 ns |   12 | 5.9814 | 0.0610 |   25239 B |
| BuildLogicalAndOfComplexObjectDynamicLinq | .NET 6.0 | .NET 6.0 | 44,806.9 ns | 886.78 ns | 1,021.22 ns | 45,005.1 ns |   13 | 6.7139 |      - |   28317 B |
| BuildLogicalAndOfComplexObjectDynamicLinq | .NET 7.0 | .NET 7.0 | 49,006.3 ns | 954.51 ns | 1,368.94 ns | 48,910.4 ns |   14 | 6.7139 |      - |   28331 B |