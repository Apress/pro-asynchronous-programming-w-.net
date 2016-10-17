using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace $rootnamespace$.Areas.RouteDebugger.Components
{
    /// <summary>
    /// A copy of HttpParameterBindingExtensions.cs with one change. HttpParameterBindingExtensions.WillReadUri calls the internal
    /// interface IUriValueProvderFactory, so that code is also in this method.
    /// </summary>
    internal static class HttpParameterBindingExtensions
    {
        public static bool WillReadUri(this HttpParameterBinding parameterBinding)
        {
            if (parameterBinding == null)
            {
                throw new ArgumentNullException("parameterBinding");
            }

            IValueProviderParameterBinding valueProviderParameterBinding = parameterBinding as IValueProviderParameterBinding;
            if (valueProviderParameterBinding != null)
            {
                IEnumerable<ValueProviderFactory> valueProviderFactories = valueProviderParameterBinding.ValueProviderFactories;
                // since The interface IUriValueProvderFactory is internal, following line of codes is altered
                // if (valueProviderFactories.Any() && valueProviderFactories.All(factory => factory is IUriValueProviderFactory))
                if (valueProviderFactories.Any() && valueProviderFactories.All(factory => (factory is QueryStringValueProviderFactory) || (factory is RouteDataValueProviderFactory)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
