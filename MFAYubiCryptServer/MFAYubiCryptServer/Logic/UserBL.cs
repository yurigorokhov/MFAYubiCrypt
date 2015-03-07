using System.Collections.Generic;

namespace MFAYubiCryptServer {
	public class UserBL : IUserBL {

		//--- Fields ---
		readonly IDataStore _dataStore;

		//--- Constructors ---
		public UserBL (IDataStore dataStore) {
			_dataStore = dataStore;
		}

		//--- Methods ---
		public UserEntity GetUserById (uint id) {
			return _dataStore.GetUserById (id);
		}

		public IEnumerable<UserEntity> GetUsers() {
			return _dataStore.GetUsers ();
		}

		public IEnumerable<UserEntity> GetUsersByName(string[] names) {
			return _dataStore.GetUsersByName (names);
		}

		public UserEntity CreateUser(string name, string secret) {
			return _dataStore.CreateUser(new UserEntity(0, name, secret));
		}
	}
}

