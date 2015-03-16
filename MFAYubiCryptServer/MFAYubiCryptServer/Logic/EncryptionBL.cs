using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using ServiceStack.Redis;

namespace MFAYubiCryptServer {
	public class EncryptionBL : IEncryptionBL {

		//--- Fields ---
		readonly IDataStore _session;
		readonly IChallengeBL _challengeBL;
		readonly IRedisClientsManager _redisClients;

		//--- Constructors ---
		public EncryptionBL (IDataStore session, IChallengeBL challengeBL, IRedisClientsManager redisClients) {
			_session = session;
			_challengeBL = challengeBL;
			_redisClients = redisClients;
		}

		//--- Methods ---
		public SetupInfo Setup(IEnumerable<UserEntity> users) {
			var guid = Guid.NewGuid ();
			var keys = new List<string> ();
			var encryptionEntities = users.Select (x => {
				var newKey = Guid.NewGuid().ToString();
				keys.Add(newKey);
				var encryptionKey = HMACSHA1(GenerateChallenge(guid.ToString(), x.Id), x.Secret);
				var encryptedData = Cryptography.Cryptography.Encrypt(string.Format("{0}:{1}", newKey, x.Secret), encryptionKey);
				return EncryptionEntity.New (guid, x.Id, encryptedData);
			}).ToArray ();
			_session.CreateNewEncryptions (encryptionEntities);
			return new SetupInfo(guid.ToString(), SHA256(string.Join(":", keys)));
		}

		public string Decrypt(string encryptId) {
			var encryptionEntities = _session.GetByEncryptionId (encryptId);
			if (!encryptionEntities.Any ()) {
				return null;
			}
			var requestId = Guid.NewGuid ();
			var challengeIds = new List<string> ();
			foreach(var entity in encryptionEntities) {
				var id = Guid.NewGuid ();
				challengeIds.Add (id.ToString ());
				_challengeBL.CreateChallenge (new ChallengeEntity { 
					Id = id,
					Challenge = GenerateChallenge (encryptId, entity.UserId),
					UserId = entity.UserId, 
					Ttl = TimeSpan.FromHours(1),
					EncryptionId = entity.Id
				});
			}

			// store requestId in memcache with the keys that must be decoded
			using (var client = _redisClients.GetClient ()) {
				client.Set(string.Format("request_{0}", requestId), string.Join(",", challengeIds));
			}
			return requestId.ToString ();
		}

		public string GetDecryptionKey(string requestId) {
			var responsesById = new Dictionary<string, string> ();
			using (var client = _redisClients.GetClient ()) {

				// get the request Id
				var res = client.Get<string> (string.Format ("request_{0}", requestId));
				if (res == null) {
					return null;
				}
				var challengeIds = res.Split (new [] { ',' });
				foreach (var id in challengeIds) {
					var response = _challengeBL.GetChallengeResponseById (id);
					if (response == null) {
						return null;
					}
					responsesById [id] = response;
				}
			}

			// using the responses decrypt the rows
			var keys = new List<string> ();
			foreach (var id in responsesById.Keys) {
				var encryptionId = responsesById [id].Split (new [] { '_' })[0];
				var response = responsesById [id].Split (new [] { '_' })[1];
				var encrypt = _session.GetById (encryptionId);
				keys.Add(Cryptography.Cryptography.Decrypt (encrypt.EncryptedKey, response).Split(new [] {':'})[0]);
			}
			return SHA256 (string.Join (":", keys));
		}

		string GenerateChallenge(string encryptionId, uint userId) {
			return SHA256(string.Format("{0}:{1}", encryptionId, userId));
		}

		public static string HMACSHA1(string value, string keyString) {
			if (keyString.Length < 40) {
				throw new ArgumentException ("Secret too short");
			}
			var key = new byte[20];
			for (int i = 0, j = 0; i < 20; i++, j += 2) {
				key[i] = Convert.ToByte(keyString.Substring(j, 2), 16);
			}
			var myhmacsha1 = new HMACSHA1 (key);
			var byteArray = Encoding.ASCII.GetBytes (value);
			var stream = new MemoryStream (byteArray);
			return myhmacsha1.ComputeHash (stream).Aggregate("", (s, e) => s + String.Format("{0:x2}",e), s => s );
		}

		string SHA256(string value) {
			var sha = new SHA256Cng ();
			var byteArray = Encoding.ASCII.GetBytes (value);
			var stream = new MemoryStream (byteArray);
			return sha.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}",e), s => s );
		}
	}
}

