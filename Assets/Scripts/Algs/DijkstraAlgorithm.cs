using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DijkstraAlgorithm
{
	public static int[] GetShortestPath(int[,] graph, int startNode, int endNode)
	{
		// Init
		int nodeCount = graph.GetLength(0);

		int[] distancies = Enumerable.Repeat(int.MaxValue, nodeCount).ToArray();
		bool[] shortestPathCalculated = Enumerable.Repeat(false, nodeCount).ToArray();

		distancies[startNode] = 0;

		// Start
		for (int count = 0; count < nodeCount - 1; count++)
		{
			int next = MinDistance(distancies, shortestPathCalculated);
			shortestPathCalculated[next] = true;

			for (int node = 0; node < nodeCount; node++)
				if (!shortestPathCalculated[node]
					&& Convert.ToBoolean(graph[next, node]) // != 0
					&& distancies[next] != int.MaxValue
					&& distancies[next] + graph[next, node] < distancies[node])
				{
					distancies[node] = distancies[next] + graph[next, node];
				}
		}






		// TODO: дейкстра
		return new int[0];
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
