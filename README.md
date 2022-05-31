# EcsLite ServiceContainer

💡 ServiceContainer is a unopinionated, simple, easy to use container for [LeoECSCommunity/ecslite](https://github.com/LeoECSCommunity/ecslite) designed to make it trivial to customize external services and providers and make them available to systems through a single shared class.

🏁 The goal of the ServiceContainer is to provide a single high-level point where you configure all the Game Engine service, data, and external information that your services are going to require and "inject" it into your ECS worlds.  This helps you write more abstracted code with fewer dependencies, while simplifying the tasks associated with integrating you ECS simulation with the rest of your Game Engine and make everything nice and testable.

🧩 While the repository comes with custom Unity integration, this is properly guarded so as to 
function in other environments.

> **Note:** This extension is built on and requires [ecslite-di](https://github.com/LeoECSCommunity/ecslite-di) in your project.  

# Getting Started

## General
Simply download the source repository and include the .cs files as you normally would.

## Unity 

ServiceContainer can either be imported via the Unity Package Manager or used directly from source.

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:
> https://github.com/RealityStop/ecslite-servicecontainer.git

(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

###  Manual Unity install:
Alternatively, open Packages/manifest.json and add this line under dependencies:

	"dev.leoecscommunity.ecslite.servicecontainer": "https://github.com/RealityStop/ecslite-servicecontainer.git"
	
(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)
	
## Namespace
All ServiceContainer code is under the 
```
Leopotam.EcsLite
```
namespace to simplify integration with other ecslite extensions.

# A simple example
ServiceContainer is designed to be trivial to use, as shown in the following example.

🎓Unity users have an additional way of composing services.  See  [(Optional) Unity](https://github.com/RealityStop/ecslite-ServiceContainer#optional-unity-gameobject-based-compositor).

Let's say we have a simple prototype game that we want to abstract away Unity Engine specifics from our systems to make our ECS simulation both engine-agnostic and immune to Unity changes.

## EcsStartup

```C#
// We start by constructing a container with the services we'll need in the
// systems.  Services can be added at any time to the service container, but 
// setting them up ahead of time allows us to use ecslite-di to automatically 
// inject services into the systems.
var serviceContainer = new BasicServiceContainer()  
 .Add<ILogService>(new UnityConsoleLogService())  
 .Add<IPrefabService>(new UnityPrefabLocatorService())  
 .Add<IViewService>(new UnityViewService())  
 .Add<ITimeService>(GetComponentInChildren<ITimeService>())  
 .Add<IDisplayService>(GetComponentInChildren<IDisplayService>())  
 .Add<IPerformanceService>(GetComponentInChildren<IPerformanceService>())  
 .Add<IEventBus>(_eventBus)  

// And the final call to Finalize sets this container as the globally accessible
// ServiceContainer, which allows you to use the Service Container in regular 
// MonoBehaviours as well.
 .Finalize();


//We created the container above, but if we had created it elsewhere
//(such as using the optional Unity Host) we can get the currently active
//container via the static ServiceContainer class.
serviceContainer = ServiceContainer.GetCurrentContainer();


//And then we pass the service container to the EcsSystems as the shared object
_systems = new EcsSystems (new EcsWorld (), serviceContainer);
```

## Systems

In your systems, we can fetch these systems using `EcsServiceInject`, which is an added injector (based on [ecslite-di](https://github.com/LeoECSCommunity/ecslite-di)) that will extract services from the ServiceContainer (via the EcsSystems.Shared property)

```C#
public class SpawnSystem : IEcsRunSystem  
{
	//These will automatically extract the relevant service from the ServiceContainer.
	private readonly EcsServiceInject<ITimeService> _timeService;  
	private readonly EcsServiceInject<ILogService> _logService;  
	private readonly EcsServiceInject<IPrefabLocatorService> _prefabLocatorService;  
	private readonly EcsServiceInject<IPerformanceService> _performanceService;

	public void Run(EcsSystems systems)  
	{  
		//And we can access the result via the Value property.
		if (_timeService.Value.CurrentTime > _lastSpawnTime + _spawnDelay)  
			if (!_performanceService.Value.LowPerformanceWarning)  
				Spawn();  
	}
	
	private void Spawn()
	{
		_prefab ??= _prefabLocatorService.Value;
		_prefab.Instantiate();
		_logService.Value.DebugLog("Spawned!");
	}
}
```

By referencing interfaces and compositing implementations in the `IServiceContainer` at the startup (where the game engine world sets up the ECS simulation), our systems code remains engine-agnostic and decoupled.  Different scenes could provide different implementations of the interface, or the ECS systems could be ported to another game engine.  This decoupling of our setup and game engine code from our ECS systems makes it trivially easy to perform tasks such as swap data sources, etc with test services during development.

# ServiceContainer in depth
While the example above will get you started, understanding the details will help increase what you can accomplish.

## ServiceContainer
The `ServiceContainer` static class is provides global access to the configured global IServiceContainer.  It provides the same API as the IServiceContainer (covered next), with two extra methods to facilitate setting and fetching the global container.
```C#

//... IServiceContainer implementation on the global container...
public static IServiceContainer Add<T>(T serviceInstance)
public static T Get<T>()
public static bool TryGet<T>(out T result)
public static void Remove<T>()
// + 

	/// <summary>  
	///     Fetches the current global container, if there is one.  
	/// </summary>  
	/// <returns>Current global container, or null.</returns>  
	public static IServiceContainer GetCurrentContainer()  
	
	/// <summary>  
	///     Assigns the current container as the globally accessible container. 
	/// </summary> 
	/// <param name="container"> 
	///     The container to assign.  Pass null to clear the 
	///     existing container. 
	/// </param>
	public static void SetContainer(IServiceContainer container)  
```

Usually, you won't need to call these methods yourself inside of the your Systems.  However, GetCurrentContainer is used to fetch an externally configured container to hand to the EcsSystems.Shared to enable service injection.  Calling .Finalize on a container will call SetCurrentContainer to set it as the globally accessible container, but you can also call it yourself to manually manipulate the global container if you wish.


## IServiceContainer
The `IServiceContainer` interface is the meat of the entire paradigm.  It stores references to instances of types and makes them available upon request.  Unlike many more opinionated IOC implementations, it does not do any initialization or injection on those types, and thus types can be added and removed from the container after initialization if the developer chooses.  However, it is good practice to avoid this where possible, as concentrating all container setup to a single place reduces complexity, simplifies debugging, and makes following good habits (like caching services) possible.

```C#
/// <summary>  
///     Adds a service and makes it available for requests.  Specifying the type parameter gives control over how  
///     the type should be requested  (such as by a particular interface)  
/// </summary>  
/// <param name="serviceInstance">the service to make available to requesters</param>  
/// <typeparam name="T">The type used to request this service</typeparam>  
/// <returns>the container, for chaining.</returns>  
public static IServiceContainer Add<T>(T serviceInstance)

/// <summary>  
///     Requests a service from the container.  Throws exception if it cannot be found.  Use TryGet to avoid this.  
/// </summary>  
/// <typeparam name="T">Type to request</typeparam>  
/// <returns>the service matching the requested type.</returns>  
public static T Get<T>()

/// <summary>  
///     Requests a service from the container, and notifies the caller if the service is not available.  
/// </summary>  
/// <param name="result">the service, if it exists</param>  
/// <typeparam name="T">Type to request</typeparam>  
/// <returns>true if the service was found, false otherwise.</returns>  
public static bool TryGet<T>(out T result)

/// <summary>  
///     Removes the specified service from the container.  
/// </summary>  
/// <typeparam name="T">the type to remove</typeparam>  
void Remove<T>();
```

## BasicServiceContainer
A simple, provided implementation of the `IServiceContainer` interface.  However, it does have a trick up its sleeve that is worth noting.  Notice that the `IServiceContainer` API exposes an `Add<T>(T serviceInstance)` method.  This method is written as such to give the Developer control over how consumers request information from the container.  For instance:
```C#
	container.Add<IPrefabService>(new UnityPrefabLocatorService())  
 ```

Here, we've provided the hint that this service should be asked for by the `IPrefabService` interface, like so:
```C#
	ServiceContainer.Get<IPrefabService>()
```

However, modern C# allows the type of a generic method to be inferred from its parameter.  In this case, allowing the method to be called like so:
```C#
	container.Add(new UnityPrefabLocatorService())  
```
The compiler allows this, but registers the service with the full `UnityPrefabLocatorService` Type, which would cause later lookups on `IPrefabService` to fail.  Because of this, the BasicServiceContainer contains a fallback on a lookup failure to search for a unique case of the interface among the registered types.  If it finds one, it adds an additional reference internally so that future lookups will find the correct result immediately.  However, because of this, the Remove call is more costly, as it also has to search for all references that may have been "learned", but we feel the tradeoff is worth it.

## EcsServiceInject
A simple extension of the [ecslite-di](https://github.com/LeoECSCommunity/ecslite-di) framework that automatically calls .Get on the service container internally and exposes the result in the .Value property.  Also has an implicit operator so it can be directly passed in cases where the compiler knows that the internal type is what is desired.

# (Optional) Unity gameobject-based compositor

In addition to the code-based technique shown above, the package also comes with a Unity adapter that lets you register Unity MonoBehaviours as services directly with the hierarchy.  This adds further convenience by allowing editor-based customization of the services.  To use the Unity adapter, simply add a gameobject to your scene and add the `ServiceContainerHost` component, with MonoBehaviours implementing `IHostedService` to the gameobject or as children.

## ServiceContainerHost

![enter image description here](https://imgur.com/Qlmb6g5.png)
  
|Setting| Effect |
|--|--|
| Do Not Destroy On Load | Marks this Service Container as universal, persisting through scene loads.  Handy if you want all the services always available. |
| Host All | By default, only MonoBehaviours that implement IHostedService are added to the ServiceContainer.  Checking this adds ALL MonoBehaviors on this GameObject and child GameObjects. |
| Verbose | Logs the service additions to the console.  Handy if you are using Service Containers in scene. |

Services can be added to the container by adding them to the ServiceControllerHost or as a child gameobject.  If `Host All` is unchecked, these services must derive from `IHostedService` to be included.

## IHostedService
Simply an flag interface that marks a MonoBehaviour as one that should be hosted (if `Host All` is unchecked in the `ServiceContainerHost`).  Simply add the interface to the class, it has no requirements.


# FAQ

### How can I customize the behavior of the the BasicServiceContainer?
Simple!  All of the content is designed to function against the `IServiceContainer` interface.  Simply implement the interface yourself, using the BasicServiceContainer as a rough template.


# License
All code in this repository is covered by the [Mozilla Public License](https://www.mozilla.org/en-US/MPL/), which states that you are free to use the code for any purpose, commercial or otherwise, for any type of application or purpose, and that you are free to release your works under whatever license you choose.  However, regardless of application or method, this code remains under the MPL license, and all modifications or portions of it must also remain under the MPL license and be made available, but this is limited to the covered code and modifications to it.  It is NOT viral, nor does it enforce the MPL license on any other portion of your code, as in strong copyleft licenses like GPL and its derivatives.   The intent is that **this** code is MPL, shall always be MPL regardless of author, and that it and all modified versions should be public and freely available to all to use in any way they see fit, without imposing restrictions or obligations on any other code the user may use or write.

Simple guidelines:
| Use| Modify |
|--|--|
| Put a text file in your distribution that states OSS usage, with a link to this repository among any others. | Same as **Use** and make modifications public under the MPL by either issuing a pull request to this repository, forking it, or hosting your own. |

However, these are only guidelines, please see the actual license and [Additional license FAQ](https://www.mozilla.org/en-US/MPL/2.0/FAQ/) for actual terms and conditions.

This code is explicitly NOT compatible with GPL licenses or any other "viral" license, as that runs counter to the stated goal of keeping this code freely available to all, including proprietary use.
