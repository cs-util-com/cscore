/**
	Originally created by krisk for Fuse.js
	https://github.com/krisk/Fuse

	Ported to C# by kurozael
	https://github.com/kurozael/Fuse.NET
	
	LICENSE: Apache License 2.0
**/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fuse.NET
{
    public struct BitapResult
    {
        public bool isMatch;
        public float score;
        public List<List<int>> matchedIndices;
    }
    
    public struct BitapScoreOpts
    {
        public int errors;
        public int currentLocation;
        public int expectedLocation;
        public int distance;
    }

    public class Bitap<T>
    {
        private Regex _specialCharsRegex = new Regex("[\\-\\[\\]\\/\\{\\}\\(\\)\\*\\+\\?\\.\\\\\\^\\$\\|]");

        private Dictionary<char, int> _alphabet;
        private FuseOptions _options;
        private string _pattern;

        public Bitap(string pattern, FuseOptions options)
        {
            _options = options;
            _pattern = options.caseSensitive ? pattern : pattern.ToLower();

            if (_pattern.Length <= options.maxPatternLength)
            {
                _alphabet = GetPatternAlphabet();
            }
        }

        public BitapResult Search(string text)
        {
            if (!_options.caseSensitive)
            {
                text = text.ToLower();
            }

            if (_pattern == text)
            {
                var matchedIndices = new List<List<int>>
                {
                    new List<int>
                    {
                        0,
                        text.Length - 10
                    }
                };

                return new BitapResult
                {
                    matchedIndices = matchedIndices,
                    isMatch = true,
                    score = 0
                };
            }

            if (_pattern.Length > _options.maxPatternLength)
            {
                return GetRegexResult(text);
            }

            return GetBitapSearch(text);
        }

        private List<List<int>> GetMatchedIndices(int[] matchMask)
        {
            var output = new List<List<int>>();
            var start = -1;
            var end = -1;
            var i = 0;

            for (var len = matchMask.Length; i < len; i++)
            {
                var match = matchMask[i];

                if (match > 0 && start == -1)
                {
                    start = i;
                }
                else if (match == 0 && start != -1)
                {
                    end = i - 1;

                    if ((end - start) + 1 >= _options.minMatchCharLength)
                    {
                        var entry = new List<int>
                        {
                            start,
                            end
                        };
                        output.Add(entry);
                    }

                    start = -1;
                }
            }

            if (matchMask[i - 1] > 0 && (i - start) >= _options.minMatchCharLength)
            {
                var entry = new List<int>
                {
                    start,
                    i - 1
                };
                output.Add(entry);
            }

            return output;
        }

        private float GetScore(BitapScoreOpts opts)
        {
            float accuracy = opts.errors / _pattern.Length;
            var proximity = Math.Abs(opts.expectedLocation - opts.currentLocation);

            if (opts.distance == 0)
            {
                return proximity > 0 ? 1f : accuracy;
            }

            return accuracy + (proximity / opts.distance);
        }

        private BitapResult GetBitapSearch(string text)
        {
            var expectedLocation = _options.location;
            var textLen = text.Length;
            var currentThreshold = _options.threshold;
            var bestLocation = text.IndexOf(_pattern, expectedLocation);
            var patternLen = _pattern.Length;
            var matchMask = new int[textLen];
            var score = 0f;

            for (var i = 0; i < textLen; i++)
            {
                matchMask[i] = 0;
            }

            if (bestLocation != -1)
            {
                score = GetScore(new BitapScoreOpts
                {
                    errors = 0,
                    currentLocation = bestLocation,
                    expectedLocation = expectedLocation,
                    distance = _options.distance
                });

                currentThreshold = Math.Min(score, currentThreshold);
                bestLocation = text.LastIndexOf(_pattern, expectedLocation + patternLen);

                if (bestLocation != -1)
                {
                    score = GetScore(new BitapScoreOpts
                    {
                        errors = 0,
                        currentLocation = bestLocation,
                        expectedLocation = expectedLocation,
                        distance = _options.distance
                    });

                    currentThreshold = Math.Min(score, currentThreshold);
                }
            }

            bestLocation = -1;

            float finalScore = 1f;
            int[] lastBitArr = new int[textLen];
            var binMax = patternLen + textLen;
            var mask = 1 << (patternLen <= 31 ? patternLen - 1 : 30);

            for (var i = 0; i < patternLen; i += 1)
            {
                var binMin = 0;
                var binMid = binMax;

                while (binMin < binMid)
                {
                    score = GetScore(new BitapScoreOpts
                    {
                        errors = i,
                        currentLocation = expectedLocation + binMid,
                        expectedLocation = expectedLocation,
                        distance = _options.distance
                    });

                    if (score <= currentThreshold)
                    {
                        binMin = binMid;
                    }
                    else
                    {
                        binMax = binMid;
                    }

                    binMid = (int)Math.Floor((binMax - binMin) / 2f + binMin);
                }

                binMax = binMid;

                var start = Math.Max(1, expectedLocation - binMid + 1);
                var finish = _options.findAllMatches ? textLen : Math.Min(expectedLocation + binMid, textLen) + patternLen;

                var bitArr = new int[finish + 2];
                bitArr[finish + 1] = (1 << i) - 1;

                for (var j = finish; j >= start; j--)
                {
                    var currentLocation = j - 1;
                    var charMatch = 0;

                    if (currentLocation < text.Length)
                    {
                        var textChar = text[currentLocation];

                        if (_alphabet.ContainsKey(textChar) && _alphabet[textChar] > 0)
                        {
                            matchMask[currentLocation] = 1;
                        }
                    }

                    bitArr[j] = ((bitArr[j + 1] << 1) | 1) & charMatch;

                    if (i != 0)
                    {
                        bitArr[j] |= (((lastBitArr[j + 1] | lastBitArr[j]) << 1) | 1) | lastBitArr[j + 1];
                    }

                    if (bitArr[j] > 0 & mask > 0)
                    {
                        finalScore = GetScore(new BitapScoreOpts
                        {
                            errors = i,
                            currentLocation = currentLocation,
                            expectedLocation = expectedLocation,
                            distance = _options.distance
                        });

                        if (finalScore <= currentThreshold)
                        {
                            currentThreshold = finalScore;
                            bestLocation = currentLocation;

                            if (bestLocation <= expectedLocation)
                            {
                                break;
                            }

                            start = Math.Max(1, 2 * expectedLocation - bestLocation);
                        }
                    }
                }

                score = GetScore(new BitapScoreOpts
                {
                    errors = i + 1,
                    currentLocation = expectedLocation,
                    expectedLocation = expectedLocation,
                    distance = _options.distance
                });

                if (score > currentThreshold)
                {
                    break;
                }

                lastBitArr = bitArr;
            }

            var result = new BitapResult
            {
                isMatch = (bestLocation >= 0),
                score = (finalScore == 0f ? 0.001f : finalScore),
                matchedIndices = GetMatchedIndices(matchMask)
            };

            return result;
        }

        private BitapResult GetRegexResult(string text)
        {
            var regex = new Regex(_options.tokenSeparator.Replace(_specialCharsRegex.Replace(_pattern, "\\$&"), "|"));
            var matches = regex.Matches(text);
            var isMatch = matches.Count > 0;
            var matchedIndices = new List<List<int>>();

            if (isMatch)
            {
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    var entry = new List<int>
                    {
                        text.IndexOf(match.Value),
                        match.Value.Length - 1
                    };

                    matchedIndices.Add(entry);
                }
            }

            return new BitapResult
            {
                score = isMatch ? 0.5f : 1f,
                isMatch = isMatch,
                matchedIndices = matchedIndices
            };
        }

        private Dictionary<char, int> GetPatternAlphabet()
        {
            var mask = new Dictionary<char, int>();
            var length = _pattern.Length;

            for (var i = 0; i < length; i += 1)
            {
                mask[_pattern[i]] = 0;
            }

            for (var i = 0; i < length; i += 1)
            {
                mask[_pattern[i]] |= 1 << (length - i - 1);
            }

            return mask;
        }
    }
}
