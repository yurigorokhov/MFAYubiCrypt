using Nancy;
using System.Linq;
using System;
using System.Text;
using Nancy.Extensions;

namespace MFAYubiCryptServer {
	public class YubyCryptService : Nancy.NancyModule {

		//--- Fields ---
		readonly IChallengeBL _challengeBL;
		readonly IUserBL _userBL;
		readonly IEncryptionBL _encryptionBL;

		//--- Constructors ---
		public YubyCryptService(IChallengeBL challengeBL, IUserBL userBL, IEncryptionBL encryptionBL) {
			_challengeBL = challengeBL;
			_userBL = userBL;
			_encryptionBL = encryptionBL;
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
				if(challenge == null) {
					return string.Empty;
				}
				return Response.AsJson(new { Id = challenge.Id, challenge = challenge.Challenge });
			};

			Post ["/api/challenge/{responseid}"] = parameters => {
				_challengeBL.RespondToChallenge(parameters["responseid"], Request.Body.AsString());
				return string.Empty;
			};

			Get ["/api/users"] = parameters => {
				var users = _userBL.GetUsers();
				return Response.AsJson(users.Select(x => new { Id = x.Id, UserName = x.UserName }));
			};

			Get ["/api/users/{userid:int}"] = parameters => {
				var user = _userBL.GetUserById (uint.Parse(parameters["userid"]));
				if(user == null) {
					return HttpStatusCode.NotFound;
				}
				return Response.AsJson(new { Id = user.Id, UserName = user.UserName });
			};

			// set up an encryption
			Post ["/api/setup"] = parameters => {
				var userParam = Request.Query.users ?? string.Empty;
				string[] userNames = userParam.Value.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if(userNames.Length == 0) {
					return HttpStatusCode.BadRequest;
				}
				var users = _userBL.GetUsersByName(userNames);
				if(users.Count() != userNames.Count()) {
					return HttpStatusCode.NotFound;
				}
				var info = _encryptionBL.Setup(users);
				return Response.AsJson(new { EncryptionId = info.EncryptionId, ShaKeys = info.ShaKeys });
			};

			Get ["/api/decrypt/{encryptid}"] = parameters => {
				var encryptId = parameters ["encryptid"];
				string requestId = _encryptionBL.Decrypt (encryptId);
				if (null == requestId) {
					return HttpStatusCode.BadRequest;
				}
				return Response.AsJson (new { RequestId = requestId });
			};
		}
	}
}
