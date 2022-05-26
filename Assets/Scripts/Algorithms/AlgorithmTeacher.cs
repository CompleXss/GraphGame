using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AlgorithmTeacher
{
	private FindBestPathDelegate findBestPath_method;
	private readonly List<(Node node, Action<Node> method)> subscribedNodes;

	private readonly Graph graph;
	private readonly OutputGraph outputGraph;
	private readonly NodeColors nodeColors;
	private readonly PanelMover panelMover;

	private bool teachingCanGo = true;



	// Constructor
	public AlgorithmTeacher(Graph graph, OutputGraph outputGraph, PanelMover panelMover, NodeColors nodeColors)
	{
		this.graph = graph;
		this.outputGraph = outputGraph;
		this.panelMover = panelMover;
		this.nodeColors = nodeColors;

		subscribedNodes = new List<(Node, Action<Node>)>();
	}



	public void StopAlgorithmTeaching(bool findBestPath)
	{
		ClearNodesHighlighting();

		foreach (var tuple in subscribedNodes)
		{
			var node = tuple.node;
			var method = tuple.method;

			node.OnConnectedWith -= method;
		}
		teachingCanGo = true;

		if (findBestPath && findBestPath_method != null)
		{
			FinaleAlgorithmTeachng(findBestPath_method);
			findBestPath_method = null;
		}
	}

	private void FinaleAlgorithmTeachng(FindBestPathDelegate findBestPathAlgorithm)
	{
		if (findBestPathAlgorithm != null)
			graph.FindBestPath(findBestPathAlgorithm);

		panelMover.HideAlgorithmTeachingPanel();
	}



	public IEnumerator AlgorithmTeaching(AlgorithmTeaching algorithm, int[,] matrix, Node startNode, Node endNode, FindBestPathDelegate findBestPath_method)
	{
		this.findBestPath_method = findBestPath_method;
		teachingCanGo = true;

		var connectionsQueue = new Queue<(int, int)>();
		var results = algorithm((int[,])matrix.Clone(), startNode.ID, endNode.ID);

		// TODO: возможность перематывать туда-сюда по шагам

		for (int i = 0; i < results.Length; i++)
		{
			while (!teachingCanGo)
				yield return new WaitForEndOfFrame();

			outputGraph.Show(results[i].graphCopy);
			int[] path = results[i].path;

			if (i == results.Length - 1)
			{
				graph.ValidateAndPrintPath(path);
				panelMover.HideAlgorithmTeachingPanel();
				break;
			}



			if (path.Length < 2)
			{
				ScreenDebug.LogWarning("В процессе обучения получен путь длиной меньше 2.");
				continue;
			}



			// Покраска узлов
			ClearNodesHighlighting();

			HighlightNode(path[0], nodeColors.Start);
			HighlightNode(path[path.Length - 1], nodeColors.End);

			if (path.Length > 3 || path.Length > 2 && results[i].nodeToHighlight == -1)
			{
				for (int j = 1; j < path.Length - 1; j++)
					HighlightNode(path[j], nodeColors.Additional);
			}
			HighlightNode(results[i].nodeToHighlight, nodeColors.Middle);



			for (int nodeID = 1; nodeID < path.Length; nodeID++)
			{
				connectionsQueue.Enqueue(ValueTuple.Create(path[nodeID - 1], path[nodeID]));
			}
			ScreenDebug.ShowTeachingMessage(results[i].message);



			// TODO: debug path ?
			//Debug.Log(PathToString(path));



			graph.ClearAllManualConnections();
			MakePlayerConnectNodes(connectionsQueue);
			yield return new WaitForEndOfFrame();
		}



		// Завершение работы
		ClearNodesHighlighting();
		graph.ClearAllManualConnections();

		string mes = results.Last().message;
		if (!string.IsNullOrWhiteSpace(mes))
			ScreenDebug.ShowTeachingMessage(mes);
		else
			ScreenDebug.ShowTeachingMessage("Работа алгоритма завершена.");
	}

	private void MakePlayerConnectNodes(Queue<ValueTuple<int, int>> connectionsQueue)
	{
		if (connectionsQueue.Count < 1)
			return;

		teachingCanGo = false;

		var twoNodes = connectionsQueue.Dequeue();
		int fromNodeID = twoNodes.Item1;
		int toNodeID = twoNodes.Item2;

		Node fromNode = graph.Nodes.Find(x => x.ID == fromNodeID);
		Node toNode = graph.Nodes.Find(x => x.ID == toNodeID);

		if (!graph.ValidateStartEndNodes("Алгоритм обучения"))
		{
			teachingCanGo = true;
			return;
		}



		fromNode.OnConnectedWith += CheckIfNodeIsRight;
		subscribedNodes.Add((fromNode, CheckIfNodeIsRight));

		void CheckIfNodeIsRight(Node conNode)
		{
			if (conNode != null && conNode.ID == toNodeID)
			{
				teachingCanGo = true;
				fromNode.OnConnectedWith -= CheckIfNodeIsRight;
				subscribedNodes.Remove((fromNode, CheckIfNodeIsRight));

				MakePlayerConnectNodes(connectionsQueue);
			}
		}
	}



	#region Node highlighting
	public void ClearNodesHighlighting()
	{
		foreach (var node in graph.Nodes)
			node.ClearHighlighting();
	}

	public void HighlightNode(Node node, Color color)
	{
		if (node != null)
			node.Highlight(color);
	}
	public void HighlightNode(int nodeID, Color color)
	{
		HighlightNode(graph.Nodes.Find(x => x.ID == nodeID), color);
	}
	#endregion
}
