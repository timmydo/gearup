namespace Shared.Interfaces
{
	using Microsoft.AspNet.Mvc;

	public interface IUserAuthenticator
    {
		IUserLoginInfo AuthenticateUser(Controller c);

		string GenerateCookie(IUserLoginInfo userLoginInfo);
		bool VerifyLoginInfo(IUserLoginInfo userLoginInfo);

	}
}
