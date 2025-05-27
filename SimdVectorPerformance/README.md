# SIMD Vector Benchmark

This folder contains code from my talk "Speedrun into Massive Data".

Notes from Terje (@terjew):
I updated the existing benchmarks in a few ways:
- Massively increased the total number of elements in the array, from 50_000 to 5_000_000.
- Extracted the innards of `SumLinqSimdNaive` so it can be reused from other methods
- Updated SumForLoop to actually do an array lookup, not just sum the counter
- Changed the contents of the array so the sum doesn't overflow for large counts

I also added some new benchmarks:
- `SumForLoopUnrolled` simply unrolls the loop by 16 and does 16 additions per iteration.
- `SumForLoopSpan` uses a Span to access the array
- `SumTaskThreaded` spawns n tasks, each summing a portion of the array.
- `SumTaskThreadedSimd` spawns n tasks, but uses `SumLinqSimdNaive` to sum the portion of the array in each task.

Here are the results from the benchmark on various machines I have access to:

## Thinkpad T14S i7-1355U (AVX-512, 2 performance cores, 8 efficiency cores), ConcurrencyFactor=10

| Method               | Mean       | Error    | StdDev    | Median     | Ratio | RatioSD | Rank |
|--------------------- |-----------:|---------:|----------:|-----------:|------:|--------:|-----:|
| SumForLoop           | 1,873.9 us | 37.13 us |  91.08 us | 1,842.0 us |  1.00 |    0.07 |    5 |
| SumForLoopUnrolled   | 3,886.3 us | 77.42 us | 182.50 us | 3,904.5 us |  2.08 |    0.14 |    7 |
| SumForLoopSpan       | 1,769.7 us | 32.51 us |  58.63 us | 1,777.4 us |  0.95 |    0.05 |    5 |
| SumLinq              | 1,291.2 us | 29.33 us |  86.48 us | 1,287.7 us |  0.69 |    0.06 |    4 |
| SumPLinq             | 2,407.4 us | 47.59 us |  39.74 us | 2,404.7 us |  1.29 |    0.06 |    6 |
| SumLinqSimdNaive     |   907.5 us | 13.96 us |  12.38 us |   906.0 us |  0.49 |    0.02 |    3 |
| SumLinqSimdBetter    |   925.1 us |  8.28 us |   7.34 us |   924.8 us |  0.49 |    0.02 |    3 |
| SumLinqSimdUnrolled4 |   914.2 us | 17.69 us |  19.66 us |   914.0 us |  0.49 |    0.03 |    3 |
| SumTaskThreaded      |   834.5 us | 16.19 us |  15.15 us |   837.6 us |  0.45 |    0.02 |    2 |
| SumTaskThreadedSimd  |   465.6 us |  9.09 us |  13.33 us |   467.9 us |  0.25 |    0.01 |    1 |

## Stationary gaming PC i9-14900KF (AVX2, 8 performance cores, 16 efficiency cores)), 

### ConcurrencyFactor=10
| Method               | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Rank |
|--------------------- |------------:|----------:|----------:|------------:|------:|--------:|-----:|
| SumForLoop           |   929.38 us | 12.732 us |  9.940 us |   928.39 us |  1.00 |    0.01 |    7 |
| SumForLoopUnrolled   | 1,983.53 us | 36.258 us | 64.448 us | 1,953.56 us |  2.13 |    0.07 |    8 |
| SumForLoopSpan       |   861.95 us |  5.092 us |  4.514 us |   860.54 us |  0.93 |    0.01 |    6 |
| SumLinq              |   337.20 us |  4.879 us |  4.325 us |   336.13 us |  0.36 |    0.01 |    4 |
| SumPLinq             |   401.47 us |  1.770 us |  1.656 us |   402.06 us |  0.43 |    0.00 |    5 |
| SumLinqSimdNaive     |   247.05 us |  2.512 us |  2.350 us |   246.54 us |  0.27 |    0.00 |    3 |
| SumLinqSimdBetter    |   227.12 us |  0.645 us |  0.571 us |   227.21 us |  0.24 |    0.00 |    2 |
| SumLinqSimdUnrolled4 |   233.36 us |  3.112 us |  3.584 us |   232.45 us |  0.25 |    0.00 |    2 |
| SumTaskThreaded      |   231.50 us |  1.943 us |  1.723 us |   230.98 us |  0.25 |    0.00 |    2 |
| SumTaskThreadedSimd  |    69.79 us |  0.545 us |  0.510 us |    69.74 us |  0.08 |    0.00 |    1 |

