using Nancy;

namespace MFAYubiCryptServer {
	public class YubyCryptService : Nancy.NancyModule {

		//--- Fields ---
		readonly IChallengeBL _challengeBL;
		readonly IUserBL _userBL;

		//--- Constructors ---
		public YubyCryptService(IChallengeBL challengeBL, IUserBL userBL) {
			_challengeBL = challengeBL;
			_userBL = userBL;
			initRoutes ();

			StaticConfiguration.DisableErrorTraces = false;
		}

		//--- Methods ---
		public void initRoutes() {
			Get ["/api/status"] = _ => {
				return Response.AsJson (new { Status = "OK" });
			};

			Get ["/api/challenge/{userid:int}"] = parameters => {
				var challenge = _challengeBL.GetChallengeForUserId (parameters["userid"]);
				return Response.AsJson(new { Id = challenge.Id, challenge = challenge.Challenge });
			};

			Post ["/api/response/{responseid:int}"] = parameters => {
				return string.Empty;
			};
		
			Get ["/api/users/{userid:int}"] = parameters => {
				var user = _userBL.GetUserById (uint.Parse(parameters["userid"]));
				if(user == null) {
					return HttpStatusCode.NotFound;
				}
				return Response.AsJson(new { Id = user.Id, UserName = user.UserName });
			};
		}
	}
}
