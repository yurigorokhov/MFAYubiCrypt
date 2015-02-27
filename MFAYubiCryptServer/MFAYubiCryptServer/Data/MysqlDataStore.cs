using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;

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

		MySqlCommand NewCommand(string sql) {
			if (_connection.State != System.Data.ConnectionState.Open) {
				_connection.Open ();
			}
			return new MySqlCommand (sql, _connection);
		}

		IEnumerable<UserEntity> UsersFromReader(MySqlDataReader reader) {
			var list = new List<UserEntity> ();
			while (reader.Read ()) {
				list.Add(new UserEntity (reader.GetUInt32 ("user_id"), reader.GetString ("user_name")));
			}
			reader.Close ();
			return list;
		}
	}
}

