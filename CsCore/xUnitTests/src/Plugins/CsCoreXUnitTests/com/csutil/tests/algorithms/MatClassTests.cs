using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using com.csutil.algorithms.images;

namespace com.csutil.tests.AlgorithmTests
{
    public class MatClassTests
    {
        private ITestOutputHelper _output;

        public MatClassTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task RunMatrixFunctionality()
        {
            var a = new Mat<int>(3, 3, 1);
            var b = new Mat<int>(3, 3, 1);
            a.ColorEntireChannel(1, 1);
            b.ColorEntireChannel(1, 2);
            _output.WriteLine(a.PrintMatrix());
            _output.WriteLine(b.PrintMatrix());
            var c1 = a + b;
            _output.WriteLine(c1.PrintMatrix());
            var c2 = a * b;
            _output.WriteLine(c2.PrintMatrix());

            
        }
    }
}
