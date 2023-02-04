using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.algorithms {

    public class GraphClusteringChineseWhispers<T> {

        public static IEnumerable<IEnumerable<T>> ComputeClusters(IEnumerable<T> Collection, Func<T, T, double> WeightFunction, double Threshold, int? MaxIterations = null) {
            return ComputeClusters(Collection, WeightFunction, (v1, v2, w) => w > Threshold, MaxIterations);
        }

        public static IEnumerable<IEnumerable<T>> ComputeClusters(IEnumerable<T> Collection, Func<T, T, double> WeightFunction, Func<T, T, double, bool> ConnectionFunction = null, int? MaxIterations = null) {
            Graph graph = CreateGraph(Collection, WeightFunction, ConnectionFunction);
            int iterationIndex = 0;
            bool isChanged = false;
            Dictionary<int, double> WeightMap = new Dictionary<int, double>();
            do {
                isChanged = false;
                iterationIndex++;
                Randomize(graph.Vertices);
                foreach (Vertex Vertex in graph.Vertices) {
                    foreach (Edge Edge in Vertex.Edges) {
                        if (Edge.GetVertex(Vertex).Edges.Count == 0) continue;
                        if (!WeightMap.ContainsKey(Edge.GetVertex(Vertex).Cluster)) {
                            WeightMap.Add(Edge.GetVertex(Vertex).Cluster, 0);
                        }
                        WeightMap[Edge.GetVertex(Vertex).Cluster] += Edge.Weight; //  Edge.GetVertex(Vertex).Edges.Sum(edge => edge.Weight);
                    }
                    if (WeightMap.Count == 0) continue;
                    KeyValuePair<int, double> Max = GetMaxPair(WeightMap);
                    if (Max.Key != Vertex.Cluster) {
                        isChanged = true;
                        Vertex.Cluster = Max.Key;
                    }
                    WeightMap.Clear();
                }
            } while (isChanged || (MaxIterations.HasValue && iterationIndex >= MaxIterations.Value));
            var clusters = graph.Vertices.GroupBy(c => c.Cluster).Select(group => group.AsEnumerable().Select(v => v.Source));
            return clusters;
        }

        private static KeyValuePair<int, double> GetMaxPair(Dictionary<int, double> dictionary) {
            double max = double.MinValue;
            KeyValuePair<int, double> Result;
            foreach (KeyValuePair<int, double> Pair in dictionary) {
                if (Pair.Value > max) {
                    max = Pair.Value;
                    Result = Pair;
                }
            }
            return Result;
        }

        private static Graph CreateGraph(IEnumerable<T> Collection, Func<T, T, double> WeightFunction, Func<T, T, double, bool> ConnectionFunction) {
            Graph Graph = new Graph();
            if (Collection is ICollection<T>) {
                Graph.Vertices = new List<Vertex>(((ICollection<T>)Collection).Count);
            } else {
                Graph.Vertices = new List<Vertex>();
            }
            foreach (T Source in Collection) {
                Vertex Vertex = new Vertex() { Source = Source };
                Vertex.Cluster = Vertex.GetHashCode();
                Graph.Vertices.Add(Vertex);
            }
            for (int i = 0; i < Graph.Vertices.Count; i++) {
                for (int j = i + 1; j < Graph.Vertices.Count; j++) {
                    double Weight = WeightFunction(Graph.Vertices[i].Source, Graph.Vertices[j].Source);
                    if (ConnectionFunction == null || ConnectionFunction(Graph.Vertices[i].Source, Graph.Vertices[j].Source, Weight)) {
                        Edge Edge = new Edge() { Vertex1 = Graph.Vertices[i], Vertex2 = Graph.Vertices[j], Weight = Weight };
                        Graph.Vertices[i].Edges.Add(Edge);
                        Graph.Vertices[j].Edges.Add(Edge);
                    }
                }
            }
            return Graph;
        }

        /// <summary> Uses Fisher–Yates shuffle https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle </summary>
        private static void Randomize(List<Vertex> Vertices) {
            Random Rand = new Random();
            int n = Vertices.Count;
            while (n > 1) {
                n--;
                int k = Rand.Next(n + 1);
                Vertex value = Vertices[k];
                Vertices[k] = Vertices[n];
                Vertices[n] = value;
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