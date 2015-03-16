using Nancy;
using System.Linq;
using System;
using System.Text;
using Nancy.Extensions;
using System.IO;

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
				return Response.AsJson(new { Id = challenge.Id, Challenge = challenge.Challenge, EncryptionId = challenge.EncryptionId });
			};

			Post ["/api/challenge/{responseid}"] = parameters => {
				using (var ms = new MemoryStream()) {
					Request.Body.CopyTo(ms);
					var keyString = Encoding.UTF8.GetString(ms.ToArray());
					var key = new byte[20];
					for (int i = 0, j = 0; i < 20; i++, j += 2) {
						key[i] = Convert.ToByte(keyString.Substring(j, 2), 16);
					}
					_challengeBL.RespondToChallenge(parameters["responseid"], key);
				}
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

			Post ["api/users"] = _ => {
				var username = Request.Query.username;
				var secret = Request.Query.secret;
				if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(secret)) {
					return HttpStatusCode.BadRequest;
				}
				var user = _userBL.CreateUser(username, secret);
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

			Post ["/api/decrypt/{encryptid}"] = parameters => {
				var encryptId = parameters ["encryptid"];
				string requestId = _encryptionBL.Decrypt (encryptId);
				if (null == requestId) {
					return HttpStatusCode.BadRequest;
				}
				return Response.AsJson (new { RequestId = requestId });
			};

			Get ["/api/decrypt/{requestid}"] = parameters => {
				var requestId = parameters ["requestid"];
				return _encryptionBL.GetDecryptionKey (requestId);
			};
		}
	}
}
