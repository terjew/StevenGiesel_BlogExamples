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

## Thinkpad T14S i7-1355U (AVX-512, 2 performance cores, 8 efficiency cores)
### ConcurrencyFactor=128

| Method                      | Mean       | Error     | StdDev    | Ratio | RatioSD | Rank |
|---------------------------- |-----------:|----------:|----------:|------:|--------:|-----:|
| SumTaskThreaded             |   666.1 us |  13.17 us |   8.71 us |  0.37 |    0.03 |    2 |
| SumTaskThreadedSimd         |   347.6 us |   7.58 us |   5.01 us |  0.19 |    0.01 |    1 |
| SumTaskThreadedSimdUnrolled |   347.1 us |  10.30 us |   6.81 us |  0.19 |    0.01 |    1 |
| SumForLoop                  | 1,815.0 us | 217.53 us | 143.88 us |  1.01 |    0.11 |    5 |
| SumForLoopUnrolled          | 3,887.4 us | 293.38 us | 194.05 us |  2.15 |    0.19 |    7 |
| SumForLoopSpan              | 1,649.2 us |  56.65 us |  37.47 us |  0.91 |    0.07 |    5 |
| SumLinq                     | 1,171.3 us |  31.76 us |  21.01 us |  0.65 |    0.05 |    4 |
| SumPLinq                    | 2,302.1 us |  45.88 us |  24.00 us |  1.28 |    0.10 |    6 |
| SumLinqSimdNaive            |   893.5 us |  22.14 us |  14.64 us |  0.50 |    0.04 |    3 |
| SumLinqSimdBetter           |   927.1 us |  20.25 us |  13.40 us |  0.51 |    0.04 |    3 |
| SumLinqSimdUnrolled4        |   902.1 us |  26.81 us |  17.73 us |  0.50 |    0.04 |    3 |

## Stationary gaming PC i9-14900KF (AVX2, 8 performance cores, 16 efficiency cores)), 
### ConcurrencyFactor=128
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
ConcurrencyFactor = 32

| Method                      | Mean       | Error    | StdDev   | Ratio | Rank |
|---------------------------- |-----------:|---------:|---------:|------:|-----:|
| SumTaskThreaded             |   489.8 us |  3.49 us |  3.26 us |  0.26 |    4 |
| SumTaskThreadedSimd         |   280.4 us |  1.05 us |  0.93 us |  0.15 |    1 |
| SumTaskThreadedSimdUnrolled |   277.4 us |  0.87 us |  0.82 us |  0.15 |    1 |
| SumForLoop                  | 1,876.9 us |  0.62 us |  0.52 us |  1.00 |    7 |
| SumForLoopUnrolled          | 3,622.2 us |  1.75 us |  1.55 us |  1.93 |    9 |
| SumForLoopSpan              | 1,882.9 us |  0.32 us |  0.30 us |  1.00 |    7 |
| SumLinq                     | 1,013.4 us |  0.44 us |  0.41 us |  0.54 |    6 |
| SumPLinq                    | 2,145.8 us | 13.61 us | 12.07 us |  1.14 |    8 |
| SumLinqSimdNaive            |   784.7 us |  0.11 us |  0.10 us |  0.42 |    5 |
| SumLinqSimdBetter           |   408.4 us |  1.85 us |  1.64 us |  0.22 |    3 |
| SumLinqSimdUnrolled4        |   317.6 us |  0.57 us |  0.48 us |  0.17 |    2 |

