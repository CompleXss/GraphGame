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
		int[,] graph = (int[,])inputGraph.Clone();

		int[,] p = new int[nodeCount, nodeCount];
		for (int x = 0; x < nodeCount; x++)
			for (int y = 0; y < nodeCount; y++)
			{
				p[x, y] = -1;
			}

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

		return GetFullPath(fromNode, toNode, p);
	}

	(int[] path, int[,] graphCopy, string message, int nodeToHighlight)[] GetAlgorithmStep(int[,] inputGraph, int fromNode, int toNode)
	{
		// Init
		int nodeCount;
		int[,] graph, p;

		nodeCount = inputGraph.GetLength(0);
		graph = (int[,])inputGraph.Clone();

		p = new int[nodeCount, nodeCount];
		for (int x = 0; x < nodeCount; x++)
			for (int y = 0; y < nodeCount; y++)
			{
				p[x, y] = -1;
			}

		var output = new List<(int[], int[,], string, int)>();
		int[] path;
		string message;
		int nodeToHighlight;
		int[,] graphCopy;

		// Algorithm
		for (int k = 0; k < nodeCount; k++)
			for (int i = 0; i < nodeCount; i++)
				for (int j = 0; j < nodeCount; j++)
					if (graph[i, k] != int.MaxValue && graph[k, j] != int.MaxValue)
						if ((graph[i, k] + graph[k, j]) < graph[i, j])
						{
							graph[i, j] = graph[i, k] + graph[k, j];
							p[i, j] = k;

							path = GetFullPath(i, j, p);
							message = "Проведи линию из точки А в Б через выделенную точку.";
							nodeToHighlight = k;
							graphCopy = (int[,])graph.Clone();

							output.Add((path, graphCopy, message, nodeToHighlight));
						}
		//else if (i != j)
		//{
		//	string message = "Проведи линию из точки А в Б.";
		//	int nodeToHighlight = -1;

		//	int[,] outputGraph = (int[,])graph.Clone();
		//	output.Add((GetFullPath(i, j, p), outputGraph, message, nodeToHighlight));
		//}

		path = GetFullPath(fromNode, toNode, p);
		message = "Работа алгоритма завершена.";
		nodeToHighlight = -1;
		graphCopy = (int[,])graph.Clone();

		output.Add((path, graphCopy, message, nodeToHighlight));

		return output.ToArray();
	}



	int[] GetPathBetween(int i, int j, int[,] p)
	{
		int k = p[i, j];
		if (k == -1)
			return new int[0];

		var path = new List<int>();
		path.AddRange(GetPathBetween(i, k, p));
		path.Add(k);
		path.AddRange(GetPathBetween(k, j, p));

		return path.ToArray();
	}

	int[] GetFullPath(int fromNode, int toNode, int[,] p)
	{
		var path = new List<int>();
		path.Add(fromNode);
		path.AddRange(GetPathBetween(fromNode, toNode, p));
		path.Add(toNode);

		return path.ToArray();
	}
}
