namespace MFAYubiCryptServer {
	public class ChallengeBL : IChallengeBL {

		//--- Methods ---
		public ChallengeEntity GetChallengeForUserId(int id) {
			return new ChallengeEntity (123, "somechallenge");
		}
	}
}

