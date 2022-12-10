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
        public static M RunRansac<E, M>(this Random rnd, IEnumerable<E> elems, int d, Func<IEnumerable<E>, M> createModel, Func<M, E, bool> isInlier, int iterations = 1000, int minSampleSize = 3) where M : IModel<E> {
            if (minSampleSize >= elems.Count()) {
                throw new ArgumentOutOfRangeException($"minSampleSize must be smaller then nr of elements, otherwise ransac would not make sense: minSampleSize={minSampleSize} and elems.Count()={elems.Count()}");
            }
            AssertV3.IsTrue(iterations > 100, () => "It is recommended to have >100 iterations for ransac, iterations=" + iterations);
            M bestModel = default(M);
            for (int i = 0; i < iterations; i++) {
                var maybeInliers = rnd.SampleElemsToGetRandomSubset(elems, minSampleSize).ToHashSet();
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
                if (alsoInliers.Count >= d) {
                    var allInliers = maybeInliers.Union(alsoInliers);
                    M betterModel = createModel(allInliers);
                    betterModel.totalModelError.ThrowErrorIfNull("totalModelError");
                    if (bestModel == null || betterModel.totalModelError < bestModel.totalModelError) {
                        bestModel = betterModel;
                        bestModel.inliers = allInliers;
                        bestModel.outliers = outliers;
                    }
                }
            }
            return bestModel;
        }

        public interface IModel<E> {
            double? totalModelError { get; }
            IEnumerable<E> inliers { set; }
            List<E> outliers { set; }
        }

    }

}