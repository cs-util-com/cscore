Sharper Scroller - Virtualized scroller for Unity
===============================

A simple virtualized scroller for Unity!

Features
--------

- Horizontal and vertical scrolling
- Loopable (optional)
- Reverse order
- Zoom centered item
- Jump to index
- Scroll to index

Known Issues
--------

- The intertia isn't so good on small looping lists

Installation
------------

Copy the Scroller folder to your project

Usage
------------

1. Create a data and prefab classes by inheriting IScrollerData and IScrollerPrefab<TData>
2. Create a scroller class by inheriting IScroller<TData,TPrefab> use your data and prefab classes here
3. Create the prefab that will populate the scroller, with your scroller prefab class attached
4. Create the scroller and link your prefab

Check out ExampleScrollerScene for reference. It's pretty simple to use, strangely difficult to explain.

Contributors
------------

I hope to keep improving this plugin. Although I intend to keep the feature set slim, suggestions for improvements/optimizations are very welcome!

- [Shane Harper](http://shaneharper.uk/) - Creator

License
-------

Licensed under the MIT. See [LICENSE] file for full license information.  

[LICENSE]: LICENSE