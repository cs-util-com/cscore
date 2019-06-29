using System;
using System.Collections.Generic;

namespace com.csutil {

    public delegate A Reducer<B, A>(A x, B t);

    /// <summary> 
    /// A transducer takes a Reducer and transforms it, so: transducer = Reducer -> Reducer
    /// Related sources:
    /// - https://medium.com/javascript-scene/transducers-efficient-data-processing-pipelines-in-javascript-7985330fe73d
    /// - http://raganwald.com/2017/04/30/transducers.html
    /// - https://jrsinclair.com/articles/2019/magical-mystical-js-transducers/
    /// </summary> 
    public static class Transducers {

        /// <summary> 
        /// Make a function that takes a reducer and returns a new reducer that 
        /// filters out some items so that the original reducer never sees them.
        /// </summary> 
        public static Func<Reducer<IN, _>, Reducer<IN, _>> NewFilter<IN, _>(Func<IN, bool> filter) {
            return (nextReducer) => {
                return (accumulator, currentItem) => {
                    return filter(currentItem) ? nextReducer(accumulator, currentItem) : accumulator;
                };
            };
        }

        /// <summary> 
        /// Make a function that takes a reducer and returns a new reducer that 
        /// transforms every time before the original reducer gets to see it.
        /// </summary> 
        public static Func<Reducer<OUT, _>, Reducer<IN, _>> NewMapper<IN, OUT, _>(Func<IN, OUT> mapper) {
            return (nextReducer) => {
                return (accumulator, currentItem) => {
                    return nextReducer(accumulator, mapper(currentItem));
                };
            };
        }

        public static OUT Reduce<IN, OUT>(this Reducer<IN, OUT> self, OUT seed, IEnumerable<IN> elements) {
            return elements.Reduce(seed, (result, elem) => self(result, elem));
        }

        public static Reducer<_, List<IN>> NewMapperToList<IN, _>(this Func<Reducer<IN, List<IN>>, Reducer<_, List<IN>>> self) {
            return self((all, x) => { all.Add(x); return all; });
        }

    }

}