namespace MFAYubiCryptServer {
	public interface IChallengeBL {

		//--- Methods ---
		ChallengeEntity GetChallengeForUserId(int id);
		void CreateChallenge(ChallengeEntity challenge);
		void RespondToChallenge (string challengeId, byte[] response);
		string GetChallengeResponseById (string id);
	}
}

