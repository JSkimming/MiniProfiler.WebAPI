namespace StackExchange.Profiling
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handler to add profiling to a Web API service.
    /// </summary>
    public class MiniProfilerHandler : DelegatingHandler
    {
        /// <summary>
        /// The header name used to serialize the data.
        /// </summary>
        public const string ResultsHeaderName = "MiniProfilerResults";

        /// <summary>
        /// Sets the profiler results in the <paramref name="response"/> headers if profiling is enabled.
        /// </summary>
        /// <param name="response">The HTTP response message to send to the client.</param>
        /// <returns>The HTTP response message to send to the client.</returns>
        private static HttpResponseMessage SetProfilerHeadder(HttpResponseMessage response)
        {
            // Stop the profiling.
            MiniProfiler.Stop();

            MiniProfiler profiler = MiniProfiler.Current;

            // If profiling is disabled, do nothing.
            if (profiler == null) return response;

            // Add the profiler results to the response headers.
            response.Headers.Add(ResultsHeaderName, profiler.Serialize());

            return response;
        }

        /// <summary>
        /// Adds profiling to the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns <see cref="Task{T}"/>. The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> was null.</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException("request");

            // If there was no request for the profiling results, just continue as normal.
            IEnumerable<string> profilerHeadders;
            if (!request.Headers.TryGetValues(MiniProfilerMessageHandler.RequestHeaderName, out profilerHeadders))
                return base.SendAsync(request, cancellationToken);

            return base.SendAsync(request, cancellationToken)
                       .Then(response => SetProfilerHeadder(response), cancellationToken);
        }
    }
}
