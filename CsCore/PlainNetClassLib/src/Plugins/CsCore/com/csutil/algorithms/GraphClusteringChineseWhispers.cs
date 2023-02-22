using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.algorithms {

    public class GraphClusteringChineseWhispers<T> {

        public static IEnumerable<IEnumerable<T>> ComputeClusters(IEnumerable<T> collection, Func<T, T, double> weightFunction, double threshold, int? maxIterations = null) {
            return ComputeClusters(collection, weightFunction, (v1, v2, w) => w > threshold, maxIterations);
        }

        public static IEnumerable<IEnumerable<T>> ComputeClusters(IEnumerable<T> collection, Func<T, T, double> weightFunction, Func<T, T, double, bool> connectionFunction = null, int? maxIterations = null) {
            Graph graph = CreateGraph(collection, weightFunction, connectionFunction);
            int iterationIndex = 0;
            bool isChanged = false;
            Dictionary<int, double> weightMap = new Dictionary<int, double>();
            do {
                isChanged = false;
                iterationIndex++;
                Randomize(graph.Vertices);
                foreach (Vertex vertex in graph.Vertices) {
                    foreach (Edge edge in vertex.Edges) {
                        if (edge.GetVertex(vertex).Edges.Count == 0) continue;
                        if (!weightMap.ContainsKey(edge.GetVertex(vertex).Cluster)) {
                            weightMap.Add(edge.GetVertex(vertex).Cluster, 0);
                        }
                        weightMap[edge.GetVertex(vertex).Cluster] += edge.Weight; //  Edge.GetVertex(Vertex).Edges.Sum(edge => edge.Weight);
                    }
                    if (weightMap.Count == 0) continue;
                    KeyValuePair<int, double> max = GetMaxPair(weightMap);
                    if (max.Key != vertex.Cluster) {
                        isChanged = true;
                        vertex.Cluster = max.Key;
                    }
                    weightMap.Clear();
                }
            } while (isChanged || (maxIterations.HasValue && iterationIndex >= maxIterations.Value));
            var clusters = graph.Vertices.GroupBy(c => c.Cluster).Select(group => group.AsEnumerable().Select(v => v.Source));
            return clusters;
        }

        private static KeyValuePair<int, double> GetMaxPair(Dictionary<int, double> dictionary) {
            double max = double.MinValue;
            KeyValuePair<int, double> result = default;
            foreach (KeyValuePair<int, double> pair in dictionary) {
                if (pair.Value > max) {
                    max = pair.Value;
                    result = pair;
                }
            }
            return result;
        }

        private static Graph CreateGraph(IEnumerable<T> collection, Func<T, T, double> weightFunction, Func<T, T, double, bool> connectionFunction) {
            Graph graph = new Graph();
            if (collection is ICollection<T>) {
                graph.Vertices = new List<Vertex>(((ICollection<T>)collection).Count);
            } else {
                graph.Vertices = new List<Vertex>();
            }
            foreach (T source in collection) {
                Vertex vertex = new Vertex() { Source = source };
                vertex.Cluster = vertex.GetHashCode();
                graph.Vertices.Add(vertex);
            }
            for (int i = 0; i < graph.Vertices.Count; i++) {
                for (int j = i + 1; j < graph.Vertices.Count; j++) {
                    double weight = weightFunction(graph.Vertices[i].Source, graph.Vertices[j].Source);
                    if (connectionFunction == null || connectionFunction(graph.Vertices[i].Source, graph.Vertices[j].Source, weight)) {
                        Edge edge = new Edge() { Vertex1 = graph.Vertices[i], Vertex2 = graph.Vertices[j], Weight = weight };
                        graph.Vertices[i].Edges.Add(edge);
                        graph.Vertices[j].Edges.Add(edge);
                    }
                }
            }
            return graph;
        }

        /// <summary> Uses Fisher–Yates shuffle https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle </summary>
        private static void Randomize(List<Vertex> vertices) {
            Random rnd = new Random();
            int n = vertices.Count;
            while (n > 1) {
                n--;
                int k = rnd.Next(n + 1);
                Vertex value = vertices[k];
                vertices[k] = vertices[n];
                vertices[n] = value;
            }
        }

        private class Graph {
            public List<Vertex> Vertices;
        }

        private class Vertex {
            public int Cluster;
            public List<Edge> Edges = new List<Edge>();
            public T Source;
        }

        private class Edge {
            public double Weight = 0;
            public Vertex Vertex1;
            public Vertex Vertex2;

            public Vertex GetVertex(Vertex vertex) {
                return (vertex == Vertex1) ? Vertex2 : Vertex1;
            }
        }

    }

}