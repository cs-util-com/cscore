/**
	Originally created by krisk for Fuse.js
	https://github.com/krisk/Fuse

	Ported to C# by kurozael
	https://github.com/kurozael/Fuse.NET
	
	LICENSE: Apache License 2.0
**/

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace Fuse.NET
{
    public class SearchOpts
    {
        public int limit;
    }
    
    public struct FuseMatch
    {
        public List<List<int>> indicies;
        public string value;
        public string key;
        public int arrayIndex;
    }
    
    public struct FuseResult<T>
    {
        public T item;
        public float score;
        public List<FuseMatch> matches;
    }
    
    public struct SearchKey
    {
        public string name;
        public float weight;
    }

    public delegate int SortFunction(float a, float b);
    public delegate object GetFunction(object source, string path);

    public class FuseOptions
    {
        public string id = null;
        public bool caseSensitive = false;
        public bool includeMatches = false;
        public bool includeScore = false;
        public bool shouldSort = true;
        public SortFunction sortFn = (a, b) => a.CompareTo(b);
        public GetFunction getFn;
        public List<SearchKey> keys = new List<SearchKey>();
        public bool verbose = false;
        public bool tokenize = false;
        public Regex tokenSeparator = new Regex(" +");
        public bool matchAllTokens = false;
        public int location = 0;
        public int distance = 100;
        public float threshold = 0.6f;
        public int maxPatternLength = 32;
        public int minMatchCharLength = 1;
        public bool findAllMatches = false;
    }

    public class Fuse<T>
    {
        internal delegate void TransformResult(AnalyzeResult result, ref TransformData data);

        internal class Searchers
        {
            public List<Bitap<T>> tokenSearchers;
            public Bitap<T> fullSearcher;
        }

        private FuseOptions _options;
        private List<T> _list;

        public Fuse(List<T> list, FuseOptions options = null)
        {
            _options = options != null ? options : new FuseOptions();

            if (_options.getFn == null)
            {
                _options.getFn = DeepValue;
            }

            SetCollection(list);
        }

        public List<T> SetCollection(List<T> list)
        {
            _list = list;
            return list;
        }

        public void AddKey(string name, float weight = 1f)
        {
            _options.keys.Add(new SearchKey
            {
                name = name,
                weight = weight
            });
        }

        public void ClearKeys()
        {
            _options.keys.Clear();
        }

        public List<FuseResult<T>> Search(string pattern, SearchOpts opts = null)
        {
            var searchers = PrepareSearchers(pattern);
            var search = InternalSearch(searchers.tokenSearchers, searchers.fullSearcher);

            ComputeScore(search);

            if (_options.shouldSort)
            {
                SortSearchResults(search.results);
            }

            if (opts != null)
            {
                search.results = search.results.GetRange(0, opts.limit);
            }

            return Format(search.results);
        }

        private List<FuseResult<T>> Format(List<AnalyzeResult> results)
        {
            var finalOutput = new List<FuseResult<T>>();
            var transformers = new List<TransformResult>();

            if (_options.includeMatches)
            {
                transformers.Add((AnalyzeResult result, ref TransformData data) => {
                    var output = result.output;
                    data.matches = new List<FuseMatch>();

                    for (var i = 0; i < output.Count; i++)
                    {
                        var item = output[i];

                        if (item.matchedIndices.Count == 0)
                        {
                            continue;
                        }

                        var entry = new FuseMatch
                        {
                            indicies = item.matchedIndices,
                            value = item.value
                        };

                        if (!string.IsNullOrEmpty(item.key))
                        {
                            entry.key = item.key;
                        }

                        if (item.arrayIndex > -1)
                        {
                            entry.arrayIndex = item.arrayIndex;
                        }

                        data.matches.Add(entry);
                    }
                });
            }

            if (_options.includeScore) {
                transformers.Add((AnalyzeResult result, ref TransformData data) =>
                {
                    data.score = result.score;
                });
            }

            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (!string.IsNullOrEmpty(_options.id))
                {
                    var value = _options.getFn(result.item, _options.id);

                    if (value is List<string>)
                    {
                        result.item = ((List<string>)value)[0];
                    }
                    else if (value is string)
                    {
                        result.item = value;
                    }
                }

                if (transformers.Count == 0)
                {
                    finalOutput.Add(new FuseResult<T>
                    {
                        item = (T)result.item
                    });

                    continue;
                }

                var data = new TransformData {
                    item = result.item
                };

                for (var j = 0; j < transformers.Count; j++)
                {
                    transformers[j](result, ref data);
                }

                finalOutput.Add(new FuseResult<T>
                {
                    item = (T)data.item,
                    score = data.score,
                    matches = data.matches
                });
            }

            return finalOutput;
        }

        private void ComputeScore(SearchResult search)
        {
            for (var i = 0; i < search.results.Count; i++)
            {
                var output = search.results[i].output;
                var scoreLen = output.Count;

                var bestScore = 1f;
                var curScore = 1f;
        
                for (var j = 0; j < scoreLen; j++)
                {
                    var weight = search.weights != null ? search.weights[output[j].key] : 1f;
                    var score = weight == 1f ? output[j].score : (output[j].score > 0f ? output[j].score : 0.001f);
                    var nScore = score * weight;

                    if (weight != 1f)
                    {
                        bestScore = Math.Min(bestScore, nScore);
                    }
                    else
                    {
                        output[j].nScore = nScore;
                        curScore *= nScore;
                    }
                }

                search.results[i].score = bestScore == 1f ? curScore : bestScore;
            }
        }

        internal struct TransformData
        {
            public object item;
            public float score;
            public List<FuseMatch> matches;
        }

        internal class SearchResult
        {
            public Dictionary<string, float> weights;
            public List<AnalyzeResult> results;
        }

        private SearchResult InternalSearch(List<Bitap<T>> tokenSearchers, Bitap<T> fullSearcher)
        {
            var resultMap = new Dictionary<int, AnalyzeResult>();
            var results = new List<AnalyzeResult>();
            var output = new AnalyzeOutput
            {
                resultMap = resultMap,
                results = results,
                tokenSearchers = tokenSearchers,
                fullSearcher = fullSearcher
            };

            if (_list[0] is string)
            {
                for (var i = 0; i < _list.Count; i++)
                {
                    Analyze(new AnalyzeOpts
                    {
                        key = "",
                        value = (_list[i] as string),
                        record = i,
                        index = i
                    }, output);
                }

                return new SearchResult
                {
                    weights = null,
                    results = results
                };
            }

            var weights = new Dictionary<string, float>();

            for (var i = 0; i < _list.Count; i++)
            {
                var item = _list[i];

                for (var j = 0; j < _options.keys.Count; j++)
                {
                    var key = _options.keys[j];
                    var weight = (1f - key.weight);

                    if (weight == 0f)
                    {
                        weight = 1f;
                    }

                    weights[key.name] = weight;

                    Analyze(new AnalyzeOpts
                    {
                        key = key.name,
                        value = _options.getFn(item, key.name),
                        record = item,
                        index = i
                    }, output);
                }
            }

            return new SearchResult
            {
                weights = weights,
                results = results
            };
        }

        private void SortSearchResults(List<AnalyzeResult> matches)
        {
            matches.Sort((a, b) =>
            {
                return _options.sortFn(a.score, b.score);
            });
        }

        private Searchers PrepareSearchers(string pattern)
        {
            var tokenSearchers = new List<Bitap<T>>();

            if (_options.tokenize)
            {
                var tokens = _options.tokenSeparator.Split(pattern);

                for (var i = 0; i < tokens.Length; i++)
                {
                    tokenSearchers.Add(new Bitap<T>(tokens[i], _options));
                }
            }

            var fullSearcher = new Bitap<T>(pattern, _options);

            return new Searchers
            {
                tokenSearchers = tokenSearchers,
                fullSearcher = fullSearcher
            };
        }

        internal class AnalyzeOpts
        {
            public string key;
            public object record;
            public int arrayIndex;
            public object value;
            public int index;
        }

        internal class AnalyzeOutput
        {
            public List<Bitap<T>> tokenSearchers;
            public Bitap<T> fullSearcher;
            public Dictionary<int, AnalyzeResult> resultMap;
            public List<AnalyzeResult> results;
        }

        internal class AnalyzeMatch
        {
            public float nScore;
            public string key;
            public int arrayIndex;
            public string value;
            public float score;
            public List<List<int>> matchedIndices;
        }

        internal class AnalyzeResult
        {
            public object item;
            public float score;
            public List<AnalyzeMatch> output;
        }

        private void Analyze(AnalyzeOpts opts, AnalyzeOutput output)
        {
            if (opts.value == null)
            {
                return;
            }

            var exists = false;
            var averageScore = -1f;
            var numTextMatches = 0;

            if (opts.value is string)
            {
                var mainSearchResult = output.fullSearcher.Search((string)opts.value);

                if (_options.tokenize)
                {
                    var words = _options.tokenSeparator.Split((string)opts.value);
                    var scores = new List<float>();

                    for (var i = 0; i < output.tokenSearchers.Count; i++)
                    {
                        var tokenSearcher = output.tokenSearchers[i];
                        var hasMatchInText = false;

                        for (var j = 0; j < words.Length; j++)
                        {
                            var word = words[j];
                            var tokenSearchResult = tokenSearcher.Search(word);

                            if (tokenSearchResult.isMatch)
                            {
                                exists = true;
                                hasMatchInText = true;

                                scores.Add(tokenSearchResult.score);
                            }
                            else
                            {
                                if (!_options.matchAllTokens)
                                {
                                    scores.Add(1f);
                                }
                            }
                        }

                        if (hasMatchInText)
                        {
                            numTextMatches += 1;
                        }
                    }

                    averageScore = scores[0];
                    var scoresLen = scores.Count;

                    for (var i = 1; i < scoresLen; i++)
                    {
                        averageScore += scores[i];
                    }

                    averageScore = averageScore / scoresLen;
                }

                var finalScore = mainSearchResult.score;

                if (averageScore > -1f)
                {
                    finalScore = (finalScore + averageScore) / 2f;
                }

                var checkTextMatches = (_options.tokenize && _options.matchAllTokens) ? numTextMatches >= output.tokenSearchers.Count : true;

                if ((exists || mainSearchResult.isMatch) && checkTextMatches)
                {
                    if (output.resultMap.ContainsKey(opts.index))
                    {
                        output.resultMap[opts.index].output.Add(new AnalyzeMatch
                        {
                            key = opts.key,
                            arrayIndex = opts.arrayIndex,
                            value = (string)opts.value,
                            score = finalScore,
                            matchedIndices = mainSearchResult.matchedIndices
                        });
                    }
                    else
                    {
                        output.resultMap[opts.index] = new AnalyzeResult
                        {
                            item = opts.record,
                            output = new List<AnalyzeMatch>
                            {
                                new AnalyzeMatch
                                {
                                    key = opts.key,
                                    arrayIndex = opts.arrayIndex,
                                    value = (string)opts.value,
                                    score = finalScore,
                                    matchedIndices = mainSearchResult.matchedIndices
                                }
                            }
                        };

                        output.results.Add(output.resultMap[opts.index]);
                    }
                }
            }
            else if (opts.value is List<string>)
            {
                var list = (List<string>)opts.value;

                for (var i = 0; i < list.Count; i++)
                {
                    Analyze(new AnalyzeOpts
                    {
                        key = opts.key,
                        arrayIndex = i,
                        value = list[i],
                        record = opts.record,
                        index = opts.index
                    }, output);
		        }
            }
	    }

        private object DeepValue(object source, string path)
        {
            if (path.Contains("."))
            {
                var temp = path.Split(new char[] { '.' }, 2);
                return DeepValue(DeepValue(source, temp[0]), temp[1]);
            }
            else
            {
                var prop = source.GetType().GetField(path);
                return prop?.GetValue(source);
            }
        }
    }
}
