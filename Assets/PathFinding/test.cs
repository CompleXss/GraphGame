using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

class FloydAlgorithm
{
	string Name => "Алгоритм Флойда-Уоршелла";

	int[] FindBestPath(int[,] inputGraph, int fromNode, int toNode)
	{
		// Init
		int nodeCount = inputGraph.GetLength(0);
		var p = new int[nodeCount, nodeCount];

		var graph = new int[nodeCount, nodeCount];
		for (int i = 0; i < nodeCount; i++)
			for (int j = 0; j < nodeCount; j++)
				graph[i, j] = inputGraph[i, j];

		// Algorithm
		for (int k = 0; k < nodeCount; k++)
			for (int i = 0; i < nodeCount; i++)
				for (int j = 0; j < nodeCount; j++)
					if (graph[i, k] != int.MaxValue && graph[k, j] != int.MaxValue)
						if ((graph[i, k] + graph[k, j]) < graph[i, j])
						{
							graph[i, j] = graph[i, k] + graph[k, j];
							p[i, j] = k;
						}

		var path = new List<int>();
		path.Add(fromNode);
		path.AddRange(Path(fromNode, toNode, p));
		path.Add(toNode);



		return path.ToArray();
	}

	//int[][] FindAllPaths(int[,] inputGraph, int fromNode, int toNode)
	//{

	//}

	int[] Path(int i, int j, int[,] p)
	{
		int k = p[i, j];
		if (k == 0)
			return new int[0];

		var path = new List<int>();
		path.AddRange(Path(i, k, p));
		path.Add(k);
		path.AddRange(Path(k, j, p));

		return path.ToArray();
	}

	//int[,] Floyd(int[,] graph, int nodeCount)
	//{


	//}
}

class SecondClass
{
	string Name => "Имя второго класса";

	int[] FindBestPath(int[,] graph, int a, int b)
	{
		return new int[1] { 10 };
	}
}