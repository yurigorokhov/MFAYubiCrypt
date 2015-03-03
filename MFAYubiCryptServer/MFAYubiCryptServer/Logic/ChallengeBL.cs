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

		public void RespondToChallenge(string challengeId, string response) {
			using (var client = _redisManager.GetClient ()) {
				var challenges = client.As<ChallengeEntity> ();
				var challenge = challenges.GetById (challengeId);
				if (challenge == null) {
					return;
				}
				challenges.DeleteById (challengeId);

				//TODO: unlock here
			}
		}
	}
}

