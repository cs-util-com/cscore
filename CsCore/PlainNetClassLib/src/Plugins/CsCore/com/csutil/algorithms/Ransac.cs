using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.netstandard2_1polyfill;

namespace com.csutil.algorithms {

    public static class Ransac {

        /// <summary> Returns a list of (randomly) sampled elements (outliers filtered out from this list). See https://en.wikipedia.org/wiki/Random_sample_consensus#Algorithm </summary>
        /// <param name="elems"> All elements </param>
        /// <param name="createModel"> Has to create a model based on the set of provided elements and calculate its total error </param>
        /// <param name="isElemInlier"> Will be called with an element and has to return true if the element is in the error margin of the current model </param>
        public static Result<E> RunRansac<E>(this Random rnd, IEnumerable<E> elems, Func<IEnumerable<E>, double> createModel, Func<E, bool> isElemInlier, int iterations = 1000, int minSampleSize = 3) {
            Result<E> bestModel = new Result<E>();
            for (int i = 0; i < iterations; i++) {
                var maybeInliers = rnd.SampleElemsToGetRandomSubset(elems, minSampleSize).ToHashSet();
                var maybeError = createModel(maybeInliers); // Fit that sub-sample to the model
                if (maybeError < bestModel.error) { // Has potential to be better model then the best one
                    var allInliers = maybeInliers;
                    var outliers = new List<E>();
                    foreach (var elem in elems) {
                        if (!maybeInliers.Contains(elem)) {
                            if (isElemInlier(elem)) {
                                allInliers.Add(elem);
                            } else {
                                outliers.Add(elem);
                            }
                        }
                    }
                    if (allInliers.Count >= bestModel.inliers.Count()) { // Has more inliers then best model
                        var betterError = createModel(allInliers);
                        if (betterError < bestModel.error) { // Has better error then best model so far
                            bestModel = new Result<E> {
                                inliers = allInliers,
                                error = betterError,
                                outliers = outliers
                            };
                        }
                    }
                }
            }
            return bestModel;
        }

        public class Result<E> {
            public HashSet<E> inliers = new HashSet<E>();
            public double error = double.MaxValue;
            public List<E> outliers;
        }

    }

}