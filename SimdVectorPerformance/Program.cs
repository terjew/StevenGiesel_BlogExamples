using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

Console.WriteLine(Environment.ProcessorCount);
//BenchmarkRunner.Run<ConcurrentBenchmarks>();
BenchmarkRunner.Run<Benchmarks>();

public class BenchmarkBase
{    
    protected static readonly int[] Values =
        Enumerable.Range(0, 5_000_000)
        .Select(i => i % 7) // 
        .ToArray();

    public static int SumLinqSimdNaiveImpl(ReadOnlySpan<int> values)
    {
        // The performant way of getting an array of vectors rather than doing it by hand
        var vectors = MemoryMarshal.Cast<int, Vector<int>>(values);
        var accVector = Vector<int>.Zero;

        foreach (var vector in vectors)
        {
            accVector += vector;
        }

        var remainder = 0;
        for (var i = values.Length - (values.Length % Vector<int>.Count); i < values.Length; i++)
        {
            remainder += values[i];
        }

        // Combine vector sum and remainder
        return Vector.Sum(accVector) + remainder;
    }

    public static int SumLinqSimdUnrolled4Impl(ReadOnlySpan<int> values)
    {
        var vectors = MemoryMarshal.Cast<int, Vector<int>>(values);
        var simdCount = vectors.Length;

        // Four accumulators to enable EVEN MORE ILP
        var acc1 = Vector<int>.Zero;
        var acc2 = Vector<int>.Zero;
        var acc3 = Vector<int>.Zero;
        var acc4 = Vector<int>.Zero;

        var i = 0;
        for (; i <= simdCount - 4; i += 4)
        {
            acc1 += vectors[i];
            acc2 += vectors[i + 1];
            acc3 += vectors[i + 2];
            acc4 += vectors[i + 3];
        }

        // Combine the accumulators
        var accVector = acc1 + acc2 + acc3 + acc4;

        // Handle remaining vectors if length % 4 != 0
        for (; i < simdCount; i++)
        {
            accVector += vectors[i];
        }

        // Handle remaining elements that didn't fit into a vector
        var remainingElements = values.Length % Vector<int>.Count;
        if (remainingElements > 0)
        {
            Span<int> lastVectorElements = stackalloc int[Vector<int>.Count];
            values[^remainingElements..].CopyTo(lastVectorElements);
            accVector += new Vector<int>(lastVectorElements);
        }

        return Vector.Sum(accVector);
    }
}

[RankColumn]
//These are useful when tuning the parameters, as the benchmarks will otherwise take ages to complete
//[MinIterationCount(5)]
//[MaxIterationCount(10)]
//[MinWarmupCount(5)]
//[MaxWarmupCount(10)]
public class ConcurrentBenchmarks : BenchmarkBase
{
    //Note: These are hardcoded for the Intel Core i9-14900K, which has 8 performance cores and 16 efficiency cores.
    public enum AffinityType
    {
        AllPerformanceCores =   unchecked((int)0b0000_0000_0000_0000_1111_1111_1111_1111), // all performance cores (SMT cores included)
        RealPerformanceCores =  unchecked((int)0b0000_0000_0000_0000_0101_0101_0101_0101), // all "real" performance cores (no SMT cores)
        SMTPerformanceCores =   unchecked((int)0b0000_0000_0000_0000_1010_1010_1010_1010), // Only the SMT cores from the performance cores
        EfficiencyCores =       unchecked((int)0b0111_1111_1111_1111_0000_0000_0000_0000), // All efficiency cores
        RealPlusEfficiency =    unchecked((int)0b0111_1111_1111_1111_0101_0101_0101_0101), // All "real" performance cores and all efficiency cores
        AllCores =              unchecked((int)0b1111_1111_1111_1111_1111_1111_1111_1111), // "All" cores (except one efficiency core)
    }

    //[Params(96,104,112,120,128,192,256)]
    //Note: Experimenting with this, I found that 128 seems to be the sweet spot for the Intel Core i9-14900K.
    public static int ConcurrencyFactor = 128;

    //Note: Experimenting with this on the Intel Core i9-14900K, I found that given a relatively high ConcurrencyFactor (above 64),
    //enabling all cores gives the best results for all multithreaded benchmarks.
    public static AffinityType Affinity = AffinityType.AllCores;

