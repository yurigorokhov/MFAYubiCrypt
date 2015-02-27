namespace MFAYubiCryptServer {
	public class ChallengeEntity {

		//--- Constructors ---
		public ChallengeEntity (uint id, string challenge) {
			Id = id;
			Challenge = challenge;
		}

		//-- Fields ---
		public readonly uint Id;
		public readonly string Challenge;
	}
}

