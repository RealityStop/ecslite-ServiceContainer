/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using System;
using System.Collections.Generic;
using System.Data;

namespace Leopotam.EcsLite
{
	/// <summary>
	///     Provides a basic implementation of the service container.
	/// </summary>
	public class BasicServiceContainer : IServiceContainer
	{
		private readonly Dictionary<Type, object> _services = new();


		/// <summary>
		///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how
		///     the type should be requested  (such as by a particular interface)
		/// </summary>
		/// <param name="serviceInstance">the service to make available to requesters</param>
		/// <typeparam name="T">The type used to request this service</typeparam>
		/// <returns>the container, for chaining.</returns>
		public IServiceContainer Add<T>(T serviceInstance)
		{
			return Add(typeof(T), serviceInstance);
		}


		/// <summary>
		///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how
		///     the type should be requested.  This overload provides for compile-time indeterminate additions, where the
		///     type is only known at runtime.
		/// </summary>
		/// <param name="serviceInstance">the service to make available to requesters</param>
		/// <typeparam name="T">The type used to request this service</typeparam>
		/// <returns>the container, for chaining.</returns>
		public IServiceContainer Add(Type registrationType, object serviceInstance)
		{
			if (_services.ContainsKey(registrationType))
				throw new DuplicateNameException($"Service for {registrationType.Name} already defined");

			_services.Add(registrationType, serviceInstance);
			return this;
		}


		/// <summary>
		///     Requests a service from the container.  Throws exception if it cannot be found.  Use TryGet to avoid this.
		/// </summary>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>the service matching the requested type.</returns>
		public T Get<T>()
		{
			if (!_services.ContainsKey(typeof(T)))
			{
				if (!ScanForDerivedType<T>(out var result))
					throw new KeyNotFoundException($"No service registered for {typeof(T).Name}");

				return result;
			}

			return (T)_services[typeof(T)];
		}


		/// <summary>
		///     Requests a service from the container, and notifies the caller if the service is not available.
		/// </summary>
		/// <param name="result">the service, if it exists</param>
		/// <typeparam name="T">Type to request</typeparam>
		/// <returns>true if the service was found, false otherwise.</returns>
		public bool TryGet<T>(out T result)
		{
			result = default;
			if (!_services.ContainsKey(typeof(T)))
			{
				if (ScanForDerivedType(out result))
					return true;

				return false;
			}

			result = (T)_services[typeof(T)];
			return true;
		}


		/// <summary>
		///     Removes the specified service from the container.
		/// </summary>
		/// <typeparam name="T">the type to remove</typeparam>
		public void Remove<T>()
		{
			var toRemove = new List<Type>();
			foreach (var service in _services)
				if (service.Value is T casted)
					toRemove.Add(service.Key);

			foreach (var removeType in toRemove) _services.Remove(removeType);
		}


		/// <summary>
		///     Hunts through the existing types looking for the requested type.  If it finds a
		///     unique reference, then it updates the container to cache the result and
		///     returns it.
		/// </summary>
		/// <param name="result"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="DuplicateNameException"></exception>
		private bool ScanForDerivedType<T>(out T result)
		{
			var found = false;
			result = default;

			foreach (var service in _services)
				if (service.Value is T casted)
				{
					if (found)
						throw new DuplicateNameException($"Found multiple services for type {typeof(T).Name}.");

					found = true;
					result = casted;
				}

			//now update so we don't have to search for it again
			if (found)
				Add(result);

			return found;
		}
	}
}