    static ConcurrentBenchmarks()
    {
        var process = Process.GetCurrentProcess();
        process.PriorityClass = ProcessPriorityClass.High;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            unchecked
            {
                if (Affinity != AffinityType.AllCores)
                    process.ProcessorAffinity = (IntPtr)(Affinity);
            }
        }
    }

    [Benchmark]
    public async Task<int> SumTaskThreaded()
    {

        int chunkSize = Values.Length / ConcurrencyFactor; //NOTE: assumes Values.Length is divisible by count

        var chuckSum = new Func<int, int>(i =>
        {
            var span = new ReadOnlySpan<int>(Values, i * chunkSize, chunkSize);
            int acc = 0;
            for (int j = 0; j < chunkSize; j++)
            {
                acc += span[j];
            }
            return acc;
        });

        var tasks = Enumerable.Range(0, ConcurrencyFactor)
            .Select(i => Task.Run(() => chuckSum(i)));
        var partSums = await Task.WhenAll(tasks);
        return partSums.Sum();
    }

    [Benchmark]
    public async Task<int> SumTaskThreadedSimd()
    {
        int chunkSize = Values.Length / ConcurrencyFactor; //NOTE: assumes Values.Length is divisible by count

        var chuckSum = new Func<int, int>(i =>
        {
            var span = new ReadOnlySpan<int>(Values, i * chunkSize, chunkSize);
            return SumLinqSimdNaiveImpl(span);
        });

        var tasks = Enumerable.Range(0, ConcurrencyFactor)
            .Select(i => Task.Run(() => chuckSum(i)));
        var partSums = await Task.WhenAll(tasks);
        return partSums.Sum();
    }

    [Benchmark]
    public async Task<int> SumTaskThreadedSimdUnrolled()
    {
        int chunkSize = Values.Length / ConcurrencyFactor; //NOTE: assumes Values.Length is divisible by count

        var chuckSum = new Func<int, int>(i =>
        {
            var span = new ReadOnlySpan<int>(Values, i * chunkSize, chunkSize);
            return SumLinqSimdUnrolled4Impl(span);
        });

        var tasks = Enumerable.Range(0, ConcurrencyFactor)
            .Select(i => Task.Run(() => chuckSum(i)));
        var partSums = await Task.WhenAll(tasks);
        return partSums.Sum();
    }
}

public class Benchmarks : ConcurrentBenchmarks
{ 
    [Benchmark(Baseline = true)]
    public int SumForLoop()
    {
        var acc = 0;
        for (var i = 0; i < Values.Length; i++)
            acc += Values[i];

        return acc;
    }

    [Benchmark]
    public int SumForLoopUnrolled()
    {
        var acc = 0;
        int chunkSize = 16; //16 * 32 = 512 bits, so maybe the compiler is clever and optimizes for us?
        int numChunks = Values.Length / chunkSize;
        var span = new ReadOnlySpan<int>(Values, 0, numChunks * chunkSize);

        for (var i = 0; i < span.Length; i += 16)
        {
            acc = acc + span[i + 0] + span[i + 1] + span[i + 2] + span[i + 3]
                      + span[i + 4] + span[i + 5] + span[i + 6] + span[i + 7]
                      + span[i + 8] + span[i + 9] + span[i + 10] + span[i + 11]
                      + span[i + 12] + span[i + 13] + span[i + 14] + span[i + 15]
                      ;
        }
        // Handle remaining elements if Values.Length is not a multiple of chunkSize
        for (int i = numChunks % chunkSize; i < Values.Length; i++)
        {
            acc += Values[i];
        }

        return acc;
    }

    [Benchmark]
    public int SumForLoopSpan()
    {
        var span = new ReadOnlySpan<int>(Values);
        var acc = 0;
        for (var i = 0; i < span.Length; i++)
            acc += span[i];

        return acc;
    }
    
    [Benchmark]
    public int SumLinq() => Values.Sum();

    [Benchmark]
    public int SumPLinq() => Values.AsParallel().Sum();

    [Benchmark]
    public int SumLinqSimdNaive()
    {
        return SumLinqSimdNaiveImpl(Values);
    }

    [Benchmark]
    public int SumLinqSimdBetter()
    {
        /*
         * In comparison to the naive version, this one is a bit more complex.
         *
         * Instead of only using "one" Vector<int> to sum the elements,
         * we use two vectors at a time and put the result into the acc vector.
         * This enables ILP (Instruction Level Parallelism) and
         * allows the CPU to execute multiple instructions
         */
        var spanAsVectors = MemoryMarshal.Cast<int, Vector<int>>(Values);
        var remainingElements = Values.Length % Vector<int>.Count;
        var accVector = Vector<int>.Zero;

        for (var i = 0; i < spanAsVectors.Length - 1; i += 2)
        {
            accVector += spanAsVectors[i] + spanAsVectors[i + 1];
        }

        if (spanAsVectors.Length % 2 == 1)
        {
            accVector += spanAsVectors[^1];
        }

        if (remainingElements > 0)
        {
            var startingLastElements = Values.Length - remainingElements;
            var remainingElementsSlice = Values[startingLastElements..];

            accVector += new Vector<int>(remainingElementsSlice);
        }

        return Vector.Sum(accVector);
    }

    [Benchmark]
    public int SumLinqSimdUnrolled4(){
        return SumLinqSimdUnrolled4Impl(Values);
    }

}