using System;
using Nancy.Hosting.Self;

namespace MFAYubiCryptServer {
	class Program {
		public static void Main(string[] args) {
			const int port = 8888;
			var nancyHost = new NancyHost (
				new Uri (string.Format ("http://localhost:{0}/", port))
			);
			nancyHost.Start ();
			Console.WriteLine("Started host on port {0}. Press 'x' to exit", port);
			while (Console.ReadKey().KeyChar != 'x') { }
			Console.WriteLine ("Cleaning up");
			nancyHost.Stop ();
		}
	}
}
