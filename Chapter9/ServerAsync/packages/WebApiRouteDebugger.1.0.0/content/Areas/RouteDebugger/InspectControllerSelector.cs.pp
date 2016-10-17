using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using $rootnamespace$.Areas.RouteDebugger.Models;

namespace $rootnamespace$.Areas.RouteDebugger
{
    /// <summary>
    /// This class replaces the DefaultHttpControllerSelector (see RouteDebuggerConfig.cs).  
    /// It uses _innerSelector to call into DefaultHttpControllerSelector methods.
    /// See http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-and-action-selection for more info.
    /// 
    /// The SelectController method examines the request header. If an inspection header is found:
    ///     1. Saves all candidate controllers in inspection data.
    ///     2. Marks the selected controller.
    /// </summary>
    public class InspectControllerSelector : IHttpControllerSelector
    {
        private IHttpControllerSelector _innerSelector;

        public InspectControllerSelector(IHttpControllerSelector innerSelector)
        {
            _innerSelector = innerSelector;
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _innerSelector.GetControllerMapping();
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request.IsInspectRequest())
            {
                var controllers = _innerSelector.GetControllerMapping().Values.Select(desc =>
                    new ControllerSelectionInfo
                    {
                        ControllerName = desc.ControllerName,
                        ControllerType = desc.ControllerType.AssemblyQualifiedName
                    }).ToArray();

                request.Properties[RequestHelper.ControllerCache] = controllers;
            }

            // DefaultHttpControllerSelector.SelectController
            var controllerDescriptor = _innerSelector.SelectController(request);

            // if exception is not thrown
            request.Properties[RequestHelper.SelectedController] = controllerDescriptor.ControllerName;

            return controllerDescriptor;
        }
    }
}
