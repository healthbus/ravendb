using System;
using System.Linq;
using Raven.Abstractions.Extensions;
using Raven.Server;
using Xunit;

namespace Raven.Tests.Shard.BlogModel
{
	public class CanQueryOnlyUsers : ShardingScenario
	{
		[Fact]
		public void WhenQueryingForUserById()
		{
			using (var session = ShardedDocumentStore.OpenSession())
			{
				var user = session.Load<User>("users/1");
				Assert.Null(user);
			}

			AssertNumberOfRequests(Servers["Users"], 1);
			Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
				.ForEach(server => AssertNumberOfRequests(server.Value, 0));
		}

		private void AssertNumberOfRequests(RavenDbServer server, int numberOfRequests)
		{
			try
			{
				Assert.Equal(numberOfRequests, server.Server.NumberOfRequests);
			}
			catch
			{
				Console.WriteLine(string.Join(Environment.NewLine, server.Server.LastRequests));
				throw;
			}
		}

		[Fact]
		public void WhenQueryingForUsersById()
		{
			using (var session = ShardedDocumentStore.OpenSession())
			{
				var users = session.Load<User>("users/1", "users/2");
				Assert.Equal(2, users.Length);
				Assert.Null(users[0]);
				Assert.Null(users[1]);

				AssertNumberOfRequests(Servers["Users"], 1);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => AssertNumberOfRequests(server.Value, 0));
			}
		}

		[Fact]
		public void WhenStoringUser()
		{
			using (var session = ShardedDocumentStore.OpenSession())
			{
				session.Store(new User { Name = "Fitzchak Yitzchaki" });
				AssertNumberOfRequests(Servers["Users"], 2); // HiLo

				session.SaveChanges();
				AssertNumberOfRequests(Servers["Users"], 3);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => AssertNumberOfRequests(server.Value, 0));
			}

			using (var session = ShardedDocumentStore.OpenSession())
			{
				var user = session.Load<User>("users/1");
				Assert.NotNull(user);
				Assert.Equal("Fitzchak Yitzchaki", user.Name);

				AssertNumberOfRequests(Servers["Users"], 4);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => AssertNumberOfRequests(server.Value, 0));
			}
		}

		[Fact]
		public void WhenQueryingForUserByName()
		{
			using (var session = ShardedDocumentStore.OpenSession())
			{
				var user = session.Query<User>()
					.FirstOrDefault(x => x.Name == "Fitzchak");
				Assert.Null(user);

				AssertNumberOfRequests(Servers["Users"], 1);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => AssertNumberOfRequests(server.Value, 0));
			}
		}
	}
}