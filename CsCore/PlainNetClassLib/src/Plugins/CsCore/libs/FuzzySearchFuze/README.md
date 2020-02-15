<p align="center">
<img src="https://github.com/kurozael/Fuse.NET/blob/master/logo.png?raw=true"/>
</p>

## Introduction

A lightweight zero-dependency C# port of the Fuse.js fuzzy-search library created by [@krisk](https://github.com/krisk) at https://github.com/krisk/Fuse

**I ported this to C# because I couldn't find anything like it, and because I love Fuse.js.**

## Licenses

You can find the original license in the `LICENSE.ORIGINAL` file, and the license for this derivative work in the `LICENSE.DERIVATIVE` file.

## Documentation

At this time there is no documentation for Fuse.NET, but as it is a direct port of Fuse.js, you can find all the information you need on the Fuse.js website at https://fusejs.io/

## Example Usage

```csharp
public struct Book
{
	public string title;
	public string author;
}

// This test data was taken from the fixtures in Fuse.js.
var input = new List<Book>();

input.Add(new Book
{
	title = "The Code of The Wooster",
	author = "Bob James"
});

input.Add(new Book
{
	title = "The Wooster Code",
	author = "Rick Martin"
});

input.Add(new Book
{
	title = "The Code",
	author = "Jimmy Charles"
});

input.Add(new Book
{
	title = "Old Man's War",
	author = "John Scalzi"
});

input.Add(new Book
{
	title = "The Lock Artist",
	author = "Steve Hamilton"
});

var opt = new FuseOptions();

opt.includeMatches = true;
opt.includeScore = true;

// Here we search through a list of `Book` types but you could search through just a list of strings.
var fuse = new Fuse<Book>(input, opt);

fuse.AddKey("title");
fuse.AddKey("author");

var output = fuse.Search("woo");

output.ForEach((a) =>
{
	Debug.Log(a.item.title + ": " + a.item.author);
	Debug.Log("Score: " + a.score);

	if (a.matches != null)
	{
		a.matches.ForEach((b) =>
		{
			Debug.Log("{Match}");
			Debug.Log(b.key + ": " + b.value + " (Indicies: " + b.indicies.Count + ")");
		});
	}
});
```

## Installation

Drag and drop the Fuse.NET folder directly into your C# project.

## Contributing

Contributions are welcome, if you have one to make please don't hestitate to create a pull request.
