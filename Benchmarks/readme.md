Interop benchmarks
------------------

### Invoke one .NET function 10⁶ times

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1916 (1909/November2019Update/19H2)
Intel Core i7-4600U CPU 2.10GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=5.0.301
  [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
```
|     Method |         Mean |      Error |     StdDev |
|----------- |-------------:|-----------:|-----------:|
|       Jint |  1,532.22 ms |  29.179 ms |  34.736 ms |
|         V8 | 13,714.55 ms | 191.816 ms | 170.040 ms |
| ChakraCore |  1,140.05 ms |  22.340 ms |  20.896 ms |
|     Roslyn |     25.74 ms |   0.719 ms |   2.099 ms |
|  Moonsharp |    875.94 ms |  15.590 ms |  13.820 ms |
|  Pythonnet |  2,974.47 ms |  34.115 ms |  28.487 ms |


### Invoke 100 .NET functions each one time

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1916 (1909/November2019Update/19H2)
Intel Core i7-4600U CPU 2.10GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=5.0.301
  [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
```
|     Method |         Mean |        Error |       StdDev |
|----------- |-------------:|-------------:|-------------:|
|       Jint |     343.8 μs |      6.68 μs |      5.57 μs |
|         V8 |   1,509.0 μs |     23.09 μs |     30.83 μs |
| ChakraCore |     306.8 μs |      3.90 μs |      3.64 μs |
|     Roslyn | 137,687.1 μs | 10,899.45 μs | 32,137.27 μs |
|  Moonsharp |     717.9 μs |    121.55 μs |    334.80 μs |
|  Pythonnet |     782.6 μs |      8.72 μs |      7.73 μs |


JS engines benchmarks
---------------------

### SunSpider's md5

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1916 (1909/November2019Update/19H2)
Intel Core i7-4600U CPU 2.10GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=5.0.301
  [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
```
|     Method |       Mean |     Error |    StdDev |
|----------- |-----------:|----------:|----------:|
|       Jint | 222.336 ms | 3.8474 ms | 3.5988 ms |
| ChakraCore |  13.108 ms | 0.2573 ms | 0.2149 ms |
|         V8 |   9.945 ms | 0.1885 ms | 0.2171 ms |

### SunSpider's 3d-cube

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1916 (1909/November2019Update/19H2)
Intel Core i7-4600U CPU 2.10GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=5.0.301
  [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
```
|     Method |      Mean |    Error |    StdDev |
|----------- |----------:|---------:|----------:|
|       Jint | 480.54 ms | 9.382 ms | 10.804 ms |
| ChakraCore |  23.04 ms | 0.449 ms |  0.630 ms |
|         V8 |  14.52 ms | 0.276 ms |  0.245 ms |

### Recursive Fibonacci number computation

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1916 (1909/November2019Update/19H2)
Intel Core i7-4600U CPU 2.10GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=5.0.301
  [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
  DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
```
|     Method |        Mean |     Error |    StdDev |
|----------- |------------:|----------:|----------:|
|       Jint | 2,645.31 ms | 21.574 ms | 18.015 ms |
|         V8 |    12.95 ms |  0.170 ms |  0.142 ms |
| ChakraCore |    23.83 ms |  0.218 ms |  0.171 ms |

