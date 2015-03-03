using System.Collections.Generic;

namespace MFAYubiCryptServer {
	public interface IEncryptionBL {

		//--- Methods ---
		SetupInfo Setup (IEnumerable<UserEntity> users);
		string Decrypt(string encryptId);
	}
}

