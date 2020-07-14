using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace com.csutil.http.apis {

    public static class GoogleSearchSuggestions {

        public static async Task<HashSet<string>> GetAlternativesRecursively(string searchTerm) {
            var t = Log.MethodEntered("Recursive alternatives for " + searchTerm);
            IEnumerable<string> alternativesToStarWars = await GetAlternatives(searchTerm);
            HashSet<string> all = new HashSet<string>();
            foreach (var a in alternativesToStarWars) { // Repeat on all direct alternatives:
                foreach (var r in await GetAlternatives(a)) { all.Add(r); }
            }
            Log.MethodDone(t);
            return all;
        }

        public static async Task<IEnumerable<string>> GetAlternatives(string searchTerm) {
            searchTerm += " vs ";
            searchTerm = searchTerm.ToLowerInvariant();
            Response s = await GetSearchSuggestions(searchTerm);
            IEnumerable<string> result = s.CompleteSuggestion.Map(x => x.Suggestion.Data);
            result = result.Filter(x => x.ToLowerInvariant().StartsWith(searchTerm));
            return result.Map(x => x.SubstringAfter(searchTerm).Trim());
        }

        // http://suggestqueries.google.com/complete/search?&output=toolbar&hl=en&q=starwars%20vs%20
        public static async Task<Response> GetSearchSuggestions(string search, string language = "en") {
            var q = $"&hl={language}&q={Uri.EscapeDataString(search)}";
            var suggestionsUrl = "http://suggestqueries.google.com/complete/search?&output=toolbar";
            RestRequest request = new Uri(suggestionsUrl + q).SendGET();
            return ParseXml<Response>(await request.GetResult<Stream>());
        }

        public static T ParseXml<T>(Stream stream) {
            using (var r = XmlReader.Create(stream)) { return (T)new XmlSerializer(typeof(T)).Deserialize(r); }
        }

        [XmlRoot("toplevel")]
        public class Response {[XmlElement("CompleteSuggestion")] public List<SearchSuggestion> CompleteSuggestion; }
        public class SearchSuggestion {[XmlElement("suggestion")] public SugData Suggestion; }
        public class SugData {[XmlAttribute("data")] public string Data; }

    }

}