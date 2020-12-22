using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class StackOverflowCom {

        public static Task RethrowWithAnswers(this Exception e) {
            return RethrowWithAnswers(e, new List<string>() { "C#" });
        }

        private static async Task RethrowWithAnswers(Exception e, List<string> tags) {
            if (!EnvironmentV2.isDebugMode) { throw e; }
            throw Log.e(await CheckError(e.Message, tags, maxResults: 3), e);
        }

        public static async Task<string> CheckError(string fullErrorString, List<string> tags, int maxResults = 3) {
            return await Ask(ExtractRelevantWords(fullErrorString), tags, maxResults);
        }

        public static async Task<string> Ask(string question, List<string> tags, int maxResults = 3) {
            var bestAnswers = await GetBestAnswersFor(question, tags, maxResults);
            if (bestAnswers.IsEmpty()) { throw new Error("No answers found for " + question); }
            var answerStrings = bestAnswers.Map(a => {
                var answerHtmlAsText = RestRequestHelper.HtmlToPlainText(a.Value.body);
                var questi = $"?? Question ({a.Key.score} votes):  {WebUtility.HtmlDecode(a.Key.title)}";
                var answer = $">> Answer {a.Value.answer_url} ({a.Value.score} votes): \n{answerHtmlAsText}";
                return "\n" + questi + ":\n" + answer + "\n";
            });
            var searchSentence = question + " " + tags.ToStringV2("[", "]", "] [");
            var s = "https://stackoverflow.com/search?q=" + WebUtility.UrlEncode(searchSentence);
            return $"\n{question}\nSearching.. {s}\n{answerStrings.ToStringV2("", "", "")}";
        }

        public static string ExtractRelevantWords(string question, int minWordLength = 1, int maxWordCount = 15) {
            var allWords = question.SplitViaRegex(regex: @"[^\W\d](\w|[-']{1,2}(?=\w))*");
            var filtered = allWords.Filter(w => w.Length > minWordLength); // Filter out short words
            filtered = filtered.Take(maxWordCount); // From all words take only the first x based on maxWordCount
            var result = filtered.Reduce((res, x) => res + " " + x);
            return result;
        }

        public static async Task<Dictionary<Item, Item>> GetBestAnswersFor(string question, List<string> tags, int maxResults = 3) {
            ApiResponse bestQuestions = await Search(question, tags);
            var questionsWithAnswers = bestQuestions.items.Filter(x => x.answer_count > 0 && x.score > 0);
            Dictionary<Item, Item> bestAnswers = new Dictionary<Item, Item>();
            foreach (Item q in questionsWithAnswers) {
                var answers = await GetAnswersForQuestion(q.question_id, pageSize: 1);
                AssertV2.AreEqual(1, answers.items.Count());
                bestAnswers.Add(q, answers.items.First());
                if (bestAnswers.Count >= maxResults) { break; }
            }
            return bestAnswers;
        }

        /// <summary> See https://api.stackexchange.com/docs/answers-on-questions for documentation </summary>
        public static async Task<ApiResponse> GetAnswersForQuestion(int questionId, int pageSize = 2) {
            var uri = new Uri($"https://api.stackexchange.com/2.2/questions/{questionId}/answers?pagesize={pageSize}&order=desc&sort=votes&site=stackoverflow&filter=withbody");
            return (await uri.SendGET().GetResult<ApiResponse>()).ThrowIfError();
        }

        /// <summary> See https://api.stackexchange.com/docs/advanced-search for documentation </summary>
        public static async Task<ApiResponse> Search(string question, List<string> tags) {
            var q = WebUtility.UrlEncode(question);
            string url = $"https://api.stackexchange.com/2.2/search/advanced?order=desc&sort=relevance&q={q}&site=stackoverflow";
            if (!tags.IsNullOrEmpty()) {
                url += "&tagged=" + WebUtility.UrlEncode(tags.ToStringV2("", "", ";"));
            }
            ApiResponse apiResponse;
            try {
                apiResponse = await new Uri(url).SendGET().GetResult<ApiResponse>();
            }
            catch (Exception e) {
                throw new Error("Failed to Get questions from " + url, e);
            }
            return apiResponse.ThrowIfError();
        }

        public class ApiResponse {
            public List<Item> items { get; set; }
            public bool has_more { get; set; }
            public int quota_max { get; set; }
            public int quota_remaining { get; set; }
            public string error_id { get; set; }
            public string error_message { get; set; }
            public string error_name { get; set; }
            public int backoff { get; set; }

            public ApiResponse ThrowIfError() {
                if (!error_id.IsNullOrEmpty()) { throw new Error($"{error_id} - {error_name}: {error_message}"); }
                return this;
            }
        }

        public class Owner {
            public int reputation { get; set; }
            public int user_id { get; set; }
            public string user_type { get; set; }
            public string profile_image { get; set; }
            public string display_name { get; set; }
            public string link { get; set; }
            public int? accept_rate { get; set; }
        }

        /// <summary> Can be a question or an answer </summary>
        public class Item {
            public List<string> tags { get; set; }
            public Owner owner { get; set; }
            public bool is_answered { get; set; }
            public int view_count { get; set; }
            public int answer_count { get; set; }
            public int score { get; set; }
            public int last_activity_date { get; set; }
            public int creation_date { get; set; }
            public int question_id { get; set; }
            public string content_license { get; set; }
            public string link { get; set; }
            public string title { get; set; }
            public int? last_edit_date { get; set; }
            public int? accepted_answer_id { get; set; }

            public bool is_accepted { get; set; }
            public int answer_id { get; set; }
            public string answer_url => "https://stackoverflow.com/a/" + answer_id;

            // If "&filter=withbody" is included in the query, the body of the item will be included
            public string body { get; set; }

            public override string ToString() {
                if (!body.IsNullOrEmpty()) { return body; }
                if (!title.IsNullOrEmpty()) { return title; }
                return base.ToString();
            }
        }

    }

}