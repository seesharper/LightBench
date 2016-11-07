namespace LightBench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// A super simple benchmarking tool.
    /// </summary>
    public static class Benchmark
    {
        /// <summary>
        /// Executes the given <see cref="action"/>. 
        /// </summary>
        /// <param name="action">A delegate that represents the code to be benchmarked.</param>
        /// <param name="numberOfRuns">The number of runs to execute.</param>
        /// <param name="name">The name/description of the benchmark.</param>
        /// <param name="warmup">Indicates whether we should perform warmup.</param>
        /// <returns></returns>
        public static Report Run(Action action, int numberOfRuns, string name, bool warmup = true)
        {
            if (warmup)
            {
                action();
            }

            List<double> times = new List<double>();

            var report = StartReport();
            report.Name = name;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < numberOfRuns; i++)
            {
                sw.Restart();
                try
                {
                    action();
                    times.Add(sw.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    report.Exceptions.Add(e);
                }

            }
            EndReport(report);
            CollectResult(times.ToArray(), report);
            return report;
        }

        /// <summary>
        /// Monitor a given <param name="action"></param> and calls the <paramref name="render"/> delegate after each run.
        /// </summary>
        /// <param name="action">A delegate that represents the code to be benchmarked.</param>
        /// <param name="duration">A <see cref="TimeSpan"/> that represents how long we should monitor the action.</param>
        /// <param name="thinkTime">A function that returns a <see cref="TimeSpan"/> that represents the time to "think" between runs.</param>
        /// <param name="name">A function delegate that returns the name/description of the benchmark.</param>
        /// <param name="render">An action delegate that passes the current <see cref="Report"/> object allowing it to be rendered.</param>
        /// <param name="warmup">Indicates whether we should perform warmup.</param>
        public static void Monitor(Action action, TimeSpan duration, Func<TimeSpan> thinkTime, Func<string> name, Action<Report> render, bool warmup = true)
        {
            var startTime = DateTime.Now;

            if (warmup)
            {
                action();
            }
            List<double> times = new List<double>();
            var report = StartReport();
            report.Duration = duration;
            Stopwatch sw = Stopwatch.StartNew();
            while ((DateTime.Now - startTime) < duration)
            {
                try
                {
                    sw.Restart();
                    action();
                    times.Add(sw.ElapsedMilliseconds);
                    CollectResult(times.ToArray(), report);
                    TimeSpan currentThinkTime = thinkTime();
                    report.ThinkTime = currentThinkTime;
                    report.Name = name();
                    render(report);
                    Thread.Sleep(currentThinkTime);
                }
                catch (Exception e)
                {
                    report.Exceptions.Add(e);
                }
            }
            EndReport(report);
            render(report);
        }

        private static void EndReport(Report report)
        {
            report.MemoryEnd = GC.GetTotalMemory(false);
            report.MemoryAfterCollect = GC.GetTotalMemory(true);
            report.IsComplete = true;
        }

        private static Report StartReport()
        {
            var report = new Report();
            report.Started = DateTime.Now;
            report.MemoryStart = GC.GetTotalMemory(true);
            return report;
        }

        private static void CollectResult(double[] times, Report report)
        {
            report.Times = times;
            report.NumberOfRuns = times.Length;
            report.StandardDeviation = CalculateStandardDeviation(times);
            report.Total = times.Sum();
            report.Longest = times.Max();
            report.Shortest = times.Min();
            report.Mean = times.Average();
            report.RelativeStandardDeviation = CalculateRelativeStandardDeviation(report.Mean, report.StandardDeviation);
            report.MemoryCurrent = GC.GetTotalMemory(false);
        }

        private static double CalculateStandardDeviation(double[] numbers)
        {
            double average = numbers.Average();
            double sumOfSquaresOfDifferences = numbers.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / numbers.Length);
            return sd;
        }

        private static double CalculateRelativeStandardDeviation(double mean, double standardDeviation)
        {
            if (mean == 0)
            {
                return 0;
            }

            return standardDeviation / mean;

        }
    }

    public class Report
    {
        public DateTime Started;
        public TimeSpan Duration;
        public TimeSpan ThinkTime;
        public double Total;
        public int NumberOfRuns;
        public double StandardDeviation;

        public double RelativeStandardDeviation;

        public bool IsComplete;
        public double Longest;
        public double Shortest;
        public double Mean;
        public string Name;
        public long MemoryStart;
        public long MemoryCurrent;
        public long MemoryEnd;
        public long MemoryAfterCollect;
        public readonly List<Exception> Exceptions = new List<Exception>();
        public double[] Times;

        public int[] CreateHistogram(int totalBuckets)
        {
            var min = Times.Min();
            var max = Times.Max();
            int[] buckets = new int[totalBuckets];

            var bucketSize = (max - min) / totalBuckets;

            foreach (var value in Times)
            {
                int bucketIndex = 0;
                if (bucketSize > 0.0)
                {
                    bucketIndex = (int)((value - min) / bucketSize);
                    if (bucketIndex == totalBuckets)
                    {
                        bucketIndex--;
                    }
                }
                buckets[bucketIndex]++;
            }
            return buckets;
        }

        public static int[] NormalizeHistogram(int[] histogram, int resolution)
        {
            var max = histogram.Max();
            if (max == 0)
            {
                return histogram;
            }

            double normalizeFactor = (double)max / (double)resolution;
            int[] normalizedValues = new int[histogram.Length];

            for (int i = 0; i < histogram.Length; i++)
            {
                int value = histogram[i];
                int normalizedValue = (int)(value / normalizeFactor);
                if (normalizedValue == 0 && value > 0)
                {
                    normalizedValue = 1;
                }
                normalizedValues[i] = normalizedValue;
            }

            return normalizedValues;
        }

        public double Percentile(double excelPercentile)
        {
            var sequence = Times.ToArray();
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * excelPercentile + 1;
            if (n == 1d) return sequence[0];
            if (n == N) return sequence[N - 1];
            int k = (int)n;
            double d = n - k;
            return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*********************");
            sb.AppendLine(Name);
            sb.AppendLine("*********************");
            sb.AppendLine($"Started: {Started}");
            if (IsComplete)
            {
                sb.AppendLine($"Status: Completed ({DateTime.Now})");
            }
            else
            {
                sb.AppendLine($"Status: Not Completed, thinking for {ThinkTime.TotalSeconds} seconds");
            }

            sb.AppendLine($"Number of runs: {NumberOfRuns}");
            sb.AppendLine($"Total time: {Total:0.00} ms");
            sb.AppendLine($"------ Memory ------");
            sb.AppendLine($"Memory (start): \t{MemoryStart} bytes");

            if (IsComplete)
            {
                sb.AppendLine($"Memory (end): \t\t{MemoryEnd} bytes");
                sb.AppendLine($"Memory (after collect):\t{MemoryAfterCollect} bytes");
                sb.AppendLine($"Memory (allocated) \t{MemoryEnd - MemoryStart} bytes");
            }
            else
            {
                sb.AppendLine($"Memory (current): \t{MemoryCurrent} bytes");
            }

            sb.AppendLine();
            sb.AppendLine("------ Frequency Distribution Table ------");
            sb.AppendLine($"Mean: {Mean:0.00} ms +/- {StandardDeviation:0.00} ms ({RelativeStandardDeviation * 100:0.00}%)");
            sb.AppendLine($"Min: {Shortest:0.00} ms");
            var histogram = CreateHistogram(10);
            var normalizedHistogram = NormalizeHistogram(histogram, 50);
            for (int i = 0; i < normalizedHistogram.Length; i++)
            {
                sb.Append($"({histogram[i]})\t").Append('*', normalizedHistogram[i]);
                sb.AppendLine();

            }
            sb.AppendLine($"Max: {Longest:0.00} ms");

            sb.AppendLine();
            sb.AppendLine("Percentage of the requests served within a certain time (ms)");
            double[] percentiles = new[] { 0.5, 0.66, 0.75, 0.80, 0.90, 0.95, 0.98, 0.99, 1 };
            foreach (var percentile in percentiles)
            {
                sb.AppendLine($"{percentile * 100:0}%\t\t{Percentile(percentile):0.00}");
            }

            if (IsComplete)
            {
                sb.AppendLine();
                sb.AppendLine("Benchmark Completed!");
            }

            return sb.ToString();
        }
    }

    public static class EnumerableExtensions
    {
        public static T PickRandomItem<T>(this IEnumerable<T> enumerable)
        {
            var random = new Random();
            var array = enumerable.ToArray();
            int index = random.Next(array.Length - 1);
            return array[index];
        }
    }
}
