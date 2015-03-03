using System.Collections.Generic;
using System;

namespace MFAYubiCryptServer {
	public interface IDataStore {
		UserEntity GetUserById(uint id);
		IEnumerable<UserEntity> GetUsers ();
		IEnumerable<UserEntity> GetUsersByName(string[] names);
		void CreateNewEncryptions(IEnumerable<EncryptionEntity> encryptionEntities);
		IEnumerable<EncryptionEntity> GetByEncryptionId (string encryptId);
	}
}

