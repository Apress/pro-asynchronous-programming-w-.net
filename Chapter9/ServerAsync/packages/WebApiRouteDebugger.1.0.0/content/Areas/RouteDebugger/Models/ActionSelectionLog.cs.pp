using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace $rootnamespace$.Areas.RouteDebugger.Models
{
    /// <summary>
    /// Logs information collected when ActionSelectSimulator.Simulate is called. 
    /// ActionSelectSimulator.Simulate replaces DefaultActionSelector.
    /// </summary>
    public class ActionSelectionLog
    {
        private Dictionary<HttpActionDescriptor, ActionSelectionInfo> _actionDescriptors;

        public ActionSelectionLog(IEnumerable<HttpActionDescriptor> actions)
        {
            _actionDescriptors = new Dictionary<HttpActionDescriptor, ActionSelectionInfo>();

            foreach (var each in actions)
            {
                _actionDescriptors[each] = new ActionSelectionInfo(each);
            }
        }

        public string ActionName { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public ActionSelectionInfo[] ActionSelections
        {
            get
            {
                return _actionDescriptors.Values.ToArray();
            }
        }

        /// <summary>
        /// Looks for related ActionSelectionInfo instances based on given action
        /// descriptors. Invoke given marking functor on each of them.
        /// 
        /// This method is used in action simulator when a selection decision is 
        /// made in every stage, the selected action will be passed in along with
        /// a marking functor. The marking functor usually set a particular boolean
        /// property on related ActionSelectionInfo instance to mark that this action
        /// is selected in particular stage.
        /// </summary>
        /// <param name="actions">The actions to be marked.</param>
        /// <param name="marking">The functor picking the bool property of an action to be set to true.</param>
        internal void MarkSelected(IEnumerable<HttpActionDescriptor> actions, Action<ActionSelectionInfo> marking)
        {
            foreach (var action in actions)
            {
                ActionSelectionInfo found;
                if (_actionDescriptors.TryGetValue(action, out found))
                {
                    marking(found);
                }
            }
        }

        /// <summary>
        /// Counterpart of function MarkSelected, instead of marking selected action
        /// this method mark unselected action.
        /// </summary>
        /// <param name="actions">the actions NOT to be marked</param>
        /// <param name="marking">the functor picking the bool property of an action to be set to true.</param>
        internal void MarkOthersSelected(IEnumerable<HttpActionDescriptor> actions, Action<ActionSelectionInfo> marking)
        {
            HashSet<HttpActionDescriptor> remaining = new HashSet<HttpActionDescriptor>(_actionDescriptors.Keys);

            foreach (var each in actions)
            {
                remaining.Remove(each);
            }

            foreach (var each in remaining)
            {
                ActionSelectionInfo found;
                if (_actionDescriptors.TryGetValue(each, out found))
                {
                    marking(found);
                }
            }
        }
    }
}
