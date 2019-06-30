using System;
using System.Collections.Generic;

namespace com.csutil {

    public delegate Reducer<IN> Transducer<IN, OUT>(Reducer<OUT> reducer);

    public delegate object Reducer<IN>(object accumulator, IN elem);

    /// <summary> 
    /// A transducer takes a Reducer and transforms it, so: transducer = Reducer -> Reducer
    /// Related sources:
    /// - https://jrsinclair.com/articles/2019/magical-mystical-js-transducers/
    /// - https://medium.com/javascript-scene/transducers-efficient-data-processing-pipelines-in-javascript-7985330fe73d
    /// - http://raganwald.com/2017/04/30/transducers.html
    /// </summary> 
    public static class Transducers {

        /// <summary> 
        /// Make a function that takes a reducer and returns a new reducer that 
        /// filters out some items so that the original reducer never sees them.
        /// </summary> 
        public static Transducer<IN, IN> NewFilter<IN>(Func<IN, bool> filter) {
            return (nextReducer) => (accumulator, elem) => filter(elem) ? nextReducer(accumulator, elem) : accumulator;
        }

        /// <summary> 
        /// Make a function that takes a reducer and returns a new reducer that 
        /// transforms every time before the original reducer gets to see it.
        /// </summary> 
        public static Transducer<IN, OUT> NewMapper<IN, OUT>(Func<IN, OUT> mapper) {
            return (nextReducer) => (accumulator, elem) => nextReducer(accumulator, mapper(elem));
        }

        public static OUT Reduce<IN, OUT>(this Reducer<IN> self, OUT seed, IEnumerable<IN> elements) {
            return elements.Reduce(seed, (accumulator, elem) => {
                var result = self(accumulator, elem);
                if (result is OUT) { return (OUT)result; }
                throw Log.e("The reducer has to return an result of type " + typeof(OUT));
            });
        }

        public static Transducer<T, T> Compose<T>(params Transducer<T, T>[] transducers) {
            return r => { for (int i = transducers.Length - 1; i >= 0; i--) { r = transducers[i](r); } return r; };
        }

        public static List<T> FilterToList<T>(this IEnumerable<T> elements, Transducer<T, T> transducer) { return elements.MapToList(transducer); }


        public static List<OUT> MapToList<IN, OUT>(this IEnumerable<IN> elements, Transducer<IN, OUT> transducer) {
            Reducer<IN> reducer = transducer((accumulator, elem) => {
                (accumulator as List<OUT>).Add(elem); return accumulator;
            });
            return reducer.Reduce(new List<OUT>(), elements);
        }

        public static OUT ReduceTo<IN, OUT>(this IEnumerable<IN> elements, Transducer<IN, OUT> transducer, Func<OUT, OUT, OUT> reduce, OUT seed) {
            return transducer((accumulator, elem) => reduce((OUT)accumulator, elem)).Reduce(seed, elements);
        }

    }

}