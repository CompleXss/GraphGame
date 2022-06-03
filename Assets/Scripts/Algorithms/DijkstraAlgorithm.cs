using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DijkstraAlgorithm
{
	public static int[] FindBestPath(int[,] graph, int fromNode, int toNode)
	{
		if (fromNode == toNode)
			return new int[1] { fromNode };

		// Previous nodes in optimal path from source
		var path = new List<int>();
		var previous = new Dictionary<int, int>();

		// Init
		int nodeCount = graph.GetLength(0);

		int[] distancies = Enumerable.Repeat(int.MaxValue, nodeCount).ToArray();
		bool[] shortestPathCalculated = Enumerable.Repeat(false, nodeCount).ToArray();

		distancies[fromNode] = 0;



		// Start
		for (int count = 0; count < nodeCount; count++)
		{
			int cur = MinDistance(distancies, shortestPathCalculated);
			shortestPathCalculated[cur] = true;

			// Если мы наткнулись на то, что ищем
			if (cur == toNode)
			{
				// Построить кратчайший путь
				while (previous.ContainsKey(cur))
				{
					path.Add(cur);

					cur = previous[cur];
				}
				path.Add(cur);

				break;
			}



			for (int node = 0; node < nodeCount; node++)
				if (!shortestPathCalculated[node]
					&& Convert.ToBoolean(graph[cur, node]) // != 0
					&& distancies[cur] != int.MaxValue
					&& graph[cur, node] != int.MaxValue
					&& distancies[cur] + graph[cur, node] < distancies[node])
				{
					distancies[node] = distancies[cur] + graph[cur, node];
					previous[node] = cur;
				}
		}

		path.Reverse();
		return path.ToArray();
	}

	public static (int[] path, int[,] graphCopy, string message, int nodeToHighlight)[] GetAlgorithmStep(int[,] graph, int fromNode, int toNode)
	{
		if (fromNode == toNode)
			return new (int[], int[,], string, int)[0];

		// Previous nodes in optimal path from source
		var path = new List<int>();
		var previous = new Dictionary<int, int>();

		// Init
		int nodeCount = graph.GetLength(0);

		int[] distancies = Enumerable.Repeat(int.MaxValue, nodeCount).ToArray();
		bool[] shortestPathCalculated = Enumerable.Repeat(false, nodeCount).ToArray();

		distancies[fromNode] = 0;



		// Обучение
		var output = new List<(int[] path, int[,] graphCopy, string message, int nodeToHighlight)>();
		string message;
		int[,] graphCopy;

		// Start
		for (int count = 0; count < nodeCount; count++)
		{
			int cur = MinDistance(distancies, shortestPathCalculated);
			shortestPathCalculated[cur] = true;

			// Если мы наткнулись на то, что ищем
			if (cur == toNode)
			{
				// Построить кратчайший путь
				while (previous.ContainsKey(cur))
				{
					path.Add(cur);

					cur = previous[cur];
				}
				path.Add(cur);

				break;
			}



			for (int node = 0; node < nodeCount; node++)
				if (!shortestPathCalculated[node]
					&& Convert.ToBoolean(graph[cur, node]) // != 0
					&& distancies[cur] != int.MaxValue
					&& graph[cur, node] != int.MaxValue
					&& distancies[cur] + graph[cur, node] < distancies[node])
				{
					distancies[node] = distancies[cur] + graph[cur, node];
					previous[node] = cur;

					// Обучение
					var teachingPath = new List<int>();
					teachingPath.AddRange(GetPath(cur, previous));
					teachingPath.Add(node);

					graphCopy = DistanciesToGraph(distancies, fromNode);
					message = $"Проведи линию из {teachingPath[0]} в {node}\nАлгоритм смотрит стоимость пути из {cur} в соседей";

					output.Add((teachingPath.ToArray(), graphCopy, message, -1));
				}
		}

		path.Reverse();

		output.Add((path.ToArray(), new int[0, 0], "Работа алгоритма завершена", -1));
		return output.ToArray();
	}

	private static int[] GetPath(int toNode, Dictionary<int, int> previous)
	{
		var path = new List<int>();

		var cur = toNode;
		while (previous.ContainsKey(cur))
		{
			path.Add(cur);
			cur = previous[cur];
		}
		path.Add(cur);
		path.Reverse();

		return path.ToArray();
	}

	private static int[,] DistanciesToGraph(int[] distancies, int fromNode)
	{
		int[,] graph = new int[distancies.Length, distancies.Length];
		for (int i = 0; i < distancies.Length; i++)
			for (int j = 0; j < distancies.Length; j++)
			{
				graph[i, j] = int.MaxValue;
			}



		for (int i = 0; i < distancies.Length; i++)
			graph[fromNode, i] = distancies[i];

		return graph;
	}

	private static int MinDistance(int[] distancies, bool[] shortestPathCalculated)
	{
		int nodeCount = distancies.Length;
		int min = int.MaxValue;
		int minIndex = 0;

		for (int node = 0; node < nodeCount; node++)
			if (!shortestPathCalculated[node] && distancies[node] <= min)
			{
				min = distancies[node];
				minIndex = node;
			}

		return minIndex;
	}
}
