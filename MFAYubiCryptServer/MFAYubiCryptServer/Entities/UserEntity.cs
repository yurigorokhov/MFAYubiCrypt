namespace MFAYubiCryptServer {
	public class UserEntity {

		//--- Constructor ---
		public UserEntity (uint id, string username) {
			Id = id;
			UserName = username;
		}

		//--- Fields ---
		public readonly uint Id;
		public readonly string UserName;
	}
}

