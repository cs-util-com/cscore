# ☄️ The cscore Library
`cscore` is a minimal, zero-dependency collection of common patterns & helpers needed in most C# projects. It can be used in both pure **C#** and **Unity** projects. 

#  Overview 
See the [examples](#💡-Usage-&-Examples) below to get a quick overview of all library features:

### Pure C# Components
* [Log](#Logging) - A minimalistic logging wrapper 
* [EventBus](#The-EventBus) - Publish and subscribe to global events from anywhere in your code
* [Injection Logic](#Injection-Logic) - A simple inversion of control pattern that does not rely on magic 
* [JSON Parsing](#JSON-Parsing) - Reading and writing JSON through a simple interface
* [REST Extensions](#REST-Extensions) - Extensions to simplify doing REST calls 
* [Directory & File Extensions](#Directory-&-File-Extensions) - To simplify handling handing files and persisting data
* String extension methods demonstrated in StringExtensionTests.cs
* Many other helpfull extension methods demonstrated in HelperMethodTests.cs

### Additional Unity Components
* 
* 
* 

<!-- ### Status -->
![](https://img.shields.io/badge/Maintained%3F-yes-green.svg?style=flat-square)
![](https://img.shields.io/github/last-commit/cs-util-com/cscore.svg?colorB=4267b2&style=flat-square)
![](https://img.shields.io/github/issues-closed/cs-util-com/cscore.svg?colorB=006400&style=flat-square)
[![](https://badge.waffle.io/cs-util-com/cscore.svg?columns=all&style=flat-square)](https://waffle.io/cs-util-com/cscore)

* To get started, see the [installation instructions](#💾-Installation) below.
* To ensure full test coverage mutation testing is used (thanks to [Stryker](#https://github.com/stryker-mutator/stryker-net)!)
* To get in contact and stay updated [see the links below](#How-to-get-in-contact)

# 💡 Usage & Examples
See below for a full usage overview to explain the APIs with simple examples.

## Logging

```cs
Log.d("I'm a log message");
Log.w("I'm a warning");
Log.e("I'm an error");
Log.e(new Exception("I'm an exception"));
Log.w("I'm a warning with parmas:", "param 1", 2, "..");
```

### AssertV2
`AssertV2` can be used anywhere in your code, it will be automatically removed from your production code:
```cs
AssertV2.IsTrue(1 + 1 == 3, "This assertion will fail");
```

### Log.MethodEntered

Simple monitoring of method calls and method-timings to detect abnormal behavior:
```cs
private void SomeExampleMethod1(string s, int i) {
    Stopwatch timing = Log.MethodEntered("s=" + s, "i=" + i);
    { // .. here would be some method logic ..
        Thread.Sleep(1);
    } // .. as the last line in the tracked method add:
    Log.MethodDone(timing, maxAllowedTimeInMs: 50);
    // If the method needed more then 50ms an error is logged
}
```

## The EventBus

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

__Rule of thumb__: Only use the `EventBus` if you can't exactly tell who will listen to the published events. Do not use the `EventBus` to pass an event from x to y if you know exactly who x and y will be! 

## Injection Logic
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
    return myClass1Singleton;
});

// Now calling IoC.inject.Get<MyClass1>() will always result in the same instance:
MyClass1 myClass1 = injector.Get<MyClass1>(this);
Assert.Same(myClass1Singleton, myClass1); // Its the same object reference
```

Another extended example usage can be found in `InjectionTests.ExampleUsage2()`

## JSON Parsing 
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
// The property names are based on the https://httpbin.org/get json response
class HttpBinGetResp { 
    public string origin { get; set; }
    public Dictionary<string, object> headers { get; set; }
}

RestRequest request = new Uri("https://httpbin.org/get").SendGET();
// Send the request and parse the response into the HttpBinGetResp class:
HttpBinGetResp response = await request.GetResult<HttpBinGetResp>();
Log.d("Your external IP is " + response.origin);
```

## Directory & File Extensions 
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


# 💾 Installation

## 📦 Installing cscore into pure C# projects

 `cscore` can be installed via [NuGet](https://www.nuget.org/profiles/csutil.com), add the following lines to the root of your `.csproj` file: 

``` XML
<ItemGroup>
    <PackageReference Include="com.csutil.cscore" Version="*" />
</ItemGroup>
```

After adding the references, install the packages by executing `dotnet restore` inside the project folder.

## 🎮 Installing cscore into Unity projects
Download the Unity package from the release page.

# 💚 Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
<!---
See current features in development here: https://github.com/cs-util-com/cscore/projects/1
-->

## How to get in contact

[![Twitter](https://img.shields.io/twitter/follow/csutil_com.svg?style=for-the-badge&logo=twitter)](https://twitter.com/intent/follow?screen_name=csutil_com)

[![Discord](https://img.shields.io/discord/518684359667089409.svg?logo=discord&label=chat%20on%20discord&style=for-the-badge)](https://discord.gg/bgGqRe)

[![Gitter](https://img.shields.io/gitter/room/csutil-com/community.svg?style=for-the-badge&logo=gitter-white)](https://gitter.im/csutil-com)

To stay updated via Email see https://www.csutil.com/updates

# License

![](https://img.shields.io/github/license/cs-util-com/cscore.svg?style=for-the-badge)

[![csutil.com](https://forthebadge.com/images/badges/built-with-love.svg)](https://www.csutil.com/)

<!--- // Other very important badges:
![](https://forthebadge.com/images/badges/made-with-c-sharp.svg)
![](https://forthebadge.com/images/badges/does-not-contain-treenuts.svg)
![](https://forthebadge.com/images/badges/contains-cat-gifs.svg)
![](https://forthebadge.com/images/badges/as-seen-on-tv.svg)
-->

