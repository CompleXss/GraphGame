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


	// TODO: шаг алгоритма
	int[] GetAlgorithmStep(int[,] inputGraph, int fromNode, int toNode, ref object dataToSave, out string message, out int nodeToHighlight)
	{
		// Init
		int nodeCount = inputGraph.GetLength(0);
		int[,] graph, p;
		int k, i, j;

		if (dataToSave is DataToSave data)
		{
			graph = data.graph;
			p = data.p;

			k = data.k;
			i = data.i;
			j = data.j;
		}
		else
		{
			data = new DataToSave();
			k = i = j = 0;

			p = new int[nodeCount, nodeCount];

			graph = new int[nodeCount, nodeCount];
			for (int a = 0; a < nodeCount; a++)
				for (int b = 0; b < nodeCount; b++)
					graph[a, b] = inputGraph[a, b];
		}

		// Algorithm
		for (; k < nodeCount; k++)
			for (; i < nodeCount; i++)
				for (; j < nodeCount;)
					if (graph[i, k] != int.MaxValue && graph[k, j] != int.MaxValue
						&& (graph[i, k] + graph[k, j]) < graph[i, j])
					{
						graph[i, j] = graph[i, k] + graph[k, j];
						p[i, j] = k;

						message = "Проведи линию из точки А в Б через выделенную точку.";
						nodeToHighlight = k;

						// save data
						data.i = i;
						data.j = j;
						data.k = k;
						dataToSave = data;

						return new int[3] { i, k, j++ };
					}
					else
					{
						message = "Проведи линию из точки А в Б.";
						nodeToHighlight = -1;
						return new int[2] { i, j++ };
					}

		var path = new List<int>();
		path.Add(fromNode);
		path.AddRange(Path(fromNode, toNode, p));
		path.Add(toNode);



		message = "Работа алгоритма завершена.";
		nodeToHighlight = -1;

		return path.ToArray();
	}


	class DataToSave
	{
		public int[,] graph;
		public int[,] p;

		public int k, i, j;
	}

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
}

class SecondClass
{
	string Name => "Имя второго класса";

	int[] FindBestPath(int[,] graph, int a, int b)
	{
		return new int[1] { 10 };
	}
}