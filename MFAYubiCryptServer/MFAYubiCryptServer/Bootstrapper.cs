using Nancy.Bootstrappers.Autofac;
using Autofac;
using Nancy.Bootstrapper;
using Nancy;
using MySql.Data.MySqlClient;

namespace MFAYubiCryptServer {
	public class Bootstrapper : AutofacNancyBootstrapper {
		protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines) {
			// No registrations should be performed in here, however you may
			// resolve things that are needed during application startup.

		}

		protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer) {

			// Perform registration that should have an application lifetime
			existingContainer.Update (builder => {
				builder.RegisterType<ChallengeBL>().As<IChallengeBL>();
				builder.RegisterType<UserBL>().As<IUserBL>();

				//TODO: read this from config file
				var mysqlConnection = new MySqlConnection("server=yubicrypt_mysql;userid=root;password=password123;database=yubicrypt");
				builder.Register(c => new MysqlDataStore(mysqlConnection)).As<IDataStore>();
			});
		}

		protected override void ConfigureRequestContainer(ILifetimeScope container, NancyContext context) {
			// Perform registrations that should have a request lifetime
		}

		protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context) {
			// No registrations should be performed in here, however you may
			// resolve things that are needed during request startup.
		}
	}
}

