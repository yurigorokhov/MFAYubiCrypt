using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MFAYubiCryptServer {
	public class MysqlDataStore : IDataStore {

		//--- Fields ---
		readonly MySqlConnection _connection;

		//--- Constructors ---
		public MysqlDataStore(MySqlConnection conn) {
			_connection = conn;
		}

		//--- Methods ---
		public UserEntity GetUserById(uint id) {
			var cmd = NewCommand ("SELECT * FROM users WHERE user_id = @ID");
			cmd.Parameters.AddWithValue ("ID", id);
			var users = UsersFromReader (cmd.ExecuteReader ());
			return users.FirstOrDefault ();
		}

		public IEnumerable<UserEntity> GetUsers() {
			var cmd = NewCommand ("SELECT * FROM users");
			return UsersFromReader (cmd.ExecuteReader ());
		}

		public IEnumerable<UserEntity> GetUsersByName(string[] names) {
			var query = string.Format ("SELECT * FROM users WHERE user_name IN ({0})", string.Join (",", names.Select (x => string.Format ("'{0}'", x))));
			var cmd = NewCommand (query);
			return UsersFromReader (cmd.ExecuteReader ());
		}

		public void CreateNewEncryptions(IEnumerable<EncryptionEntity> encryptionEntities) {
			foreach (var entity in encryptionEntities) {
				var cmd = NewCommand ("INSERT INTO encrypt_keys (keys_encryption_id, keys_user_id, keys_key_encrypted) VALUES (@ENCRYPTIONID, @USERID, @ENCRYPTEDKEY)");
				cmd.Parameters.AddWithValue ("ENCRYPTIONID", entity.EncryptionId);
				cmd.Parameters.AddWithValue ("USERID", entity.UserId);
				cmd.Parameters.AddWithValue ("ENCRYPTEDKEY", entity.EncryptedKey);
				cmd.ExecuteNonQuery ();
			}
		}

		public IEnumerable<EncryptionEntity> GetByEncryptionId(string encryptId) {
			var cmd = NewCommand ("SELECT * FROM encrypt_keys WHERE keys_encryption_id = @ID");
			cmd.Parameters.AddWithValue ("ID", encryptId);
			return EncryptionEntitiesFromReader (cmd.ExecuteReader ());
		}

		MySqlCommand NewCommand(string sql) {
			if (_connection.State != System.Data.ConnectionState.Open) {
				_connection.Open ();
			}
			return new MySqlCommand (sql, _connection);
		}

		IEnumerable<UserEntity> UsersFromReader(MySqlDataReader reader) {
			var list = new List<UserEntity> ();
			while (reader.Read ()) {
				list.Add(new UserEntity (
					reader.GetUInt32 ("user_id"), 
					reader.GetString ("user_name"), 
					reader.GetString("user_secret")
				));
			}
			reader.Close ();
			return list;
		}

		IEnumerable<EncryptionEntity> EncryptionEntitiesFromReader(MySqlDataReader reader) {
			var list = new List<EncryptionEntity> ();
			while (reader.Read ()) {
				list.Add(new EncryptionEntity (
					reader.GetUInt32 ("keys_id"), 
					new Guid(reader.GetString ("keys_encryption_id")), 
					reader.GetUInt32("keys_user_id"),
					reader.GetString("keys_key_encrypted")
				));
			}
			reader.Close ();
			return list;
		}
	}
}

