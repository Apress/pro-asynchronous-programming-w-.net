using System.Web.Http;
using System.Web.Mvc;

namespace $rootnamespace$.Areas.RouteDebugger
{
    public class RouteDebuggerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "RouteDebugger";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "RouteDebugger_default",
                "rd/{action}",
                new { controller = "RouteDebugger", action = "Index" });

            // Replace some of the default routing implementations with our custom debug
            // implementations.
            RouteDebuggerConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}
