/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using System;

namespace Leopotam.EcsLite
{
	/// <summary>
	///     A container that stores references to various services and offers them for request by type.
	/// </summary>
	public interface IServiceContainer
	{
		/// <summary>
		///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how
		///     the type should be requested  (such as by a particular interface)
		/// </summary>
		/// <param name="serviceInstance">the service to make available to requesters</param>
		/// <typeparam name="T">The type used to request this service</typeparam>
		/// <returns>the container, for chaining.</returns>
		IServiceContainer Add<T>(T serviceInstance);


		/// <summary>
		///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how
		///     the type should be requested.  This overload provides for compile-time indeterminate additions, where the
		///     type is only known at runtime.
		/// </summary>
		/// <param name="serviceInstance">the service to make available to requesters</param>
		/// <typeparam name="T">The type used to request this service</typeparam>
		/// <returns>the container, for chaining.</returns>
		IServiceContainer Add(Type registrationType, object serviceInstance);


		/// <summary>
		///     Requests a service from the container.  Throws exception if it cannot be found.  Use TryGet to avoid this.
		/// </summary>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>the service matching the requested type.</returns>
		T Get<T>();


		/// <summary>
		///     Requests a service from the container, and notifies the caller if the service is not available.
		/// </summary>
		/// <param name="result">the service, if it exists</param>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>true if the service was found, false otherwise.</returns>
		bool TryGet<T>(out T result);


		/// <summary>
		///     Removes the specified service from the container.
		/// </summary>
		/// <typeparam name="T">the type to remove</typeparam>
		void Remove<T>();
	}

	public static class IServiceContainerExt
	{
		/// <summary>
		///     Assigns this container to be the global service container.
		/// </summary>
		/// <param name="self">The container to promote</param>
		/// <returns>the service container</returns>
		public static IServiceContainer Finalize(this IServiceContainer self)
		{
			ServiceContainer.SetContainer(self);
			return self;
		}
	}
}