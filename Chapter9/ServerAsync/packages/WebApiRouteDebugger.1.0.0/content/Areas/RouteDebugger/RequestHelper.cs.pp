using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using $rootnamespace$.Areas.RouteDebugger.Components;

namespace $rootnamespace$.Areas.RouteDebugger
{
    public static class RequestHelper
    {
        public static readonly string InspectHeaderName = "RouteInspecting";
        public static readonly string RouteDataCache = "RD_ROUTEDATA";
        public static readonly string RoutesCache = "RD_ROUTES";
        public static readonly string ControllerCache = "RD_CONTROLLER";
        public static readonly string ActionCache = "RD_ACTION";
        public static readonly string SelectedController = "RD_SELECTED_CONTROLLER";

        /// <summary>
        /// Returns true if this request is a inspect request. 
        /// 
        /// For sake of security only inspect request from local will be accepted.
        /// </summary>
        public static bool IsInspectRequest(this HttpRequestMessage request)
        {
            IEnumerable<string> values;

            if (request.Headers.TryGetValues(InspectHeaderName, out values))
            {
                if (String.Equals(values.FirstOrDefault(), "true", StringComparison.InvariantCulture))
                {
                    return request.IsFromLocal();
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if this request is from local
        /// </summary>
        public static bool IsFromLocal(this HttpRequestMessage request)
        {
            if (request == null)
            {
                return false;
            }

            Lazy<bool> isLocal;
            if (request.Properties.TryGetValue<Lazy<bool>>(HttpPropertyKeys.IsLocalKey, out isLocal))
            {
                return isLocal.Value;
            }

            return false;
        }
    }
}
