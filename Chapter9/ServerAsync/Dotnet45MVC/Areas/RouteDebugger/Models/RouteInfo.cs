using System.Collections.Generic;

namespace Dotnet45MVC.Areas.RouteDebugger.Models
{
    public class RouteInfo
    {
        public string RouteTemplate { get; set; }

        public KeyValuePair<string, string>[] Defaults { get; set; }

        public KeyValuePair<string, string>[] Constraints { get; set; }

        public KeyValuePair<string, string>[] DataTokens { get; set; }

        public string Handler { get; set; }

        public bool Picked { get; set; }
    }
}
