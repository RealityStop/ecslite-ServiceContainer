/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

namespace Leopotam.EcsLite
{
#if UNITY_5_3_OR_NEWER
	/// <summary>
	///     Simply a flag interface that marks a MonoBehaviour as a service to be auto-injected into the ServicesContainer.
	///     Unnecessary if manually constructing the ServicesContainer
	/// </summary>
	public interface IHostedService
	{
	}
#endif
}