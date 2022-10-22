using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Web;

namespace SlaytonNichols.Common.ServiceStack.Auth;
public class AppUserAuthEvents : AuthEvents
{
    public override void OnAuthenticated(IRequest req, IAuthSession session, IServiceBase authService,
        IAuthTokens tokens, Dictionary<string, string> authInfo)
    {
        var authRepo = HostContext.AppHost.GetAuthRepository(req);
        using (authRepo as IDisposable)
        {
            var userAuth = authRepo.GetUserAuth(session.UserAuthId);
            authRepo.SaveUserAuth(userAuth);
        }
    }
}