using System.Threading.Tasks;

namespace AuthenticationPlugins
{
    public class IAuthenticationResult
    {
       string Identity { get;  }
       bool Authenticated { get; }
    }

    public interface IAuthenticationProvider
    {
        IAuthenticationResult Authenticate();
    }
}