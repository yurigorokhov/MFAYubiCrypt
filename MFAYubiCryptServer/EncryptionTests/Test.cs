using NUnit.Framework;
using MFAYubiCryptServer;
using System.Linq;
using System;

namespace EncryptionTests {

	[TestFixture]
	public class Test {

		[Test]
		public void Can_perform_HMACSHA2 () {
			var result = EncryptionBL.HMACSHA1 ("abcdef", "ec39110a7789e281399f3ea0d654412f95108e80");
			Assert.AreEqual ("48eaacc73300ddadae8c3c587140f2f48d6ccfd5", result);
		}
	}
}

