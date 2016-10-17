using System.Linq;
using System.Web.Http.Controllers;
using $rootnamespace$.Areas.RouteDebugger.Components;

namespace $rootnamespace$.Areas.RouteDebugger
{
    /// <summary>
    /// This class replaces ApiControllerActionSelector (it's hooked up in RouteDebuggerConfig.cs). 
    /// It uses  _innerSelector to call into ApiControllerActionSelector methods and it calls ActionSelectSimulator methods.
    /// Private members of ApiControllerActionSelector cannot be called with a delegate, so a copy of the private members of
    /// ApiControllerActionSelector are contained in the class ActionSelectSimulator.
    ///
    /// See http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-and-action-selection for more info.   
    /// 
    ///  The SelectAction method examines the request header. If an inspection header is found, it runs the 
    ///  action selection simulator, saves the inspection data in the request property, then uses the delegate
    ///  to run the ApiControllerActionSelector.SelectAction method.
    /// </summary>
    public class InspectActionSelector : IHttpActionSelector
    {
        private IHttpActionSelector _innerSelector;

        public InspectActionSelector(IHttpActionSelector innerSelector)
        {
            _innerSelector = innerSelector;
        }

        public ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
        {
            return _innerSelector.GetActionMapping(controllerDescriptor);
        }

        public HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            var request = controllerContext.Request;
            if (request.IsInspectRequest())
            {
                var simulate = new ActionSelectSimulator();
                request.Properties[RequestHelper.ActionCache] = simulate.Simulate(controllerContext);
            }

            var selectedAction = _innerSelector.SelectAction(controllerContext);

            return selectedAction;
        }
    }
}
