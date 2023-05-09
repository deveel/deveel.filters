![Codecov](https://img.shields.io/codecov/c/github/deveel/deveel.filters?logo=codecov&token=37K687Z96N)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/deveel/deveel.filters/cicd.yml?logo=github)

# Deveel Filters
Provides a generic and structured approach to create filters to be applied to a data source.

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
```


```csharp
var filter = Filter.And(
	Filter.Equal(
		Filter.Variable("x.name"),
		Filter.Constant("Antonello"),
		FilterType.Equals),
	Filter.Equal(
		Filter.Variable("x.age"),
		Filter.Constant(30),
		FilterType.Equals));

var result = filter.Evaluate(new {name = "antonello", age = 30});
```

```csharp
var filter = Filter.Function("x.name", "StartsWith", Filter.Constant("Anto"));

var result = filter.Evaluate(new {name = "Antonello"});
```

## Performances

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