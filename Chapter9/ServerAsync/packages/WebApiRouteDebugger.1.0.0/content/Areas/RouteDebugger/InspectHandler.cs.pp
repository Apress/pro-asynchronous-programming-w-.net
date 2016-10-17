using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using $rootnamespace$.Areas.RouteDebugger.Models;

namespace $rootnamespace$.Areas.RouteDebugger
{
    /// <summary>
    /// Inspect handler saves route inspect data and handler error.
    /// 
    /// If a request has inspect header, the handler will save all routes and route data to inspect data.
    /// 
    /// If the return response is not 200. That may cause by 500 or 404, handler extract Inspect data from
    /// request property and set back the the response. The original response status is saved to the inspect 
    /// data and actual return response is always 200.
    /// </summary>
    public class InspectHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.IsInspectRequest())
            {
                var config = GlobalConfiguration.Configuration;

                request.Properties[RequestHelper.RouteDataCache] =
                    new RouteDataInfo
                    {
                        RouteTemplate = request.GetRouteData().Route.RouteTemplate,
                        Data = request.GetRouteData().Values.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString())).ToArray()
                    };

                request.Properties[RequestHelper.RoutesCache] = config.Routes.Select(route =>
                    new RouteInfo
                    {
                        RouteTemplate = route.RouteTemplate,
                        Defaults = route.Defaults != null ? route.Defaults.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString())).ToArray() : null,
                        Constraints = route.Constraints != null ? route.Constraints.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString())).ToArray() : null,
                        DataTokens = route.DataTokens != null ? route.DataTokens.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString())).ToArray() : null,
                        Handler = route.Handler != null ? route.Handler.GetType().Name : null,
                        Picked = route.RouteTemplate == request.GetRouteData().Route.RouteTemplate
                    }).ToArray();

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var newRequest = response.RequestMessage;
                    var inspectData = new InspectData(newRequest);
                    inspectData.RealHttpStatus = response.StatusCode;
                    response = newRequest.CreateResponse<InspectData>(HttpStatusCode.OK, inspectData);
                }

                response.Headers.Add(RequestHelper.InspectHeaderName, "done");

                return response;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
