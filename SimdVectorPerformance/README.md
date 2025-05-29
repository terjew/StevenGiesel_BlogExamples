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

| Method                      | Mean        | Error     | StdDev    | Ratio | RatioSD | Rank |
|---------------------------- |------------:|----------:|----------:|------:|--------:|-----:|
| SumTaskThreaded             |   160.30 us |  0.702 us |  0.656 us |  0.18 |    0.00 |    2 |
| SumTaskThreadedSimd         |    46.87 us |  0.359 us |  0.336 us |  0.05 |    0.00 |    1 |
| SumTaskThreadedSimdUnrolled |    47.78 us |  0.331 us |  0.309 us |  0.05 |    0.00 |    1 |
| SumForLoop                  |   907.48 us | 12.078 us | 10.707 us |  1.00 |    0.02 |    6 |
| SumForLoopUnrolled          | 2,007.38 us | 21.444 us | 20.059 us |  2.21 |    0.03 |    7 |
| SumForLoopSpan              |   914.18 us |  9.328 us |  7.789 us |  1.01 |    0.01 |    6 |
| SumLinq                     |   336.39 us |  6.013 us |  4.695 us |  0.37 |    0.01 |    4 |
| SumPLinq                    |   405.60 us |  2.762 us |  2.448 us |  0.45 |    0.01 |    5 |
| SumLinqSimdNaive            |   256.83 us |  4.781 us |  3.733 us |  0.28 |    0.01 |    3 |
| SumLinqSimdBetter           |   269.20 us |  4.538 us |  4.245 us |  0.30 |    0.01 |    3 |
| SumLinqSimdUnrolled4        |   246.47 us |  4.886 us |  6.849 us |  0.27 |    0.01 |    3 |

## Razer Blade Pro 2019 i7-8750H (AVX2, 6 cores)
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

## Mac Mini M1 (AdvSIMD, 8 cores)
ConcurrencyFactor = 6

| Method                      | Mean       | Error    | StdDev   | Ratio | Rank |
|---------------------------- |-----------:|---------:|---------:|------:|-----:|
| SumForLoop                  | 1,874.4 us |  1.03 us |  0.96 us |  1.00 |    7 |
| SumForLoopUnrolled          | 3,627.6 us |  1.44 us |  1.20 us |  1.94 |    9 |
| SumForLoopSpan              | 1,886.3 us |  0.38 us |  0.36 us |  1.01 |    7 |
| SumLinq                     | 1,015.0 us |  0.44 us |  0.39 us |  0.54 |    6 |
| SumPLinq                    | 2,181.4 us | 24.63 us | 21.84 us |  1.16 |    8 |
| SumLinqSimdNaive            |   788.2 us |  0.14 us |  0.12 us |  0.42 |    5 |
| SumLinqSimdBetter           |   409.9 us |  7.24 us |  6.77 us |  0.22 |    3 |
| SumLinqSimdUnrolled4        |   318.0 us |  0.46 us |  0.41 us |  0.17 |    2 |
| SumTaskThreaded             |   556.3 us |  7.18 us |  6.72 us |  0.30 |    4 |
| SumTaskThreadedSimd         |   284.6 us |  3.51 us |  3.28 us |  0.15 |    1 |
| SumTaskThreadedSimdUnrolled |   278.8 us |  1.80 us |  1.59 us |  0.15 |    1 |

