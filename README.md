# ☄️ The cscore Library

`cscore` is a lightweight library providing commonly used helpers & patterns for your C# projects like events, injection logic, logging and much more (examples below). It can be used in both pure **C#** and **Unity** projects.
All components are loosly coupled so that components can be used individually when needed without deep knowledge about the full cscore library required. 

[**Website**](https://www.csutil.com/projects/cscore) 
**•**
[**GitHub**](https://github.com/cs-util-com/cscore) 
**•**
[**Examples**](#-usage--examples) 
**•**
[**Getting started**](#-getting-started)

#  Overview 
See the [examples](#-usage--examples) below to get a quick overview of all library features:


### Pure C# Components
The aim of the cscore package is to stay is slim/minimal as possible while including the feature and functionality typical projects would benefit from.

* [Log](#logging) - A minimalistic logging wrapper + [AssertV2](#assertv2) to add saveguards anywhere in your logic
* [EventBus](#The-EventBus) - Publish and subscribe to global events from anywhere in your code. Sends **1 million events in under 3 seconds** with minimal memory footprint!
* [Injection Logic](#Injection-Logic) - A simple inversion of control pattern that does not rely on magic. Relies on the EventBus system, so its super fast as well!
* [JSON Parsing](#JSON-Parsing) - Reading and writing JSON through a simple interface. Default implementation uses [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) to ensure high performance
* [REST Extensions](#REST-Extensions) - Extensions to simplify sending REST requests in as few lines as possible without limiting flexibility
* [Directory & File Extensions](#directory--file-extensions) - To simplify handling files, folders and persisting data
* Common String extension methods demonstrated in StringExtensionTests.cs
* [Functional extensions](#IEnumerable-Extensions) and [Transducers](#Transducers) to allow functional data mapping (filter, map, reduce, ..)
* Simple [statemachines](#Statemachines) that work on your existing classes
* An asynchronous chainable [key value store](#KeyValueStore) (get & set) that can be used for simple persistent settings but also for remote server/DB access 
* An [immutable datastore](#immutable-datastore) (Redux syntax) that includes undo/redo, timetravel (replay recordings) and a thunk middleware (dispatching async tasks)
* A [JsonMerger](#JsonMerger) helper to allow simple Json merging and diffing logic that helps to update an instance of a class using a [Three-way merge](https://en.wikipedia.org/wiki/Merge_(version_control)#Three-way_merge)
* Many other helpful extension methods best demonstrated in HelperMethodTests.cs


### Additional Unity Components
* [GameObject.Subscribe & MonoBehaviour.Subscribe](#gameobjectsubscribe--monobehavioursubscribe) - Listening to events while respecting the lifecycle of Unity objects
* [MonoBehaviour Injection & Singletons](#monobehaviour-injection--singletons) - Using the injection logic to create and access Unity objects 
* [The Link Pattern](#the-link-pattern) - Making it easy to connect prefabs with code (and by that separate design & UI from your logic)
* [The ViewStack Pattern](#the-viewstack-pattern) - Using GameObjects as separate views stacked in a parent object and controlled by a single ViewStack to introduce a simple solution for switching views and UI screens. 
* [MonoBehaviour.ExecuteDelayed & MonoBehaviour.ExecuteRepeated](#monobehaviourexecutedelayed--monobehaviourexecuterepeated) - Executing asynchronous actions delayed and/or repeated
* [UnityWebRequest.SendV2](#unitywebrequestsendv2) - UnityWebRequest extension methods
* [PlayerPrefsV2](#playerprefsv2) - Adds `SetBool`, `SetStringEncrypted` and more, see PlayerPrefsV2Tests.cs for all examples
* [Running xUnit tests in Unity](#Running-xUnit-tests-in-Unity) - Execute your xUnit tests in Unity even in the built application to ensure everything works as expected in the production runtime

### Status
![](https://img.shields.io/badge/Maintained%3F-yes-green.svg?style=flat-square)
![](https://img.shields.io/github/last-commit/cs-util-com/cscore.svg?colorB=4267b2&style=flat-square)
![](https://img.shields.io/github/issues-closed/cs-util-com/cscore.svg?colorB=006400&style=flat-square)
[![](https://badge.waffle.io/cs-util-com/cscore.svg?columns=all&style=flat-square)](https://waffle.io/cs-util-com/cscore)

* To get started, see the [installation instructions](#-getting-started) below.
* To ensure full test coverage mutation testing is used (thanks to [Stryker](https://github.com/stryker-mutator/stryker-net)!)
* To get in contact and stay updated [see the links below](#How-to-get-in-contact)




# 💡 Usage & Examples
See below for a full usage overview to explain the APIs with simple examples.


## Logging
A lightweight zero config Log wrapper that is automatically **stripped from production** builds and can be combined with other logging libraries like Serilog for more complex usecases

```cs
Log.d("I'm a log message");
Log.w("I'm a warning");
Log.e("I'm an error");
Log.e(new Exception("I'm an exception"));
Log.w("I'm a warning with parmas", "param 1", 2, "..");
```

This will result in the following output in the Log:
```
> I'm a log message
  at LogTests.TestBasicLogOutputExamples() 

> WARNING: I'm a warning
  at LogTests.TestBasicLogOutputExamples() 

>>> ERROR: I'm an error
    at Log.e(System.String error, System.Object[] args) c:\...\Log.cs:line 25
     LogTests.TestBasicLogOutputExamples() c:\...\LogTests.cs:line 15

>>> EXCEPTION: System.Exception: I'm an exception
    at Log.e(System.Exception e, System.Object[] args) c:\...\Log.cs:line 29
     LogTests.TestBasicLogOutputExamples() c:\...\LogTests.cs:line 16

> WARNING: I'm a warning with parmas : [[param 1, 2, ..]]
  at LogTests.TestBasicLogOutputExamples()
```

Creating logging-adapters is simple, the following logging-adapters can be used out of the box (and they can be seen as examples/templates):

- [LogToConsole.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/PlainNetClassLib/src/Plugins/CsCore/com/csutil/logging/LogToConsole.cs) - The default logger which uses the normal `System.Console` 
- [LogToUnityDebugLog.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/logging/LogToUnityDebugLog.cs) - The default logger when using the library in Unity projects, when using it `UnityEngine.Debug.Log` is used for all logging events  
- [LogToFile.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/PlainNetClassLib/src/Plugins/CsCore/com/csutil/logging/LogToFile.cs) - Allows to write all log outputs into a persisted file
- [LogToMultipleLoggers.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/PlainNetClassLib/src/Plugins/CsCore/com/csutil/logging/LogToMultipleLoggers.cs) - Allows to use multiple loggers in parallel, e.g. to log to the console, a file and a custom error reporting system simultaneously

The used logging-adapter can be set via `Log.instance = new MyCustomLogImpl();`

Through this abstraction it becomes easy to later switch to more complex logging backends, e.g. the [Serilog logging library](https://github.com/serilog/serilog), while keeping your code unchanged. 

### AssertV2

- `AssertV2` can be used anywhere in your code 
- Will be automatically removed/stripped from your production code
- Can be configured to `Log.e` an error (the default) or to throw an exception 
- Use `AssertV2` in places where you would otherwise add a temporary `Log` line while testing. `AssertV2` can stay in your code and will let you know of any unexpected behaviour 
- Will automatically pause the Debugger if it fails while debugging

```cs
AssertV2.IsTrue(1 + 1 == 3, "This assertion will fail");
```
See [here](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/LogTests.cs#L63) for more examples.

### Log.MethodEntered & Log.MethodDone

- Simple monitoring of method calls and method-timings to detect abnormal behavior
- Easy to follow logging pattern for each method or method section where logging is helpful
- Optional `maxAllowedTimeInMs` assertion at the end of the method
- The returned `Stopwatch` can be used for additional logging if needed  

```cs
private void SomeExampleMethod1(string s, int i) {
    Stopwatch timing = Log.MethodEntered("s=" + s, "i=" + i);
    
    { // .. here would be some method logic ..
        Thread.Sleep(3);
    } // .. as the last line in the tracked method add:
    
    Log.MethodDone(timing, maxAllowedTimeInMs: 50);
    // If the method needed more then 50ms an error is logged
}
```

This will result in the following output in the Log:
```cs
>  --> LogTests.SomeExampleMethod1(..) : [[s=I am a string, i=123]]
  at LogTests.SomeExampleMethod1(System.String s, Int32 i) 

>     <-- LogTests.SomeExampleMethod1(..) finished after 3 ms
  at LogTests.SomeExampleMethod1(System.String s, Int32 i) 
```



## The EventBus

- Publish and subscribe to global events from anywhere in your code
- Sends **1 million events in under 3 seconds** with minimal memory footprint! ([Tested](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/com/csutil/tests/EventBusTests.cs#L158) on my mid-range laptop - will add some more detailed numbers soon)

```cs
// The EventBus can be accessed via EventBus.instance
EventBus eventBus = EventBus.instance;
string eventName = "TestEvent1";

//Register a subscriber for the eventName that gets notified when ever an event is send:
object subscriber1 = new object(); // can be of any type
eventBus.Subscribe(subscriber1, eventName, () => {
    Log.d("The event was received!");
});

// Now send out an event:
eventBus.Publish(eventName);

// When subscribers dont want to receive events anymore they can unsubscribe:
eventBus.Unsubscribe(subscriber1, eventName);
```


__Rule of thumb__: Only use an `EventBus` if you can't exactly tell who will listen to the published events. Do not use the `EventBus` to pass an event from x to y if you know exactly who x and y will be! Artificially separating 2 components that tightly belong together does not help



## Injection Logic

- A simple inversion of control pattern with the main call being `MyClass1 x = IoC.inject.Get<MyClass1>(this);` where `this` is the requesting entity
- Relies on the EventBus system, so its **very fast** with **minimal memory footprint** as well!
- Free of any magic via anotations (at least for now;) - I tried to keep the injection API as simple as possible, existing libraries often tend to overcomplicate things in my opinion
- Lazy loading, singletons and transient types (every inject request creates a new instance) are all easily implementable via `.RegisterInjector`, see examples below:

```cs
// The default injector can be accessed via IoC.inject
Injector injector = IoC.inject;

// Requesting an instance of MyClass1 will fail because no injector registered yet to handle requests for the MyClass1 type:
Assert.Null(injector.Get<MyClass1>(this));

// Setup an injector that will always return the same instance for MyClass1 when IoC.inject.Get<MyClass1>() is called:
MySubClass1 myClass1Singleton = new MySubClass1();
injector.SetSingleton<MyClass1, MySubClass1>(myClass1Singleton);

// Internally .SetSingleton() will register an injector for the class like this:
injector.RegisterInjector<MyClass1>(new object(), (caller, createIfNull) => {
    // Whenever injector.Get is called the injector always returns the same instance:
    return myClass1Singleton; // Here the singleton could be lazy loaded
});

// Now calling IoC.inject.Get<MyClass1>() will always result in the same instance:
MyClass1 myClass1 = injector.Get<MyClass1>(this);
Assert.Same(myClass1Singleton, myClass1); // Its the same object reference
```

Another extended example usage can be found in [`InjectionTests.ExampleUsage2()` (see here)](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/com/csutil/tests/InjectionTests.cs#L40)



## IEnumerable Extensions
For common tasks on IEnumerables cscore provides methods like `Map` (same as LINQs Select), `Reduce` (same as LINQs Aggregate) and `Filter` (same as LINQs Where) but also `IsNullOrEmpty` and `ToStringV2` which are explained in this simple example:

```cs
IEnumerable<string> myStrings = new List<string>() { "1", "2", "3", "4", "5" };
IEnumerable<int> convertedToInts = myStrings.Map(s => int.Parse(s));
IEnumerable<int> filteredInts = convertedToInts.Filter(i => i <= 3); // Keep 1,2,3
Assert.False(filteredInts.IsNullOrEmpty());
Log.d("Filtered ints: " + filteredInts.ToStringV2(i => "" + i)); // "[1, 2, 3]"
int sumOfAllInts = filteredInts.Reduce((sum, i) => sum + i); // Sum up all ints
Assert.Equal(6, sumOfAllInts); // 1+2+3 is 6
```

More usage examples can be found in the [HelperMethodTests.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/com/csutil/tests/HelperMethodTests.cs)



## Transducers
Transducers allow to do similar things as the functional concepts like ``Filter``, ``Map`` and ``Reduce``. The main idea of transducers is to make this functional style as efficient as possible, iterating through the target structure only once and bulding a pipeline still of the same easy to understand functional building blocks. 

A first example that uses only ``Filter`` will give a better idea how this looks like:

```cs
List<int> testData = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

var filter1 = Transducers.NewFilter<int>(x => x > 4);
var filter2 = Transducers.NewFilter<int>(x => x % 2 != 0);
{
    List<int> result = testData.FilterToList(Transducers.Compose(filter1, filter2));
    Assert.Equal(2, result.Count()); // 6 and 8 will be left
    Assert.Equal(5, result.First());
    Assert.Equal(7, result.Last());
}
{ // without Transducers.Compose the filters have to be chained manually:
    List<int> result = testData.FilterToList(x => (filter1(filter2(x))));
    Assert.Equal(2, result.Count()); // 6 and 8 will be left
    Assert.Equal(5, result.First());
    Assert.Equal(7, result.Last());
}
```

A more complex example that uses ``Filter``, ``Map`` and ``Reduce``:
```cs
List<MyClass1> testData = newExampleList();

Transducer<MyClass1, MyClass1> filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
Transducer<MyClass1, MyClass1> filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
Transducer<MyClass1, int> mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
Func<int, int, int> sumReducer = (total, x) => total + x;

// Create the reducer by composing the transducers:
var sum = testData.ReduceTo(x => filter1(filter2(mapper(x))), sumReducer, seed: 0);
Assert.Equal(6, sum);
```
More examples can be found in the [TransducerTests.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/com/csutil/tests/datastructures/TransducerTests.cs). The syntax is still work in progress and I am happy for any suggestions how to improve this. And there are some great [related sources](https://jrsinclair.com/articles/2019/magical-mystical-js-transducers/) you can read to learn more about Transducers.



## JSON Parsing 
- The `JsonWriter` and `JsonReader` interfaces are an abstraction that should be flexiable enough to be used for most usecases. 
- The underlying implementation can easily be swapped if needed and the default implementation uses [Json.NET](https://github.com/JamesNK/Newtonsoft.Json).

```cs
class MyClass1 { // example class with a field and a property
    public string myString;
    public string myString2 { get; set; }
}

MyClass1 x1 = new MyClass1() { myString = "abc", myString2 = "def" };

// Generate a json object from the object that includes all public fields and props:
string jsonString = JsonWriter.GetWriter().Write(x1);

// Parse the json string back into a second instance x2 and compare both:
MyClass1 x2 = JsonReader.GetReader().Read<MyClass1>(jsonString);
Assert.Equal(x1.myString, x2.myString);
Assert.Equal(x1.myString2, x2.myString2);
```


## REST Extensions 
```cs
// The property names are based on the https://httpbin.org/get json response:
class HttpBinGetResp { 
    public string origin { get; set; }
    public Dictionary<string, object> headers { get; set; }
}

RestRequest request = new Uri("https://httpbin.org/get").SendGET();

// Send the request and parse the response into the HttpBinGetResp class:
HttpBinGetResp response = await request.GetResult<HttpBinGetResp>();
Log.d("Your external IP is " + response.origin);
```

A more complex REST example can be found in the `WeatherReportExamples` test class. It uses your IP to detect
the city name you are located in and then sends a weather report request to MetaWeather.com:
```cs
var ipLookupResult = await IpApiCom.GetResponse();
string yourCity = ipLookupResult.city;
var cityLookupResult = await MetaWeatherLocationLookup.GetLocation(yourCity);
int whereOnEarthIDOfYourCity = cityLookupResult.First().woeid;
var weatherReports = await MetaWeatherReport.GetReport(whereOnEarthIDOfYourCity);
var currentWeather = weatherReports.consolidated_weather.Map(r => r.weather_state_name);
Log.d("The weather today in " + yourCity + " is: " + currentWeather.ToStringV2());
```


## Directory & File Extensions 
The [DirectoryInfo](https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo) and [FileInfo](https://docs.microsoft.com/en-us/dotnet/api/system.io.fileinfo) classes already provide helpful interfaces to files and directories and the following extensions improve the usability if these classes:

```cs
// Get a directory to work in:
DirectoryInfo myDirectory = EnvironmentV2.instance.GetAppDataFolder();
Log.d("The directory path is: " + myDirectory.FullPath());

// Get a non-existing child directory
var childDir = myDirectory.GetChildDir("MyExampleSubDirectory1");

// Create the sub directory:
childDir.CreateV2(); // myDirectory.CreateSubdirectory("..") works too

// Rename the directory:
childDir.Rename("MyExampleSubDirectory2");

// Get a file in the child directory:
FileInfo file1 = childDir.GetChild("MyFile1.txt");

// Saving and loading from files:
string someTextToStoreInTheFile = "Some text to store in the file";
file1.SaveAsText(someTextToStoreInTheFile);
string loadedText = file1.LoadAs<string>(); // loading JSON would work as well
Assert.Equal(someTextToStoreInTheFile, loadedText);

// Deleting directories:
Assert.True(childDir.DeleteV2()); // (Deleting non-existing directories would returns false)
// Check that the directory no longer exists:
Assert.False(childDir.IsNotNullAndExists());
```

## StateMachines 
A statemachine in it's simplest form is a current state and a set of allowed state transitions. Transitioning from state 1 to 2 can be done in a single short [method](https://github.com/cs-util-com/cscore/blob/master/CsCore/PlainNetClassLib/src/Plugins/CsCore/com/csutil/datastructures/StateMachine.cs#L14) on the set of allowed transitions. Here an example statemachine: 

```cs
 // First define a set of allowed transitions to define the state machine:
Dictionary<MyStates, HashSet<MyStates>> stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2); // 1 => 2 allowed
stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState3); // 2 => 3 allowed

// Initialize a state-machine:
MyStates currentState = MyStates.MyState1;

// It is possible to listen to state machine transitions:
StateMachine.SubscribeToAllTransitions<MyStates>(new object(), (machine, oldState, newState) => {
    Log.d("Transitioned from " + oldState + " to " + newState);
});
// And its possible to listen only to specific transitions:
StateMachine.SubscribeToTransition(new object(), MyStates.MyState1, MyStates.MyState2, delegate {
    Log.d("Transitioned from 1 => 2");
});

// Transition the state-machine from state 1 to 2:
currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
Assert.Equal(MyStates.MyState2, currentState);

// Invalid transitions throw exceptions (current state is 2):
Assert.Throws<InvalidOperationException>(() => {
    currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
});
```

More [statemachine examples can be found here](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/StateMachineTests.cs#L17).

## KeyValueStore
Provides an async chainable key value store (get & set) that can be used for simple persistent settings but also for remote server/DB access. Different store implementations are included for some common use cases:
* InMemoryKeyValueStore - Keeps a very fast in memory dictionary for fastest possible read write
* FileBasedKeyValueStore - Enables persisting values permanently 
* RetryKeyValueStore - A retry layer using exponential backoff 
* ExceptionWrapperKeyValueStore - To handle exceptions of an inner store (e.g. if a connection to a remote server throws a timeout exception this can be handled to return the cached local value instead)

```cs
IKeyValueStore store = new InMemoryKeyValueStore();
string myKey1 = "myKey1";
MyClass1 x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
store.Set(myKey1, x1);
MyClass1 x2 = store.Get<MyClass1>(myKey1, defaultValue: null).Result;
Assert.Equal(x1.myString1, x2.myString1);
Assert.Equal(x1.myString2, x2.myString2);
```

The KeyValueStores can be chained so that if the outer store does not find the element it will ask the next inner store. This allows to have fast stores like the ``InMemoryKeyValueStore`` on the most outer level and the slowest stores like the connection to the database on the most inner one:

```cs
string myKey1 = "test123";
MyClass1 x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
// Create a fast memory store and combine it with a LiteDB store that is persisted to disk:
IKeyValueStore store = new InMemoryKeyValueStore().WithFallbackStore(new FileBasedKeyValueStore(EnvironmentV2.instance.GetOrAddTempFolder("SomeFolder123")));
await store.Set(myKey1, x1);
MyClass1 x2 = await store.Get<MyClass1>(myKey1, null);
Assert.Equal(x1.myString1, x2.myString1);
Assert.Equal(x1.myString2, x2.myString2);
```

More examples [can be found here](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/datastructures/KeyValueStoreTests.cs).

## Immutable DataStore
* It uses the Redux syntax and core principles to not reinvent a matured and well proven wheel
* Enables undo/redo of all dispatched actions out of the box without any additional work 
* Enables timetravel to enable recording the full userinteraction and replaying it later to get back into the same state
* Includes a thunk middleware to dispatcht async tasks (e.g. talking to a remote server)

See [this example for a first introduction](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/model/immutable/DataStoreExample1.cs#L11) including an example datamodel, example actions and some listeners that are informed when the datamodel changes.

See [here for additional more complex examples](https://github.com/cs-util-com/cscore/tree/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/model/immutable) which include the other features like undo/redo, middlewares and server synchronization. 

## JsonMerger
[Json merging and diffing logic](https://github.com/cs-util-com/cscore/blob/master/CsCore/xUnitTests/src/Plugins/CsCoreXUnitTests/com/csutil/tests/json/JsonDiffAndMergeTests.cs#L14) that helps to update an instance of a class using a [Three-way merge](https://en.wikipedia.org/wiki/Merge_(version_control)#Three-way_merge). Here an example:

```cs
MyClass1 originalObj = new MyClass1() { myString = "abc", myString2 = "def" };

MyClass1 copy1 = originalObj.DeepCopyViaJson();
copy1.myString = "abcd";
copy1.complexField = new MyClass1() { myString = "123", myString2 = "456" };
copy1.complexField.complexList = new List<MyClass1>() { new MyClass1() { myString = "listEntry1" } };

MyClass1 copy2 = originalObj.DeepCopyViaJson();
copy2.myString2 = "defg";

var merge = MergeJson.Merge(originalObj, copy1, copy2);
Assert.False(merge.hasMergeConflict);

// Parse the merged result back into a MyClass1 object:
MyClass1 mergeResult1 = merge.GetResult();
// The changes from both copies were merged correctly:
Assert.Equal(copy1.myString, mergeResult1.myString);
Assert.Equal(copy2.myString2, mergeResult1.myString2);
```


# Unity Component Examples
There are additional components specifically created for Unity, that will be explained below:

## `GameObject` and `MonoBehaviour` Extensions

Some helper methods are added when the com.csutil namespace is imported to help with scene graph manipulation via code. The added extension methods are `GetParent`, `AddChild`, `GetOrAddChild`, `GetOrAddComponent`, `Destroy` and `IsDestroyed`. Here are some examples:

```cs
GameObject myGo = new GameObject();
// Adding children GameObjects via AddChild:
GameObject myChildGo = myGo.AddChild(new GameObject());
// Getting the parent of the child via GetParent:
Assert.AreSame(myGo, myChildGo.GetParent());

// Lazy-initialization of the GameObject in case it does not yet exist:
GameObject child1 = myGo.GetOrAddChild("Child 1");
// Lazy-initialization of the Mono in case it does not yet exist:
MyExampleMono1 myMono1 = child1.GetOrAddComponent<MyExampleMono1>();
// Calling the 2 methods again results always in the same mono:
var myMono1_ref2 = myGo.GetOrAddChild("Child 1").GetOrAddComponent<MyExampleMono1>();
Assert.AreSame(myMono1, myMono1_ref2);

myGo.Destroy(); // Destroy the gameobject
Assert.IsTrue(myGo.IsDestroyed()); // Check if it was destroyed
```

## `GameObject.Subscribe` & `MonoBehaviour.Subscribe`

There are extension methods for both `GameObjects` and `Behaviours` which internally handle the lifecycle of their subscribers correctly. If a `GameObject` for example is currently not active or was destroyed the published events will not reach it.

```cs
// GameObjects can subscribe to events:
var myGameObject = new GameObject("MyGameObject 1");
myGameObject.Subscribe("MyEvent1", () => {
    Log.d("I received the event because I'm active");
});

// Behaviours can subscribe to events too:
var myExampleMono = myGameObject.GetOrAddComponent<MyExampleMono1>();
myExampleMono.Subscribe("MyEvent1", () => {
    Log.d("I received the event because I'm enabled and active");
});

// The broadcast will reach both the GameObject and the MonoBehaviour:
EventBus.instance.Publish("MyEvent1");
```


## MonoBehaviour Injection & Singletons
Often specific `MonoBehaviours` should only exist once in the complete scene, for this scenario `IoC.inject.GetOrAddComponentSingleton()` and `IoC.inject.GetComponentSingleton()` can be used.

```cs
// Initially there is no MonoBehaviour registered in the system:
Assert.IsNull(IoC.inject.Get<MyExampleMono1>(this));

// Calling GetOrAddComponentSingleton will create a singleton:
MyExampleMono1 x1 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this);

// Calling GetOrAddComponentSingleton again now returns the singleton:
MyExampleMono1 x2 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this);
Assert.AreSame(x1, x2); // Both references point to the same object

// Calling the default IoC.inject.Get will also return the same singleton:
MyExampleMono1 x3 = IoC.inject.Get<MyExampleMono1>(this);
Assert.AreSame(x1, x3); // Both references point to the same object
```

Calling `GetOrAddComponentSingleton` will create a singleton. The parent gameobject of this singleton will be created together with it in the scene. The location of the singleton will be:

`"Singletons" GameObject` -> `"MyExampleMono1" GameObject` -> `MyExampleMono1`

This way all created singletons will be created and grouped together in the `"Singletons" GameObject` and accessible like any other MonoBehaviour as well.

### Scriptable Object Injection & Singletons
Scriptable objects are ment as data containers created not at runtime but at editor time to store configuration data and use it in the editor UI or load it during runtime. 
* The scriptable object consists of the class that extends `ScriptableObject` and the instance file that typically is created via the [CreateAssetMenu](https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html) annotation or via an editor script (see [ScriptableObject.CreateInstance](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)).
* This allows to have many parallel instance files for a scriptable object that contain different configurations. These asset files can be loaded during runtime when placed in a [Resources folder](https://unity3d.com/learn/tutorials/topics/best-practices/resources-folder) or can be linked directly in prefabs and Unity scenes in the Editor UI. 

If scriptable object instances have to be dynamically loaded during runtime, the following example can help to avoid loading multiple different instances for the same ScriptableObject subclass into memory at once:

```cs
// Load a ScriptableObject instance and set it as the singleton:
var path = "MyExampleScriptableObject_Instance1.asset";
MyExampleScriptableObject x1 = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(path);
IoC.inject.SetSingleton(x1);

// Now that the singleton is set this instance is always returned for the ScriptableObject class:
MyExampleScriptableObject x2 = IoC.inject.Get<MyExampleScriptableObject>(this);
Assert.AreSame(x1, x2);
```


## The `Link` Pattern
Connecting prefabs created by designers with internal logic (e.g what should happen when the user presses Button 1) often is beneficial to happen in a central place. To access all required parts of the prefab the `Link` pattern and helper methods like `gameObject.GetLinkMap()` can be used:
```cs
// Load a prefab that contains Link MonoBehaviours:
GameObject prefab = ResourcesV2.LoadPrefab("ExamplePrefab1.prefab");

// Collect all Link MonoBehaviours in the prefab:
Dictionary<string, Link> links = prefab.GetLinkMap();

// In the Prefab Link-Monos are placed in all GameObjects that need 
// to be accessed by the code. Links have a id to reference them:
// Via the Link.id the objects can quickly be accessed: 
Assert.IsNotNull(links.Get<GameObject>("Button 1"));

// The GameObject "Button 1" contains a Button-Mono that can be accessed:
Button button1 = links.Get<Button>("Button 1");
button1.SetOnClickAction(delegate {
    Log.d("Button 1 clicked");
});

// The prefab also contains other Links in other places to quickly setup the UI:
links.Get<Text>("Text 1").text = "Some text";
links.Get<Toggle>("Toggle 1").SetOnValueChangedAction((isNowChecked) => {
    Log.d("Toggle 1 is now " + (isNowChecked ? "checked" : "unchecked"));
    return true;
});
```

## The `ViewStack` Pattern
The ``ViewStack`` Pattern uses GameObjects as separate views stacked in a parent GameObject. A ``ViewStack`` controller attached to this parent object controls switching between views. Views can be hidden or shown through the ``ViewStack`` and new views can be loaded. The main function of the ``ViewStack`` controller is to represent where the root of the ``ViewStack`` can be found, which is especially relevant if multiple ``ViewStack``s are stacked on top of each other. A simple example for stacking multiple ``ViewStack``s would be a main ``ViewStack`` that controls the normal application flow and a second vie stack that is loaded together with one of the view prefabs that represents a temporary tutorial or FUE that the user has to click through. 

```cs
var viewStackGo = new GameObject();
var viewStack = viewStackGo.AddComponent<ViewStack>();

// Views can be added manually without using the ViewStack:
var view1 = viewStackGo.AddChild(new GameObject("View 1"));
// You can get the ViewStack using any child gameobject:
Assert.AreEqual(view1.GetViewStack(), viewStack);
// The latest active view can be accessed from the view stack:
Assert.AreEqual(view1, viewStack.GetLatestView());

// Views can also be added using the ViewStack.ShowView method:
var view2 = viewStack.ShowView(new GameObject("View 2"));
// Hide the old view 1 now that view 2 is on top:
view1.SetActiveV2(false);
Assert.IsFalse(view1.activeInHierarchy);
Assert.AreEqual(view2, viewStack.GetLatestView());

// The ViewStack can be used to return to the last view:
Assert.IsTrue(viewStack.SwitchBackToLastView(view2));
// View 2 will be removed from the view stack by destroying it:
Assert.IsTrue(view2.IsDestroyed());
// Now view 1 is active and visible again:
Assert.IsTrue(view1.activeInHierarchy);
```


## `MonoBehaviour.ExecuteDelayed` & `MonoBehaviour.ExecuteRepeated`
```cs
// Execute a task after a defined time:
myMonoBehaviour.ExecuteDelayed(() => {
    Log.d("I am executed after 0.6 seconds");
}, delayInSecBeforeExecution: 0.6f);

// Execute a task multiple times:
myMonoBehaviour.ExecuteRepeated(() => {
    Log.d("I am executed every 0.3 seconds until I return false");
    return true;
}, delayInSecBetweenIterations: 0.3f, delayInSecBeforeFirstExecution: .2f);
```

Additionally there is myMono.StartCoroutinesInParallel(..) and myMono.StartCoroutinesSequetially(..), see [here](https://github.com/cs-util-com/cscore/blob/master/CsCore/UnityTests/Assets/Tests/CoroutineTests.cs#L103) for details



## `UnityWebRequest.SendV2` 

- It is recommended to use the `Uri` extension methods for requests (see [here](#REST-Extensions)). 
- If `UnityWebRequest` has to be used, then `UnityWebRequest.SendV2()` should be a good alternative. 
- `SendV2` creates the same `RestRequest` objects that the `Uri` extension methods create as well. 

```cs
RestRequest request1 = UnityWebRequest.Get("https://httpbin.org/get").SendV2();
Task<HttpBinGetResp> requestTask = request1.GetResult<HttpBinGetResp>();
yield return requestTask.AsCoroutine();
HttpBinGetResp response = requestTask.Result;
Log.d("Your IP is " + response.origin);

// Alternatively the asynchronous callback in GetResult can be used:
UnityWebRequest.Get("https://httpbin.org/get").SendV2().GetResult<HttpBinGetResp>((result) => {
    Log.d("Your IP is " + response.origin);
});
```



## `PlayerPrefsV2` 

Since the Unity `PlayerPrefs` class uses static methods my normal approach with extension methods won't work here, thats why there is now `PlayerPrefsV2` which extends `PlayerPrefs` and adds the following methods:

- PlayerPrefsV2.`SetBool` & PlayerPrefsV2.`GetBool`
- PlayerPrefsV2.`SetStringEncrypted` & PlayerPrefsV2.`GetStringDecrypted`
- PlayerPrefsV2.`SetObject` & PlayerPrefsV2.`GetObject`

```cs
// PlayerPrefsV2.SetBool and PlayerPrefsV2.GetBool example:
bool myBool = true;
PlayerPrefsV2.SetBool("myBool", myBool);
Assert.AreEqual(myBool, PlayerPrefsV2.GetBool("myBool", defaultValue: false));

// PlayerPrefsV2.SetStringEncrypted and PlayerPrefsV2.GetStringDecrypted example:
PlayerPrefsV2.SetStringEncrypted("mySecureString", "some text to encrypt", password: "myPassword123");
var decryptedAgain = PlayerPrefsV2.GetStringDecrypted("mySecureString", null, password: "myPassword123");
Assert.AreEqual("some text to encrypt", decryptedAgain);

// PlayerPrefsV2.SetObject and PlayerPrefsV2.GetObject example (uses JSON internally):
MyClass1 myObjectToSave = new MyClass1() { myString = "Im a string", myInt = 123 };
PlayerPrefsV2.SetObject("myObject1", myObjectToSave);
MyClass1 objLoadedAgain = PlayerPrefsV2.GetObject<MyClass1>("myObject1", defaultValue: null);
Assert.AreEqual(myObjectToSave.myInt, objLoadedAgain.myInt);

    // MyClass1 would look e.g. like this:
    class MyClass1 {
        public string myString;
        public int myInt;
    }

```

## Running xUnit tests in Unity

Initially I created this test runner to ensure that the xUnit tests I wrote for the pure C# components also were all working when running them on an actual build application, especially on platforms like WebGL this showed a few challanges with the async await Task syntax and some other edgecases. The basic idea was simple:

* Use the tests a small sample applications
* If the tests run correctly in a built application on the target platform the component can be correctly used on this platform 
* A [test runner](https://github.com/cs-util-com/cscore/blob/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/testing/XunitTestRunner.cs) is needed to run the tests. In addition the xUnit classes like the Assert class [need to be implemented](https://github.com/cs-util-com/cscore/blob/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/testing/xunitmocks/Assert.cs) to be used in Unity

The outcome works pretty well and I managed to make all tests run correctly in WebGL which I used as a platform that has a lot of very limiting restrictions like no multithreading, strict sandboxing for file logic, persistance etc

1. You want your xUnit tests to stay in your pure C# project but [you can link them](https://github.com/cs-util-com/cscore/tree/master/CsCore/xUnitTests/src) into your Unity project to include them there as well
2. The xUnit runner needs a hint in which Assembly to search for your tests, for that it needs a fully qualified type name of any of the classes in the tests. 
3. Select the [XunitTestRunnerUi](https://github.com/cs-util-com/cscore/blob/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/testing/XunitTestRunnerScene/XunitTestRunnerUi.cs) component in the [XunitTestRunnerScene.unity](https://github.com/cs-util-com/cscore/tree/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/testing/XunitTestRunnerScene) to [set this fully qualified type name](https://github.com/cs-util-com/cscore/blob/master/CsCore/CsCoreUnity/Plugins/CsCoreUnity/com/csutil/testing/XunitTestRunnerScene/XunitTestRunnerUi.cs#L17) (for example "`MyNamespace.MyClass1, MyAssembly1`"). This name it will use to start searching for all xUnit tests in your Assembly.

Additionally in your nUnit tests you can also use the xUnitTestRunner, e.g. if you want writing all your unit tests with xUnit and use the nUnit tests only to trigger them. See the [XunitTestRunnerTests.cs](https://github.com/cs-util-com/cscore/blob/master/CsCore/UnityTests/Assets/Plugins/CsCoreUnityTests/xUnitMocks/XunitTestRunnerTests.cs#L12) to understand how to use the xUnitTestRunner via code.


# 📦 Getting started

cscore can be used in **Unity** projects but also in pure **C#/.net** projects (see below):

## 🎮 Install cscore into your Unity project

The cscore project has some components that are only usable in Unity projects. The provided Unity package below includes all these components.

### Download the Unity package here:
* From the [Asset Store (https://assetstore.unity.com/packages/tools/integration/cscore-138254)](https://assetstore.unity.com/packages/tools/integration/cscore-138254) 
* From the [/CsCore/UnityPackages folder](https://github.com/cs-util-com/cscore/raw/master/CsCore/UnityPackages/) 

**Important:** If you use `.net 3.5` you will need to extract the ZIP file in the `CsCoreNet3.5Compat` folder, it contains the classes that are missing for `.net 3.5`:
![Step 1](https://i.imgur.com/1DyQ4q1.png)

### Optional experimental Unity features

Some of the included features are disabled by default via the `CSCORE_EXPERIMENTAL` compile time define, to enable these features, go to _Player Settings_ -> _Other Settings_ -> _Scripting Define Symbols_ and add `CSCORE_EXPERIMENTAL` there. See the notes about [Scripting Define Symbols in the Unity Docs](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html) for more details how this works. 

## Install via NuGet (For pure C#/.net projects)

The NuGet package from [nuget.org/packages/com.csutil.cscore](https://www.nuget.org/packages/com.csutil.cscore) can be installed in [multiple ways](https://docs.microsoft.com/en-us/nuget/consume-packages/ways-to-install-a-package), for example via the dotnet CLI:
```XML
dotnet add package Newtonsoft.Json
dotnet add package com.csutil.cscore
```
(If you already have `json.net` installed you dont need to add the `Newtonsoft.Json` package)

Or you manually add the following lines to the your `.csproj` file: 
```XML
<Project Sdk="Microsoft.NET.Sdk">
  ...
  <ItemGroup>
    <PackageReference Include="com.csutil.cscore" Version="*" />
    <!-- https://www.nuget.org/packages/Newtonsoft.Json -->
    <PackageReference Include="Newtonsoft.Json" Version="*" />
  </ItemGroup>
  ...
</Project>
```

After adding the references, install the packages by executing `dotnet restore` inside the project folder.


# 💚 Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

[![Open Source Helpers](https://www.codetriage.com/cs-util-com/cscore/badges/users.svg)](https://www.codetriage.com/cs-util-com/cscore)

## Repository structure and instructions
The cscore project is separated into multiple folders:

* **PlainNetClassLib** - Contains the pure .net logic
* **CsCoreUnity** - Contains the Unity specific logic
* **CsCoreNet3.5Compat** - Contains classes for older Unity projects (that do not use `.net 4+` yet)
* **xUnitTests** - Contains the xunit tests that cover all functionality of the PlainNetClassLib folder
* **UnityTests** - Contains the Unity project with all NUnit tests that cover the Unity spefic CsCoreUnity features
* **UnityPackages** - Contains the ready to download Unity package that can alternatively be loaded via the AssetStore

**Sym Links** can be used to link the original `PlainNetClassLib`, `CsCoreUnity` and `CsCoreNet3.5Compat` folders into your target Unity project. The sym link setup scripts (always named `linkThisIntoAUnityProject`) are located in the component folders (use the `.bat` on Win and `.sh` on Mac). 

<!---
See current features in development here: https://github.com/cs-util-com/cscore/projects/1
-->

## How to get in contact

[![Twitter](https://img.shields.io/twitter/follow/csutil_com.svg?style=for-the-badge&logo=twitter)](https://twitter.com/intent/follow?screen_name=csutil_com)
-- [![Discord](https://img.shields.io/discord/518684359667089409.svg?logo=discord&label=chat%20on%20discord&style=for-the-badge)](https://discord.gg/UCqJjEU)
-- [![Gitter](https://img.shields.io/gitter/room/csutil-com/community.svg?style=for-the-badge&logo=gitter-white)](https://gitter.im/csutil-com)

To stay updated via Email see https://www.csutil.com/updates

## Core principles
- **The main goal**: Keep the API simple to use and provide an intuitive framework for common usecases. Stick to essential features only to keep the library lightweight
- Use **examples** as a kind of test driven development but with focus on usablility of the APIs (tests must focus more on validating correctness but examples can focus more on ease of use of the target API). Thats why each test class contains also a few methods that contain example usage
- Write as much of the logic in pure C# as reasonable, stay backwards compatible to .net 3.5 to support older Unity projects as well
- Use mutation testing to check for **test coverage** on a logic level

# License

![](https://img.shields.io/github/license/cs-util-com/cscore.svg?style=for-the-badge)

[![csutil.com](https://forthebadge.com/images/badges/built-with-love.svg)](https://www.csutil.com/)

<!--- // Other very important badges:
![](https://forthebadge.com/images/badges/made-with-c-sharp.svg)
![](https://forthebadge.com/images/badges/does-not-contain-treenuts.svg)
![](https://forthebadge.com/images/badges/contains-cat-gifs.svg)
![](https://forthebadge.com/images/badges/as-seen-on-tv.svg)
-->

