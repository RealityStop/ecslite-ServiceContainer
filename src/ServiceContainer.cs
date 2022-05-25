/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */


using System;
using UnityEngine;

namespace Leopotam.EcsLite
{
	/// <summary>
	///     A Container that stores references to all of the configured systems.  It is not
	///     responsible for determining what those services are, but provides a convenient way list services.
	/// </summary>
	public static class ServiceContainer
	{
		private static IServiceContainer _instance;


		/// <summary>
		///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how
		///     the type should be requested  (such as by a particular interface)
		/// </summary>
		/// <param name="serviceInstance">the service to make available to requesters</param>
		/// <typeparam name="T">The type used to request this service</typeparam>
		/// <returns>the container, for chaining.</returns>
		public static IServiceContainer Add<T>(T serviceInstance)
		{
			if (_instance == null)
				throw new NullReferenceException("Service Container has not been assigned!");

			return _instance.Add(serviceInstance);
		}


		/// <summary>
		///     Requests a service from the container.  Throws exception if it cannot be found.  Use TryGet to avoid this.
		/// </summary>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>the service matching the requested type.</returns>
		public static T Get<T>()
		{
			if (_instance == null)
				throw new NullReferenceException("Service Container has not been assigned!");
			return _instance.Get<T>();
		}


		/// <summary>
		///     Requests a service from the container, and notifies the caller if the service is not available.
		/// </summary>
		/// <param name="result">the service, if it exists</param>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>true if the service was found, false otherwise.</returns>
		public static bool TryGet<T>(out T result)
		{
			if (_instance == null)
				throw new NullReferenceException("Service Container has not been assigned!");

			return _instance.TryGet(out result);
		}


		/// <summary>
		///     Removes the specified service from the container.
		/// </summary>
		/// <typeparam name="T">the type to remove</typeparam>
		public static void Remove<T>()
		{
			if (_instance == null)
				throw new NullReferenceException("Service Container has not been assigned!");

			_instance.Remove<T>();
		}


		/// <summary>
		///     Fetches the current global container, if there is one.
		/// </summary>
		/// <returns>Current global container, or null.</returns>
		public static IServiceContainer GetCurrentContainer()
		{
			return _instance;
		}


		/// <summary>
		///     Assigns the current container as the globally accessible container.
		/// </summary>
		/// <param name="container">
		///     The container to assign.  Pass null to clear the
		///     existing container.
		/// </param>
		public static void SetContainer(IServiceContainer container)
		{
			if (_instance != null && container != null)
			{
#if UNITY_5_3_OR_NEWER
				if (Application.isEditor)
					Debug.Log("Assigning Service Container over top of existing instance");
#endif
			}

			_instance = container;
		}
	}
}