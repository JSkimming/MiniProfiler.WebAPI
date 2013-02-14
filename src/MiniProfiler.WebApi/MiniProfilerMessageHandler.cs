namespace MiniProfiler.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Profiling;

    /// <summary>
    /// Extension to <see cref="HttpClientHandler"/> to provide profiling across WebAPI web requests.
    /// </summary>
    public class MiniProfilerMessageHandler : HttpClientHandler
    {
        /// <summary>
        /// The request header name.
        /// </summary>
        public const string RequestHeaderName = "MiniProfilerRequestHeader";

        /// <summary>
        /// The <see cref="MiniProfiler"/> used for the request.
        /// </summary>
        private readonly MiniProfiler _profiler;

        /// <summary>
        /// Backing field for the <see cref="StepPrefix"/> property.
        /// </summary>
        private string _stepPrefix;

        /// <summary>
        /// If <c>true</c> a <see cref="MiniProfilerExtensions.Step"/> is added for the request.
        /// </summary>
        public bool AddStep { get; set; }

        /// <summary>
        /// The prefix for the <see cref="MiniProfilerExtensions.Step"/> name, used if <see cref="AddStep"/> is <c>true</c>.
        /// </summary>
        public string StepPrefix
        {
            get { return _stepPrefix; }
            set
            {
                _stepPrefix = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            }
        }

        /// <summary>
        /// Constructs the <see cref="MiniProfilerMessageHandler"/>.
        /// </summary>
        /// <param name="profiler">The profiler, if <c>null</c> the <see cref="MiniProfiler.Current"/> profiler is used.</param>
        /// <param name="addStep">If <c>true</c> a <see cref="MiniProfilerExtensions.Step"/> is added for the request.</param>
        /// <param name="stepPrefix">The prefix for the <see cref="MiniProfilerExtensions.Step"/> name, used if <see cref="AddStep"/> is <c>true</c>.</param>
        public MiniProfilerMessageHandler(MiniProfiler profiler = null, bool addStep = true, string stepPrefix = "WebAPI :")
        {
            _profiler = profiler ?? MiniProfiler.Current;
            AddStep = addStep;
            StepPrefix = stepPrefix;
        }

        /// <summary>
        /// Adds the profiling results fro the <paramref name="response"/> to the supplied <paramref name="profiler"/>.
        /// </summary>
        /// <param name="response">The response from a request.</param>
        /// <param name="profiler">The profiler to add the results to.</param>
        /// <param name="newStartMilliseconds">The start time used to update the remote timings.</param>
        internal static HttpResponseMessage AddRemoteProfilerResults(HttpResponseMessage response, MiniProfiler profiler, decimal newStartMilliseconds)
        {
            // Get the serialized results header.
            IEnumerable<string> miniProfilerResults;
            if (!response.Headers.TryGetValues(MiniProfilerHandler.ResultsHeaderName, out miniProfilerResults))
                return response;

            // Get the data.
            string resultsHeader = miniProfilerResults.First();
            MiniProfiler remoteProfiler = ProfilerExtensions.Deserialize(resultsHeader);

            // Update the timings of the remote profiler results.
            remoteProfiler.Root.UpdateStartMillisecondTimingsToAbsolute(newStartMilliseconds);

            profiler.AddProfilerResults(remoteProfiler);

            return response;
        }

        /// <summary>
        /// Adds profiling to the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>Returns <see cref="Task"/>. The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> was null.</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException("request");

            // Use the profiler passed to the constructor, or the current.
            MiniProfiler profiler = _profiler ?? MiniProfiler.Current;

            // If profiling is disabled, just continue as normal.
            if (profiler == null) return base.SendAsync(request, cancellationToken);

            decimal startTime = profiler.DurationMilliseconds;

            IDisposable step = null;
            if (AddStep)
            {
                step = profiler.Step(string.Format("{0} {1} {2}", StepPrefix, request.Method.Method, request.RequestUri));
            }

            // Add the request header.
            request.Headers.Add(RequestHeaderName, profiler.Id.ToString());

            // Make the request and then add the remote profiler results.
            Task<HttpResponseMessage> result =
                base.SendAsync(request, cancellationToken)
                    .Then(response => AddRemoteProfilerResults(response, profiler, startTime), cancellationToken);

            // If the step was not added just return, otherwise dispose the step.
            return step == null ? result : result.Finally(step.Dispose);
        }
    }
}
