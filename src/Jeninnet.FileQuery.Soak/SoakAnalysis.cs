//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Soak;

/// <summary>
/// One managed-heap sample taken after a full collection.
/// </summary>
/// <param name="Iteration">The iteration at which the sample was taken.</param>
/// <param name="ManagedHeapBytes">The managed heap size in bytes.</param>
internal sealed record SoakSample(long Iteration, long ManagedHeapBytes);

/// <summary>
/// The measurements collected during the soak run.
/// </summary>
/// <param name="Iterations">Measured iterations.</param>
/// <param name="ExpectedMatches">The probe match count.</param>
/// <param name="ObservedMatches">The last iteration's match count.</param>
/// <param name="Times">Per-iteration durations in milliseconds.</param>
/// <param name="Samples">Managed-heap samples.</param>
/// <param name="AllocatedBytes">Total bytes allocated during the measured phase.</param>
/// <param name="ElapsedSeconds">Measured elapsed seconds.</param>
internal sealed record SoakMeasurement(
    long Iterations,
    int ExpectedMatches,
    int ObservedMatches,
    IReadOnlyList<double> Times,
    IReadOnlyList<SoakSample> Samples,
    long AllocatedBytes,
    double ElapsedSeconds
);

/// <summary>
/// The computed soak result.
/// </summary>
/// <param name="Iterations">Measured iterations.</param>
/// <param name="MatchesPerIteration">Files returned by every iteration.</param>
/// <param name="ThroughputPerSecond">Measured iterations per second.</param>
/// <param name="MeanMs">Mean iteration time in milliseconds.</param>
/// <param name="P50Ms">Median iteration time in milliseconds.</param>
/// <param name="P95Ms">95th percentile iteration time in milliseconds.</param>
/// <param name="FirstHalfMeanMs">Mean iteration time of the first half.</param>
/// <param name="SecondHalfMeanMs">Mean iteration time of the second half.</param>
/// <param name="HeapFirstBytes">First sampled heap size.</param>
/// <param name="HeapLastBytes">Last sampled heap size.</param>
/// <param name="HeapMinBytes">Smallest sampled heap size.</param>
/// <param name="HeapMaxBytes">Largest sampled heap size.</param>
/// <param name="HeapGrowthMegabytes">Growth between the first and last sample.</param>
/// <param name="HeapSlopeBytesPerIteration">Least-squares heap slope per iteration.</param>
/// <param name="AllocatedBytes">Total bytes allocated during the measured phase.</param>
/// <param name="Failures">The failed stability checks.</param>
internal sealed record SoakAnalysis(
    long Iterations,
    int MatchesPerIteration,
    double ThroughputPerSecond,
    double MeanMs,
    double P50Ms,
    double P95Ms,
    double FirstHalfMeanMs,
    double SecondHalfMeanMs,
    long HeapFirstBytes,
    long HeapLastBytes,
    long HeapMinBytes,
    long HeapMaxBytes,
    double HeapGrowthMegabytes,
    double HeapSlopeBytesPerIteration,
    long AllocatedBytes,
    IReadOnlyList<string> Failures
) {
    /// <summary>
    /// Gets a value indicating whether every stability check passed.
    /// </summary>
    public bool Passed => Failures.Count == 0;

    /// <summary>
    /// Computes the analysis and applies the stability rules.
    /// </summary>
    /// <param name="options">The soak options.</param>
    /// <param name="measurement">The collected measurements.</param>
    /// <returns>The computed analysis.</returns>
    public static SoakAnalysis Create(SoakOptions options, SoakMeasurement measurement) {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(measurement);

        var (iterations, expectedMatches, observedMatches, times, samples, allocatedBytes, elapsedSeconds) = measurement;

        ArgumentNullException.ThrowIfNull(times);
        ArgumentNullException.ThrowIfNull(samples);

        var failures = new List<string>();

        if(iterations < 10) {
            failures.Add($"Only {iterations} iterations completed; at least 10 are required.");
        }

        if(observedMatches != expectedMatches) {
            failures.Add(
                $"Match count changed during the run: expected {expectedMatches}, observed {observedMatches}.");
        }

        var sorted = times.Order().ToArray();
        var mean = sorted.Length > 0 ? sorted.Average() : 0;
        var p50 = Percentile(sorted, 0.50);
        var p95 = Percentile(sorted, 0.95);

        var half = sorted.Length / 2;
        var firstHalfMean = half > 0 ? sorted.Take(half).Average() : mean;
        var secondHalfMean = half > 0 ? sorted.Skip(sorted.Length - half).Average() : mean;

        if(firstHalfMean > 0 && secondHalfMean > firstHalfMean * 1.5) {
            failures.Add(
                $"Iteration time degraded from {firstHalfMean:F2} ms to {secondHalfMean:F2} ms (>{1.5:P0} of the first half).");
        }

        var heapFirst = samples.Count > 0 ? samples[0].ManagedHeapBytes : 0;
        var heapLast = samples.Count > 0 ? samples[^1].ManagedHeapBytes : 0;
        var heapMin = samples.Count > 0 ? samples.Min(static sample => sample.ManagedHeapBytes) : 0;
        var heapMax = samples.Count > 0 ? samples.Max(static sample => sample.ManagedHeapBytes) : 0;
        var growthMegabytes = (heapLast - heapFirst) / 1024.0 / 1024.0;

        if(growthMegabytes > options.MaxGrowthMegabytes) {
            failures.Add(
                $"Managed heap grew by {growthMegabytes:F2} MB (threshold {options.MaxGrowthMegabytes:F2} MB).");
        }

        var slope = CalculateSlope(samples);
        var throughput = elapsedSeconds > 0 ? iterations / elapsedSeconds : 0;

        return new SoakAnalysis(
            Iterations: iterations,
            MatchesPerIteration: observedMatches,
            ThroughputPerSecond: throughput,
            MeanMs: mean,
            P50Ms: p50,
            P95Ms: p95,
            FirstHalfMeanMs: firstHalfMean,
            SecondHalfMeanMs: secondHalfMean,
            HeapFirstBytes: heapFirst,
            HeapLastBytes: heapLast,
            HeapMinBytes: heapMin,
            HeapMaxBytes: heapMax,
            HeapGrowthMegabytes: growthMegabytes,
            HeapSlopeBytesPerIteration: slope,
            AllocatedBytes: allocatedBytes,
            Failures: failures);
    }

    /// <summary>
    /// Returns the requested percentile of a sorted array.
    /// </summary>
    /// <param name="sorted">The sorted values.</param>
    /// <param name="percentile">The percentile in the range 0..1.</param>
    /// <returns>The percentile value.</returns>
    private static double Percentile(double[] sorted, double percentile) {
        if(sorted.Length == 0) {
            return 0;
        }

        var index = (int)Math.Ceiling(percentile * sorted.Length) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
    }

    /// <summary>
    /// Calculates the least-squares slope of the heap samples.
    /// </summary>
    /// <param name="samples">The heap samples.</param>
    /// <returns>The slope in bytes per iteration, or 0 with fewer than two samples.</returns>
    private static double CalculateSlope(IReadOnlyList<SoakSample> samples) {
        if(samples.Count < 2) {
            return 0;
        }

        var meanX = samples.Average(static sample => (double)sample.Iteration);
        var meanY = samples.Average(static sample => (double)sample.ManagedHeapBytes);

        double numerator = 0;
        double denominator = 0;

        foreach(var sample in samples) {
            var deltaX = sample.Iteration - meanX;
            numerator += deltaX * (sample.ManagedHeapBytes - meanY);
            denominator += deltaX * deltaX;
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }
}
