using System.Net;
using System.Net.Http;

namespace $rootnamespace$.Areas.RouteDebugger.Models
{
    public class InspectData
    {
        public InspectData(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(RequestHelper.ActionCache))
            {
                Action = request.Properties[RequestHelper.ActionCache] as ActionSelectionLog;
            }

            if (request.Properties.ContainsKey(RequestHelper.ControllerCache))
            {
                Controller = request.Properties[RequestHelper.ControllerCache] as ControllerSelectionInfo[];
            }

            if (request.Properties.ContainsKey(RequestHelper.RoutesCache))
            {
                Routes = request.Properties[RequestHelper.RoutesCache] as RouteInfo[];
            }

            if (request.Properties.ContainsKey(RequestHelper.RouteDataCache))
            {
                RouteData = request.Properties[RequestHelper.RouteDataCache] as RouteDataInfo;
            }

            if (request.Properties.ContainsKey(RequestHelper.SelectedController))
            {
                SelectedController = request.Properties[RequestHelper.SelectedController] as string;
            }
        }

        public ActionSelectionLog Action { get; set; }

        public ControllerSelectionInfo[] Controller { get; set; }

        public RouteInfo[] Routes { get; set; }

        public RouteDataInfo RouteData { get; set; }

        public HttpStatusCode RealHttpStatus { get; set; }

        public string SelectedController { get; set; }
    }
}
