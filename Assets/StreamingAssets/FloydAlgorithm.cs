using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

class FloydAlgorithm
{
	string Name => "Алгоритм Флойда-Уоршелла";



	int[] FindBestPath(int[,] graph, int fromNode, int toNode)
	{
		// Инициализация
		int nodeCount = graph.GetLength(0);

		int[,] p = new int[nodeCount, nodeCount];
		for (int x = 0; x < nodeCount; x++)
			for (int y = 0; y < nodeCount; y++)
			{
				p[x, y] = -1;
			}

		// Сам алгоритм
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
		// Инициализация
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

		var output = new List<(int[] path, int[,] graphCopy, string message, int nodeToHighlight)>();
		int[] path;
		string message;
		int nodeToHighlight;
		int[,] graphCopy;

		// Сам алгоритм
		for (int k = 0; k < nodeCount; k++)
			for (int i = 0; i < nodeCount; i++)
				for (int j = 0; j < nodeCount; j++)
					if (graph[i, k] != int.MaxValue && graph[k, j] != int.MaxValue)
						if ((graph[i, k] + graph[k, j]) < graph[i, j])
						{
							graph[i, j] = graph[i, k] + graph[k, j];
							p[i, j] = k;

							// Вывод данных для отображения
							path = GetFullPath(i, j, p);
							graphCopy = (int[,])graph.Clone(); // Обязательно именно копировать массив!
							message = $"Проведи линию из точки {i} в {j} через выделенную точку.";
							nodeToHighlight = k;

							output.Add((path, graphCopy, message, nodeToHighlight));
						}

		// Получен итоговый путь
		path = GetFullPath(fromNode, toNode, p);
		graphCopy = (int[,])graph.Clone(); // Обязательно именно копировать массив!
		message = "Работа алгоритма завершена.";
		nodeToHighlight = -1;

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
