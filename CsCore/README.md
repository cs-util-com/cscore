# cscore
cscore is a minimal zero dependency library of core logic needed in each C# project

## Overview

* [An EventBus](#EventBus) to publish and subscribe to global events
* [Injection](#Injection)

## Usage & Examples
See below for a full usage overview to explain the APIs with simple examples.

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

### EventBus
```cs
import foobar

foobar.pluralize('word') # returns 'words'
foobar.pluralize('goose') # returns 'geese'
foobar.singularize('phenomena') # returns 'phenomenon'
```

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
