using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace $rootnamespace$.Areas.RouteDebugger.Models
{
    /// <summary>
    /// Representing one action selection
    /// </summary>
    public class ActionSelectionInfo
    {
        public ActionSelectionInfo(HttpActionDescriptor descriptor)
        {
            ActionName = descriptor.ActionName;
            SupportedHttpMethods = descriptor.SupportedHttpMethods.ToArray();
            Parameters = descriptor.GetParameters().Select(p => new HttpParameterDescriptorInfo(p)).ToArray();
        }

        public string ActionName { get; set; }

        public HttpMethod[] SupportedHttpMethods { get; set; }

        public HttpParameterDescriptorInfo[] Parameters { get; set; }

        /// <summary>
        /// Is this action selected based on its action name?
        /// </summary>
        public bool? FoundByActionName { get; set; }

        /// <summary>
        /// Is this action selected based on its action name and its supported http verb?
        /// </summary>
        public bool? FoundByActionNameWithRightVerb { get; set; }

        /// <summary>
        /// Is this action selected based on its supported http verb?
        /// </summary>
        public bool? FoundByVerb { get; set; }

        /// <summary>
        /// Do this action's parameters match the ones in query string?
        /// </summary>
        public bool? FoundWithRightParam { get; set; }

        /// <summary>
        /// Is this action finally selected by selection attribute?
        /// </summary>
        public bool? FoundWithSelectorsRun { get; set; }
    }
}
