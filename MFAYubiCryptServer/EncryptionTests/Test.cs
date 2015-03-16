using NUnit.Framework;
using MFAYubiCryptServer;
using System.Linq;
using System;

namespace EncryptionTests {

	[TestFixture]
	public class Test {

		[Test]
		public void Can_perform_HMACSHA2 () {
			var result = EncryptionBL.HMACSHA1 ("104b02b6d45160079edc0ba97331954752f0b3dd3f53531b16506f3fa82df61e", "e71c27f8e84970238edce4ca62176c52d1a6d531");
			Assert.AreEqual ("a2b101263b811ca5c3cd0f807f9168b08645e7d1", result);
		}
	}
}

