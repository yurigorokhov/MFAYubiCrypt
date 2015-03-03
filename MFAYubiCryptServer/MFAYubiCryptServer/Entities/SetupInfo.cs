using System;

namespace MFAYubiCryptServer {
	public class SetupInfo {

		//--- Fields ---
		public readonly string EncryptionId;
		public readonly string ShaKeys;

		//--- Constructors ---
		public SetupInfo (string encryptionId, string shaKeys) {
			EncryptionId = encryptionId;
			ShaKeys = shaKeys;
		}
	}
}

