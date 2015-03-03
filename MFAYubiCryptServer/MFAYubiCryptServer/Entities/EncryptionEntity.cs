using System;

namespace MFAYubiCryptServer {
	public class EncryptionEntity {

		//--- Constructor ---
		public EncryptionEntity (uint id, Guid encryptionId, uint userId, string encryptedKey) {
			Id = id;
			UserId = userId;
			EncryptedKey = encryptedKey;
			EncryptionId = encryptionId;
		}

		EncryptionEntity (Guid encryptionId, uint userId, string encryptedKey) {
			Id = 0u;
			UserId = userId;
			EncryptedKey = encryptedKey;
			EncryptionId = encryptionId;
		}

		//--- Fields ---
		public readonly uint Id;
		public readonly Guid EncryptionId;
		public readonly uint UserId;
		public readonly string EncryptedKey;

		//--- Class Methods ---
		public static EncryptionEntity New(Guid encryptionId, uint userId, string encryptedKey) {
			return new EncryptionEntity (encryptionId, userId, encryptedKey);
		}
	}
}

