using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MFAYubiCryptServer {
	public class EncryptionBL : IEncryptionBL {

		//--- Fields ---
		readonly IDataStore _session;
		readonly IChallengeBL _challengeBL;

		//--- Constructors ---
		public EncryptionBL (IDataStore session, IChallengeBL challengeBL) {
			_session = session;
			_challengeBL = challengeBL;
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
			foreach(var entity in encryptionEntities) {
				_challengeBL.CreateChallenge (new ChallengeEntity { 
					Id = Guid.NewGuid (),
					Challenge = GenerateChallenge (encryptId, entity.UserId),
					UserId = entity.UserId, 
					Ttl = TimeSpan.FromHours(1)
				});
			}
			return requestId.ToString ();
		}

		string GenerateChallenge(string encryptionId, uint userId) {

			//TODO: add sequence numbers
			return SHA256(string.Format("{0}:{1}", encryptionId, userId));
		}

		string HMACSHA1(string value, string key) {
			var myhmacsha1 = new HMACSHA1 (Encoding.UTF8.GetBytes(key));
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

