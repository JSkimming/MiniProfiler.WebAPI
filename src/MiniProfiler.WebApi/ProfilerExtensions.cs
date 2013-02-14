namespace MiniProfiler.WebApi
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using StackExchange.Profiling;

    /// <summary>
    /// The profiler extensions.
    /// </summary>
    internal static class ProfilerExtensions
    {
        /// <summary>
        /// Serializes the supplied <paramref name="profiler"/> to a compressed base 64 encoded string.
        /// </summary>
        /// <param name="profiler">The profiler to serialize.</param>
        /// <returns>The serialized <paramref name="profiler"/>.</returns>
        public static string Serialize(this MiniProfiler profiler)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");

            string serializedProfiler = JsonConvert.SerializeObject(profiler);
            byte[] data = Encoding.UTF8.GetBytes(serializedProfiler).Compress();
            return Convert.ToBase64String(data);
        }

        public static MiniProfiler Deserialize(string serializedProfiler)
        {
            if (string.IsNullOrWhiteSpace(serializedProfiler)) throw new ArgumentNullException("serializedProfiler");

            byte[] data = Convert.FromBase64String(serializedProfiler).Decompress();
            string serializedprofiler = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<MiniProfiler>(serializedprofiler);
        }

        /// <summary>
        /// We don't actually know the start milliseconds, but lets 
        /// take it as zero being the start of the current head
        /// </summary>
        /// <param name="timing">the timing data</param>
        /// <param name="newStartMilliseconds">new Start Milliseconds.</param>
        public static void UpdateStartMillisecondTimingsToAbsolute(this Timing timing, decimal newStartMilliseconds)
        {
            if (timing == null)
                return;

            UpdateStartMillisecondTimingsByDelta(timing, newStartMilliseconds - timing.StartMilliseconds);
        }

        /// <summary>
        /// Delta is added to the existing StartMillisecondsValue
        /// Recursive method
        /// </summary>
        /// <param name="timing">The timing.</param>
        /// <param name="deltaMilliseconds">The delta Milliseconds.</param>
        public static void UpdateStartMillisecondTimingsByDelta(this Timing timing, decimal deltaMilliseconds)
        {
            if (timing == null)
                return;

            timing.StartMilliseconds += deltaMilliseconds;
            if (timing.Children != null)
            {
                foreach (var child in timing.Children)
                {
                    UpdateStartMillisecondTimingsByDelta(child, deltaMilliseconds);
                }
            }

            if (timing.SqlTimings != null)
            {
                foreach (var child in timing.SqlTimings)
                {
                    child.StartMilliseconds += deltaMilliseconds;
                }
            }
        }
    }
}
