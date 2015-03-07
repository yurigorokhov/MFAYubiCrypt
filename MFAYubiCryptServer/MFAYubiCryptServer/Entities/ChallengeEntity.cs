using System;

namespace MFAYubiCryptServer {
	public class ChallengeEntity {

		//-- Properties ---
		public Guid Id { get; set; }
		public string Challenge { get; set; }
		public uint UserId { get; set; }
		public TimeSpan Ttl { get; set; }
		public uint EncryptionId { get; set; }
	}
}

