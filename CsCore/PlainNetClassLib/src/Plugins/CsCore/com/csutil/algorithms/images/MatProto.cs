using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace com.csutil.algorithms.images {

    public class Mat<T> {
        public T[,] data;
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        

        public Mat(int rows, int cols) {
            Rows = rows;
            Cols = cols;
            data = new T[rows, cols];
        }

        public T this[int row, int col] {
            get => data[row, col];
            set => data[row, col] = value;
        }

    }
}
