using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.http.apis.wikipedia {

    public static class Wikipedia {

        public static Task<SearchResult> Search(string topic) {
            var uri = new Uri($"https://en.wikipedia.org/w/api.php?action=query&list=search&format=json&srsearch={Uri.EscapeDataString(topic)}");
            return uri.SendGET().GetResult<SearchResult>();
        }

        public static Task<Article> GetArticle(int pageid) {
            var uri = new Uri($"https://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&exintro=&explaintext=&pageids={pageid}");
            return uri.SendGET().GetResult<Article>();
        }

        public class WikipediaPage {
            public int ns { get; set; }
            public string title { get; set; }
            public int pageid { get; set; }
        }

        public class SearchResult {
            public Query query { get; set; }

            public class Query {
                public Searchinfo searchinfo { get; set; }
                public Search[] search { get; set; }

                public class Searchinfo {
                    public int totalhits { get; set; }
                }

                public class Search : WikipediaPage {
                    public int size { get; set; }
                    public string snippet { get; set; }
                    public DateTime timestamp { get; set; }
                }
            }
        }

        public class Article {
            
            public Query query { get; set; }

            public class Query {
                
                public Dictionary<string, Page> pages { get; set; }

                public class Page : WikipediaPage {
                    public string extract { get; set; }
                }
                
            }
            
        }

    }

}