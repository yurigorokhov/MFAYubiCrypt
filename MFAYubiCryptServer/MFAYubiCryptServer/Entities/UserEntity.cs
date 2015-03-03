namespace MFAYubiCryptServer {
	public class UserEntity {

		//--- Constructor ---
		public UserEntity (uint id, string username, string secret) {
			Id = id;
			UserName = username;
			Secret = secret;
		}

		//--- Fields ---
		public readonly uint Id;
		public readonly string UserName;
		public readonly string Secret;
	}
}

