using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Optimization;

namespace $rootnamespace$.Areas.RouteDebugger
{
    public static class RouteDebuggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new InspectHandler());

            var services = config.Services;

            config.Services.Replace(
                typeof(IHttpActionInvoker),
                new InspectActionInvoker(services.GetActionInvoker()));

            config.Services.Replace(
                typeof(IHttpActionSelector),
                new InspectActionSelector(services.GetActionSelector()));

            config.Services.Replace(
                typeof(IHttpControllerSelector),
                new InspectControllerSelector(services.GetHttpControllerSelector()));

            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/routedebugger").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/knockout-{version}.js",
                "~/Areas/RouteDebugger/Scripts/detect.js"));
        }
    }
}
