using System;
using ServiceStack.Redis;
using System.Linq;

namespace MFAYubiCryptServer {
	public class ChallengeBL : IChallengeBL {

		//--- Fields ---
		readonly IRedisClientsManager _redisManager;

		//--- Constructors ---
		public ChallengeBL(IRedisClientsManager redisManager) {
			_redisManager = redisManager;
		}

		//--- Methods ---
		public ChallengeEntity GetChallengeForUserId(int id) {
			using (var client = _redisManager.GetClient ()) {
				var challenges = client.As<ChallengeEntity> ();
				var allChallenges = challenges.GetAll ();
				return allChallenges.FirstOrDefault (x => x != null && x.UserId == id);
			}
		}

		public void CreateChallenge(ChallengeEntity challenge) {
			using (var client = _redisManager.GetClient ()) {
				var challenges = client.As<ChallengeEntity> ();
				challenges.Store (challenge);
			}
		}

		public string GetChallengeResponseById(string id) {
			using (var client = _redisManager.GetClient ()) {
				return client.Get<string> (string.Format ("response_{0}", id));
			}
		}

		public void RespondToChallenge(string challengeId, byte[] response) {
			var key = response.Aggregate ("", (s, e) => s + String.Format ("{0:x2}", e), s => s);
			using (var client = _redisManager.GetClient ()) {
				var challenges = client.As<ChallengeEntity> ();
				var challenge = challenges.GetById (challengeId);
				if (challenge == null) {
					return;
				}
				challenges.DeleteById (challengeId);
				client.Set (string.Format ("response_{0}", challengeId), string.Format("{0}_{1}", challenge.EncryptionId, key));
			}
		}
	}
}

