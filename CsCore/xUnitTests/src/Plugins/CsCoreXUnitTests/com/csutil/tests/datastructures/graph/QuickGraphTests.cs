using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.ShortestPath;
using Xunit;

namespace com.csutil.tests {

    public class QuickGraphTests {

        public QuickGraphTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestShortestPathViaAStar() {
            // https://github.com/KeRNeLith/QuikGraph/blob/master/tests/QuikGraph.Tests/Algorithms/ShortestPath/AStarShortestPathAlgorithmTests.cs
            var edges = new List<Edge<int>>();
            edges.Add(new Edge<int>(1, 2));
            edges.Add(new Edge<int>(2, 3));
            edges.Add(new Edge<int>(3, 4));

            var myGraph1 = new AdjacencyGraph<int, Edge<int>>();
            myGraph1.AddVerticesAndEdgeRange(edges);
            var result = RunAStarAndCheck(myGraph1, myGraph1.Vertices.First());
            Assert.NotNull(result.Distances);
            Assert.Equal(myGraph1.VertexCount, result.Distances.Count);
        }

        private AStarShortestPathAlgorithm<V, E> RunAStarAndCheck<V, E>(IVertexAndEdgeListGraph<V, E> graph, V root) where E : IEdge<V> {
            var distances = new Dictionary<E, double>();
            foreach (E edge in graph.Edges) { distances[edge] = graph.OutDegree(edge.Source) + 1; }

            var algorithm = new AStarShortestPathAlgorithm<V, E>(graph, e => distances[e], v => 0.0);

            var predecessors = new VertexPredecessorRecorderObserver<V, E>();
            using (predecessors.Attach(algorithm)) { algorithm.Compute(root); }
            return algorithm;
        }

    }

}