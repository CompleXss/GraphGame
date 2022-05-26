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

	int[] GetAlgorithmStep(int[,] inputGraph, int fromNode, int toNode, out int[,] outputGraph, ref object dataToSave, out string message, out bool isAlgorithmFinished, out int nodeToHighlight)
	{
		// Init
		int nodeCount;
		int[,] graph, p;
		int k, i, j;

		isAlgorithmFinished = false;

		if (dataToSave is DataToSave data)
		{
			graph = data.graph;
			nodeCount = graph.GetLength(0);

			p = data.p;

			k = data.k;
			i = data.i;
			j = data.j;
		}
		else
		{
			data = new DataToSave();
			k = i = j = 0;

			nodeCount = inputGraph.GetLength(0);
			graph = (int[,])inputGraph.Clone();

			p = new int[nodeCount, nodeCount];
			for (int x = 0; x < nodeCount; x++)
				for (int y = 0; y < nodeCount; y++)
				{
					p[x, y] = -1;
				}
		}

		// Algorithm
		for (; k < nodeCount; k++, i = 0)
			for (; i < nodeCount; i++, j = 0)
				for (; j < nodeCount; j++)
					if (graph[i, k] != int.MaxValue && graph[k, j] != int.MaxValue)
						if ((graph[i, k] + graph[k, j]) < graph[i, j])
						{
							graph[i, j] = graph[i, k] + graph[k, j];
							p[i, j] = k;

							message = "Проведи линию из точки А в Б через выделенную точку.";
							nodeToHighlight = k;

							SaveData(ref dataToSave);

							outputGraph = graph;
							return GetFullPath(i, j, p);
						}
						else if (i != j)
						{
							message = "Проведи линию из точки А в Б.";
							nodeToHighlight = -1;

							SaveData(ref dataToSave);

							outputGraph = graph;
							return GetFullPath(i, j, p);
						}

		var path = GetFullPath(fromNode, toNode, p);

		message = "Работа алгоритма завершена.";
		isAlgorithmFinished = true;
		nodeToHighlight = -1;

		outputGraph = graph;
		return path;



		void SaveData(ref object dataToSave)
		{
			data.i = i;
			data.j = j + 1;
			data.k = k;

			data.p = p;
			data.graph = graph;
			dataToSave = data;
		}
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



	class DataToSave
	{
		public int[,] graph;
		public int[,] p;

		public int k, i, j;
	}
}
