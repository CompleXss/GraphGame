using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DijkstraAlgorithm
{
	public static int[] FindBestPath(int[,] graph, int startNode, int endNode)
	{
		if (startNode == endNode)
			return new int[1] { startNode };

		// Previous nodes in optimal path from source
		var path = new List<int>();
		var previous = new Dictionary<int, int>();

		// Init
		int nodeCount = graph.GetLength(0);

		int[] distancies = Enumerable.Repeat(int.MaxValue, nodeCount).ToArray();
		bool[] shortestPathCalculated = Enumerable.Repeat(false, nodeCount).ToArray();

		distancies[startNode] = 0;

		// Start
		for (int count = 0; count < nodeCount; count++)
		{
			int cur = MinDistance(distancies, shortestPathCalculated);
			shortestPathCalculated[cur] = true;

			// Если мы наткнулись на то, что ищем
			if (cur == endNode)
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
					&& distancies[cur] + graph[cur, node] < distancies[node])
				{
					distancies[node] = distancies[cur] + graph[cur, node];
					previous[node] = cur;
				}
		}

		// TODO: дейкстра
		path.Reverse();
		return path.ToArray();
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
