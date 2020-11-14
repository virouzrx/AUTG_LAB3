using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AUTG_Zadanie3
{
    class VerticesNames
    {
        public static Dictionary<int, string> namesOfVertices = new Dictionary<int, string>(); //<- tu są numery wierzchołków jako klucze, a jako wartości ich nazwy
    }
    class VerticeDistances
    {
        public static Dictionary<int, List<int>> distancesOfVertice = new Dictionary<int, List<int>>(); //to jest slownik ktory zbiera informacje o dystansach od danego wierzcholka. Klucz to wierzcholek, a wartość to zbiór odleglosci do innych wierzcholkow
    }
    class Dijkstra //klasa dijkstra do przechowywania algorytmu
    {
        private Graph graph;
        private int[] distance;
        private int source;

        public Dijkstra(Graph graph, int source) 
        {
            this.graph = graph;
            this.source = source;

            distance = new int[graph.Vertices.Count];
        }

        public void Run()
        {
            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                distance[i] = int.MaxValue;
            }

            distance[source] = 0; 

            while (graph.Vertices.Count > 0)
            {
                int v = GetVertex(); 
                graph.Vertices.Remove(v);

                for (int i = 0; i < graph.Edges.Count; i++)
                {
                    if (graph.Edges[i].vertexV == v)
                    {
                        int tempDistance = distance[v] + GetWeight(v, graph.Edges[i].vertexU);

                        if (tempDistance < distance[graph.Edges[i].vertexU]) 
                        {
                            distance[graph.Edges[i].vertexU] = tempDistance;
                        }
                    }
                }
            }
            var tmp = new List<int>();
            foreach (var item in distance)
            {
                tmp.Add(item);
            }
            VerticeDistances.distancesOfVertice.Add(source, tmp); //dodawanie klucza i zbioru odleglosci do slownika
        }

        private int GetVertex()
        {
            int vertex = int.MinValue;
            int min = int.MaxValue;

            foreach (int v in graph.Vertices)
            {
                if (min > distance[v])
                {
                    min = distance[v];
                    vertex = v;
                }
            }

            return vertex;
        }

        private int GetWeight(int v, int u)
        {
            int weigth = int.MaxValue;

            foreach (Edge e in graph.Edges)
            {
                if (e.vertexV == v && e.vertexU == u)
                {
                    weigth = e.weigth;
                    break;
                }
            }

            return weigth;
        }

        public string Output()
        {
            string output = String.Empty;

            for (int i = 0; i < distance.Length; i++)
            {
                output += String.Format("From {0} to {1}: {2}\r\n", source, i, distance[i]);
            }

            return output;
        }
    }

    class Graph //graf jako obiekt, zawiera liste wierzcholkow i krawedzi
    {
        private List<Edge> edges;
        private List<int> vertices;

        public Graph()
        {
            edges = new List<Edge>();
            vertices = new List<int>();
        }
        public Graph ShallowCopy() //tu mialo byc kopiowanie obiektu, ale cos nie pyka
        {
           return (Graph)this.MemberwiseClone();
        }
        public void AddEdge(int vertexV, int vertexU, int weigth)
        {
            edges.Add(new Edge(vertexV, vertexU, weigth));
            edges.Add(new Edge(vertexU, vertexV, weigth));

            if (!vertices.Contains(vertexU))
            {
                vertices.Add(vertexU);
            }

            if (!vertices.Contains(vertexV))
            {
                vertices.Add(vertexV);
            }

            vertices.Sort();
        }

        public List<Edge> Edges
        {
            get
            {
                return edges;
            }
        }

        public List<int> Vertices
        {
            get
            {
                return vertices;
            }
        }
    }

    class Edge
    {
        public int vertexV;
        public int vertexU;
        public int weigth;

        public Edge(int vertexV, int vertexU, int weigth)
        {
            this.vertexV = vertexV;
            this.vertexU = vertexU;
            this.weigth = weigth;
        }
    }

    class Program
    {
        public static void ParseGraph(string sourceFile, Graph graph) //tu jest parsowanie grafu, wszystko dziala tutaj i nie trzeba na to patrzeć
        {
            var vnames = VerticesNames.namesOfVertices;
            var vertice = new Regex(@"^(.*?(\bname\b)[^$]*)$");
            var edge = new Regex(@"^(.*?(\bweight\b)[^$]*)$");
            var lines = File.ReadAllLines(sourceFile).Skip(1).ToArray();
            if (File.Exists(sourceFile))
            {
                foreach (var line in lines)
                {
                    if (vertice.IsMatch(line))
                    {
                        int bVerticeNumber = line.IndexOf("") + "".Length;
                        int eVerticeNumber = line.LastIndexOf(" ");
                        int bVerticeName = line.IndexOf("name=\"") + "name=\"".Length;
                        int eVerticeName = line.LastIndexOf("\"");
                        int verticeNumber = int.Parse(line.Substring(bVerticeNumber, eVerticeNumber - bVerticeNumber));
                        String verticeName = line.Substring(bVerticeName, eVerticeName - bVerticeName);
                        vnames.Add(verticeNumber, verticeName);
                    }
                    else if (edge.IsMatch(line))
                    {
                        int bSource = line.IndexOf("") + "".Length;
                        int eSource = line.LastIndexOf(" -");

                        int bDestination = line.IndexOf("- ") + "- ".Length;
                        int eDestination = line.LastIndexOf(" ");

                        int bWeight = line.IndexOf("weight=") + ("weight=").Length;
                        int eWeight = line.LastIndexOf("]");
                        
                        var weight = int.Parse(line.Substring(bWeight, eWeight - bWeight));
                        var source = int.Parse(line.Substring(bSource, eSource - bSource));
                        var destination = int.Parse(line.Substring(bDestination, eDestination - bDestination));

                        graph.AddEdge(source, destination, weight);
                        Console.WriteLine("Added {0} -- {1} weight: {2}", source, destination, weight);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                Console.WriteLine("Nie znaleziono pliku");
            }
        }

        public static void MinMax() 
        {
            var distances = VerticeDistances.distancesOfVertice;
            var maxVertexDistance = new Dictionary<int, int>();
            foreach (var item in distances)
            {
                maxVertexDistance.Add(item.Key, item.Value.Max());
            }
            var min = maxVertexDistance.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
            Console.WriteLine("Answer: {0}", VerticesNames.namesOfVertices[min]);
        }
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Graph graph = new Graph(); //tworze nowy obiekt typu graf
            ParseGraph(args[0], graph); //tutaj parsuje graf z pliku 

            for (int i = 0; i < graph.Vertices.Count; i++) //tutaj sie odpala dijkstra. Tylko ze bierze on tylko pierwszy wierzcholek,bo algorytm sam w sobie usuwa wierzcholki ktore przebyl. Chcialbym by bral on KAZDY wierzcholek i odplalał na nim algorytm
            {
                Graph copiedGraph = graph.ShallowCopy(); //chcialem SKOPIOWAC ten graf aby dijkstra dzialal na kopii, a petla brala informacje z pelnego grafu 
                Dijkstra dijkstra = new Dijkstra(copiedGraph, i); //djikstra bierze SKOPIOWANY GRAF - czemu wiec oryginalny jest modyfikowany?
                dijkstra.Run(); //odpal algorytm
                Console.WriteLine(dijkstra.Output()); //wypisz jego wynik
            }

            if (!graph.Vertices.Any())
            {
                Console.WriteLine("kurwa pusty graf"); //tutaj sobie sprawdzam czy moj graf jest pusty
            }
            foreach (var item in VerticeDistances.distancesOfVertice)
            {
                Console.Write("\"{0}\": ",VerticesNames.namesOfVertices[item.Key]);
                foreach (var item2 in item.Value)
                {
                    Console.Write("{0} ", item2);
                }
                Console.WriteLine();
            }
            MinMax();
            stopwatch.Stop();
            Console.WriteLine("Press any key..");
            Console.ReadKey();
        }
    }
}

