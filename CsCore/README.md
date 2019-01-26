# cscore
cscore is a minimal zero dependency library of core logic needed in each C# project

## Overview

* [Log](#Logging) - A minimalistic logging wrapper 
* [EventBus](#The-EventBus) - Publish and subscribe to global events from anywhere in your code
* [Injection Logic](#Injection-Logic) - A simple inversion of control pattern that does not rely on magic 

## Usage & Examples
See below for a full usage overview to explain the APIs with simple examples.

### Logging

```cs
Log.d("I'm a log message");
Log.w("I'm a warning");
Log.e("I'm an error");
Log.e(new Exception("I'm an exception"));
Log.w("I'm a warning with parmas:", "param 1", 2, "..");
AssertV2.IsTrue(1 + 1 == 3, "This assertion will fail");
```
AssertV2 can be used anywhere in your code, it will be automatically removed from your production code.

Additional Log-helpers to log when a method is entered and left:
```cs
private static void SomeExampleMethod1(string s, int i) {
    Stopwatch timing = Log.MethodEntered("s=" + s, "i=" + i);
    { // .. here would be some method logic ..
        Thread.Sleep(1);
    } // .. as the last line in the tracked method add:
    Log.MethodDone(timing, maxAllowedTimeInMs: 50);
    // If the method needed more then 50ms an error is logged
}
```

### The EventBus

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

__Rule of thumb__: Only use the EventBus pattern if you can't exactly tell who wants to listen to the published events. Do not use the eventbus to pass an event from x to y if you know exactly who x and y are! 

### Injection Logic
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

Another extended example usage can be found in InjectionTests.ExampleUsage2()

## Installation

Use the package manager [pip](https://pip.pypa.io/en/stable/) to install foobar.

```bash
pip install foobar
```


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Progress
See current features in development here: https://github.com/cs-util-com/cscore/projects/1

## License
[MIT License](https://choosealicense.com/licenses/mit/)


![](https://forthebadge.com/images/badges/built-with-love.svg)
<!--- // Other very important badges:
![](https://forthebadge.com/images/badges/does-not-contain-treenuts.svg)
![](https://forthebadge.com/images/badges/contains-cat-gifs.svg)
-->
