/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using Leopotam.EcsLite.Di;

namespace Leopotam.EcsLite
{
	public struct EcsServiceInject<T> : IEcsDataInject where T : class
	{
		public T Value;


		public void Fill(EcsSystems systems)
		{
			Value = systems.GetShared<IServiceContainer>().Get<T>();
		}


		public static implicit operator T(EcsServiceInject<T> value)
		{
			return value.Value;
		}
	}
}