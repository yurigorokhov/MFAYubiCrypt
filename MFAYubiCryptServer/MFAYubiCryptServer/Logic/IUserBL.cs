using System.Collections.Generic;

namespace MFAYubiCryptServer {
	public interface IUserBL {

		//--- Methods ---
		UserEntity GetUserById (uint id);
		IEnumerable<UserEntity> GetUsers ();
		IEnumerable<UserEntity> GetUsersByName(string[] names);
		UserEntity CreateUser (string name, string secret);
	}
}

