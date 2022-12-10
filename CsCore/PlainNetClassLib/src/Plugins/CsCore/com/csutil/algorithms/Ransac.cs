using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.netstandard2_1polyfill;

namespace com.csutil.algorithms {

    public static class Ransac {

        /// <summary> Returns a list of (randomly) sampled elements (outliers filtered out from this list). See https://en.wikipedia.org/wiki/Random_sample_consensus#Algorithm </summary>
        /// <param name="elems"> All elements </param>
        /// <param name="d"> Number of close data points required to assert that a model fits well to data </param>
        /// <param name="createModel"> Has to create a model based on the set of provided elements and calculate its total error </param>
        /// <param name="isInlier"> Will be called with an element and has to return true if the element is in the error margin of the current model </param>
        public static M RunRansac<E, M>(this Random rnd, IEnumerable<E> elems, int d, int minSampleSize, int iterations, Func<IEnumerable<E>, M> createModel, Func<M, E, bool> isInlier) where M : IModel<E> {
            if (minSampleSize > elems.Count()) {
                throw new ArgumentOutOfRangeException($"minSampleSize must be smaller then nr of elements, otherwise ransac would not make sense: minSampleSize={minSampleSize} and elems.Count()={elems.Count()}");
            }
            M bestModel = default(M);
            for (int i = 0; i < iterations; i++) {
                var maybeInliers = com.csutil.netstandard2_1polyfill.IEnumerableExtensions.ToHashSet(rnd.SampleElemsToGetRandomSubset(elems, minSampleSize));
                var model = createModel(maybeInliers); // Fit that sub-sample to the model
                var alsoInliers = new List<E>();
                var outliers = new List<E>();
                foreach (var elem in elems) {
                    if (!maybeInliers.Contains(elem)) {
                        if (isInlier(model, elem)) {
                            alsoInliers.Add(elem);
                        } else {
                            outliers.Add(elem);
                        }
                    }
                }
                if (bestModel == null || alsoInliers.Count >= d) {
                    alsoInliers.AddRange(maybeInliers); // Merge to include all inliers
                    M betterModel = createModel(alsoInliers);
                    if (betterModel.totalModelError == null) {
                        throw new ArgumentNullException("The createModel function did not calculate a totalModelError for the returned model");
                    }
                    if (bestModel == null || betterModel.totalModelError < bestModel.totalModelError) {
                        bestModel = betterModel;
                        bestModel.inliers = alsoInliers;
                        bestModel.outliers = outliers;
                    }
                }
            }
            return bestModel;
        }

        public interface IModel<E> {
            double? totalModelError { get; }
            ICollection<E> inliers { set; }
            ICollection<E> outliers { set; }
        }

    }

}