### ConcurrencyFactor=30
| Method               | Mean        | Error     | StdDev    | Ratio | RatioSD | Rank |
|--------------------- |------------:|----------:|----------:|------:|--------:|-----:|
| SumTaskThreaded      |   203.38 us |  3.376 us |  3.158 us |  0.21 |    0.01 |    2 |
| SumTaskThreadedSimd  |    70.17 us |  0.488 us |  0.456 us |  0.07 |    0.00 |    1 |

## Razer Blade Pro 2019 i7-8750H (AVX2, 6 cores))
### ConcurrencyFactor=8

| Method               | Mean       | Error    | StdDev    | Median     | Ratio | RatioSD | Rank |
|--------------------- |-----------:|---------:|----------:|-----------:|------:|--------:|-----:|
| SumForLoop           | 2,038.8 us | 17.20 us |  14.36 us | 2,037.4 us |  1.00 |    0.01 |    4 |
| SumForLoopUnrolled   | 4,955.5 us | 33.53 us |  26.18 us | 4,957.6 us |  2.43 |    0.02 |    6 |
| SumForLoopSpan       | 2,007.9 us | 13.20 us |  11.02 us | 2,013.5 us |  0.98 |    0.01 |    4 |
| SumLinq              | 1,135.7 us |  9.89 us |   8.25 us | 1,135.8 us |  0.56 |    0.01 |    3 |
| SumPLinq             | 2,797.5 us | 87.63 us | 248.59 us | 2,735.7 us |  1.37 |    0.12 |    5 |
| SumLinqSimdNaive     |   811.9 us | 15.91 us |  23.82 us |   804.9 us |  0.40 |    0.01 |    2 |
| SumLinqSimdBetter    |   835.4 us | 15.29 us |  19.88 us |   830.0 us |  0.41 |    0.01 |    2 |
| SumLinqSimdUnrolled4 |   818.2 us | 15.74 us |  21.55 us |   808.6 us |  0.40 |    0.01 |    2 |
| SumTaskThreaded      |   596.3 us |  9.25 us |  14.66 us |   599.9 us |  0.29 |    0.01 |    1 |
| SumTaskThreadedSimd  |   562.7 us | 18.23 us |  53.74 us |   528.2 us |  0.28 |    0.03 |    1 |

## Macbook Pro 2013 i7-4558U (?) (AVX, 4 cores) (.NET 8)
### ConcurrencyFactor=4
| Method               | Mean       | Error    | StdDev   | Ratio | RatioSD | Rank |
|--------------------- |-----------:|---------:|---------:|------:|--------:|-----:|
| SumForLoop           | 2,980.4 us | 38.53 us | 36.05 us |  1.00 |    0.02 |    5 |
| SumForLoopUnrolled   | 5,115.6 us | 35.27 us | 31.27 us |  1.72 |    0.02 |    7 |
| SumForLoopSpan       | 2,967.3 us | 36.53 us | 34.17 us |  1.00 |    0.02 |    5 |
| SumLinq              | 1,560.2 us | 10.61 us |  9.41 us |  0.52 |    0.01 |    4 |
| SumPLinq             | 3,997.5 us | 43.80 us | 38.83 us |  1.34 |    0.02 |    6 |
| SumLinqSimdNaive     | 1,148.1 us |  8.16 us |  7.63 us |  0.39 |    0.01 |    3 |
| SumLinqSimdBetter    | 1,114.1 us | 18.16 us | 16.99 us |  0.37 |    0.01 |    3 |
| SumLinqSimdUnrolled4 | 1,111.7 us | 12.91 us | 12.08 us |  0.37 |    0.01 |    3 |
| SumTaskThreaded      |   990.6 us | 17.79 us | 17.47 us |  0.33 |    0.01 |    2 |
| SumTaskThreadedSimd  |   926.2 us |  3.25 us |  2.88 us |  0.31 |    0.00 |    1 |

```
