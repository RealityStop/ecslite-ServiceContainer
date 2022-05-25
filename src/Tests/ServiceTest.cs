/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using System.Collections.Generic;
using NUnit.Framework;

namespace Leopotam.EcsLite.Tests
{
	public class BasicServiceContainerTest
	{
		// A Test behaves as an ordinary method
		[Test]
		public void ServiceContainer_Get_CanGetBySameAsRegistered()
		{
			var services = new BasicServiceContainer();
			var service = new TestService();
			services.Add<ITestService>(service);

			var foundService = services.Get<ITestService>();

			Assert.AreEqual(service, foundService);
		}


		[Test]
		public void ServiceContainer_Get_CanGetClassByInterface()
		{
			var services = new BasicServiceContainer();
			var service = new TestService();
			services.Add(service);

			var foundService = services.Get<ITestService>();

			Assert.AreEqual(service, foundService);
		}


		[Test]
		public void ServiceContainer_Get_ReportsErrorForMissingService()
		{
			var services = new BasicServiceContainer();

			Assert.Throws<KeyNotFoundException>(() =>
			{
				var foundService = services.Get<ITestService>();
			});
		}


		[Test]
		public void ServiceContainer_TryGet_SucceedsForDirectLookup()
		{
			var services = new BasicServiceContainer();
			var service = new TestService();
			services.Add<ITestService>(service);

			Assert.True(services.TryGet<ITestService>(out var foundService));
			Assert.AreEqual(service, foundService);
		}


		[Test]
		public void ServiceContainer_TryGet_SucceedsForIndirectLookup()
		{
			var services = new BasicServiceContainer();
			var service = new TestService();
			services.Add(service);

			Assert.True(services.TryGet<ITestService>(out var foundService));
			Assert.AreEqual(service, foundService);
		}


		[Test]
		public void ServiceContainer_TryGet_FailsForMissingService()
		{
			var services = new BasicServiceContainer();

			Assert.False(services.TryGet<ITestService>(out var foundService));
		}


		[Test]
		public void ServiceContainer_RemoveDirect_RemovesAll()
		{
			var services = new BasicServiceContainer();

			var service = new TestService();
			services.Add(service);

			Assert.That(services.TryGet<ITestService>(out _));
			services.Remove<TestService>();

			Assert.False(services.TryGet<ITestService>(out _));
		}


		[Test]
		public void ServiceContainer_RemoveIndirect_RemovesAll()
		{
			var services = new BasicServiceContainer();

			var service = new TestService();
			services.Add(service);

			Assert.That(services.TryGet<ITestService>(out _));
			services.Remove<ITestService>();

			Assert.False(services.TryGet<ITestService>(out _));
		}


		private interface ITestService
		{
		}

		private class TestService : ITestService
		{
		}
	}
}