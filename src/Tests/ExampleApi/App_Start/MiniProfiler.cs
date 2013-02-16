[assembly: WebActivator.PreApplicationStartMethod(
	typeof(ExampleApi.App_Start.MiniProfilerPackage), "PreStart")]

[assembly: WebActivator.PostApplicationStartMethod(
	typeof(ExampleApi.App_Start.MiniProfilerPackage), "PostStart")]


namespace ExampleApi.App_Start 
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using StackExchange.Profiling;
    using StackExchange.Profiling.MVCHelpers;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public static class MiniProfilerPackage
    {
        public static void PreStart()
        {

            // Be sure to restart you ASP.NET Developement server, this code will not run until you do that. 

            //TODO: See - _MINIPROFILER UPDATED Layout.cshtml
            //      For profiling to display in the UI you will have to include the line @StackExchange.Profiling.MiniProfiler.RenderIncludes() 
            //      in your master layout

            //TODO: Non SQL Server based installs can use other formatters like: new StackExchange.Profiling.SqlFormatters.InlineFormatter()
            MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();

			//TODO: To profile a standard DbConnection: 
			// var profiled = new ProfiledDbConnection(cnn, MiniProfiler.Current);

            //TODO: If you are profiling EF code first try: 
			MiniProfilerEF.Initialize();

            //Make sure the MiniProfiler handles BeginRequest and EndRequest
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));

            //Setup profiler for Controllers via a Global ActionFilter
            GlobalFilters.Filters.Add(new ProfilingActionFilter());

			// You can use this to check if a request is allowed to view results
            MiniProfiler.Settings.Results_Authorize = request => true;

            // the list of all sessions in the store is restricted by default, you must return true to allow it
            MiniProfiler.Settings.Results_List_Authorize = request => true;
        }

        public static void PostStart()
        {
            // Intercept ViewEngines to profile all partial views and regular views.
            // If you prefer to insert your profiling blocks manually you can comment this out
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (var item in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) => MiniProfiler.Start();
            context.EndRequest += (sender, e) => MiniProfiler.Stop();
        }

        public void Dispose() { }
    }
}
