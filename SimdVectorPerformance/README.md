# SIMD Vector Benchmark

This folder contains code from my talk "Speedrun into Massive Data".

Notes from Terje (@terjew):
I updated the existing benchmarks in a few ways:
- Massively increased the number of elements in the array, from 50_000 to around 5_000_000.
- Extracted the innards of `SumLinqSimdNaive` so it can be reused from other methods
- Updated SumForLoop to actually do an array lookup, not just sum the counter
- Changed the contents of the array so the sum doesn't overflow for large counts

I also added some new benchmarks:
- `SumForLoopUnrolled` simply unrolls the loop by 16 and does 16 additions per iteration.
- `SumForLoopSpan` uses a Span to access the array
- `SumTaskThreaded` spawns n tasks, each summing a portion of the array.
- `SumTaskThreadedSimd` spawns n tasks, but uses `SumLinqSimdNaive` to sum the portion of the array in each task.

Here are the results from the benchmark on various machines I have access to:

### Thinkpad T14S i7-1355U (AVX-512, 2 performance cores, 8 efficiency cores)
| Method               | Mean       | Error    | StdDev    | Median     | Ratio | RatioSD | Rank |
|--------------------- |-----------:|---------:|----------:|-----------:|------:|--------:|-----:|
| SumForLoop           | 1,873.9 us | 37.13 us |  91.08 us | 1,842.0 us |  1.00 |    0.07 |    5 |
| SumForLoopUnrolled   | 3,886.3 us | 77.42 us | 182.50 us | 3,904.5 us |  2.08 |    0.14 |    7 |
| SumForLoopSpan       | 1,769.7 us | 32.51 us |  58.63 us | 1,777.4 us |  0.95 |    0.05 |    5 |
| SumLinqThreaded      |   834.5 us | 16.19 us |  15.15 us |   837.6 us |  0.45 |    0.02 |    2 |
| SumLinqThreadedSimd  |   465.6 us |  9.09 us |  13.33 us |   467.9 us |  0.25 |    0.01 |    1 |
| SumLinq              | 1,291.2 us | 29.33 us |  86.48 us | 1,287.7 us |  0.69 |    0.06 |    4 |
| SumPLinq             | 2,407.4 us | 47.59 us |  39.74 us | 2,404.7 us |  1.29 |    0.06 |    6 |
| SumLinqSimdNaive     |   907.5 us | 13.96 us |  12.38 us |   906.0 us |  0.49 |    0.02 |    3 |
| SumLinqSimdBetter    |   925.1 us |  8.28 us |   7.34 us |   924.8 us |  0.49 |    0.02 |    3 |
| SumLinqSimdUnrolled4 |   914.2 us | 17.69 us |  19.66 us |   914.0 us |  0.49 |    0.03 |    3 |

```