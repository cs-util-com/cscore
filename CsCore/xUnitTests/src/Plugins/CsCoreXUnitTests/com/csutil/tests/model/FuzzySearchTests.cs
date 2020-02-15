using System.Collections.Generic;
using Fuse.NET;
using Xunit;

namespace com.csutil.tests.model {

    public class FuzzySearchTests {

        public FuzzySearchTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        public struct Book {
            public string title;
            public string author;
        }

        [Fact]
        public void ExampleUsage1() {
            // Example copied from the docu of https://github.com/kurozael/Fuse.NET
            var input = new List<Book>();
            input.Add(new Book { title = "The Code of The Wooster", author = "Bob James" });
            input.Add(new Book { title = "The Wooster Code", author = "Rick Martin" });
            input.Add(new Book { title = "The Code", author = "Jimmy Charles" });
            input.Add(new Book { title = "Old Man's War", author = "John Scalzi" });
            input.Add(new Book { title = "The Lock Artist", author = "Steve Hamilton" });

            var opt = new FuseOptions();
            opt.includeMatches = true;
            opt.includeScore = true;
            // Here we search through a list of `Book` types but you could search through just a list of strings.
            var fuse = new Fuse<Book>(input, opt);
            fuse.AddKey("title");
            fuse.AddKey("author");
            var searchResult = fuse.Search("woo");

            searchResult.ForEach((FuseResult<Book> res) => {
                Log.d(res.item.title + ": " + res.item.author);
                Log.d("Search Result Score: " + res.score);

                if (res.matches != null) {
                    res.matches.ForEach((b) => {
                        Log.d("{Match}");
                        Log.d(b.key + ": " + b.value + " (Indicies: " + b.indicies.Count + ")");
                    });
                }
            });
        }

    }

}