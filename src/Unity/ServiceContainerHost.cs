/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using UnityEngine;

namespace Leopotam.EcsLite
{
#if UNITY_5_3_OR_NEWER
	public class ServiceContainerHost : MonoBehaviour
	{
		[Tooltip(
			"By default, this container is destroyed on scene change, meaning you will need another container in the destination scene.  If you would rather have one container that sticks through the lifetime of the game, check this")]
		[SerializeField]
		private bool doNotDestroyOnLoad;


		[Tooltip(
			"By default, the host container only includes child MonoBehaviours derived from 'IHostedService'.  Enabling this causes ALL child MonoBehaviours to be included")]
		[SerializeField]
		private bool hostAll;


		[Tooltip("Enables logging of registered services")] [SerializeField]
		private bool verbose;


		private void Awake()
		{
			if (doNotDestroyOnLoad)
			{
				if (transform.parent != null)
					transform.parent = null;
				DontDestroyOnLoad(gameObject);
			}


			var container = new BasicServiceContainer();

			if (hostAll)
				foreach (var service in GetComponentsInChildren<MonoBehaviour>())
					AddService(container, service);
			else
				foreach (var service in GetComponentsInChildren<IHostedService>())
					AddService(container, service);


			ServiceContainer.SetContainer(container);
		}


		private void OnDestroy()
		{
			ServiceContainer.SetContainer(null);
		}


		private void AddService(IServiceContainer container, object service)
		{
			if (verbose)
				Debug.Log($"Adding service {service.GetType().Name}");
			container.Add(service.GetType(), service);
		}
	}
#endif
}