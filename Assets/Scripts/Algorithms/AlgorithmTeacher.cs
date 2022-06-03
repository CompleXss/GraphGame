using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlgorithmTeacher : MonoBehaviour
{
	[SerializeField] private Graph graph;
	[SerializeField] private OutputGraph outputGraph;
	[SerializeField] private Slider stepSlider;
	[SerializeField] private TextMeshProUGUI stepNumber;
	[SerializeField] private PanelMover panelMover;
	[SerializeField] private NodeColors nodeColors;

	private Coroutine routine;
	private FindBestPathDelegate findBestPath_method;
	private List<(Node node, Action<Node> method)> subscribedNodes;
	private Queue<(int, int)> connectionsQueue;

	private bool teachingCanGo = true;

	private int step;
	private int Step
	{
		get => step;
		set
		{
			step = value;
			stepNumber.text = $"{value + 1}/{stepSlider.maxValue + 1}";

			stepSlider.SetValueWithoutNotify(value);
		}
	}



	// Awake
	private void Awake()
	{
		subscribedNodes = new List<(Node, Action<Node>)>();
	}



	#region Start / Stop
	public void StartAlgorithmTeaching(AlgorithmTeaching algorithm, int[,] matrix, Node startNode, Node endNode, FindBestPathDelegate findBestPath_method)
	{
		if (routine != null)
			StopCoroutine(routine);

		routine = StartCoroutine(AlgorithmTeaching(algorithm, matrix, startNode, endNode, findBestPath_method));
	}

	public void StopAlgorithmTeaching(bool findBestPath)
	{
		if (routine != null)
			StopCoroutine(routine);

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
	#endregion



	private IEnumerator AlgorithmTeaching(AlgorithmTeaching algorithm, int[,] matrix, Node startNode, Node endNode, FindBestPathDelegate findBestPath_method)
	{
		this.findBestPath_method = findBestPath_method;
		teachingCanGo = true;

		connectionsQueue = new Queue<(int, int)>();
		var results = algorithm((int[,])matrix.Clone(), startNode.ID, endNode.ID);

		if (results == null || results.Length < 1)
		{
			ScreenDebug.LogWarning("Выходной массив алгоритма обучения пустой или null.");
			yield break;
		}

		stepSlider.maxValue = results.Length - 2; // Последний шаг == полный путь (выход из алгоритма), поэтому его нельзя выбрать на слайдере



		for (Step = -1; Step < results.Length;)
		{
			while (!teachingCanGo)
				yield return new WaitForEndOfFrame();

			Step++;

			if (Step > 0)
				outputGraph.Show(results[Step - 1].graphCopy);
			else
				outputGraph.Show(null);

			int[] path = results[Step].path;

			if (Step == results.Length - 1)
			{
				graph.ValidateAndPrintPath(path);
				panelMover.HideAlgorithmTeachingPanel();
				break;
			}



			if (path.Length < 2)
			{
				ScreenDebug.LogWarning("Во время обучения получен путь длиной меньше 2");
				continue;
			}



			// Покраска узлов
			ClearNodesHighlighting();

			HighlightNode(path[0], nodeColors.Start);
			HighlightNode(path[path.Length - 1], nodeColors.End);

			if (path.Length > 3 || path.Length > 2 && results[Step].nodeToHighlight == -1)
			{
				for (int j = 1; j < path.Length - 1; j++)
					HighlightNode(path[j], nodeColors.Additional);
			}
			HighlightNode(results[Step].nodeToHighlight, nodeColors.Middle);



			for (int nodeID = 1; nodeID < path.Length; nodeID++)
			{
				connectionsQueue.Enqueue(ValueTuple.Create(path[nodeID - 1], path[nodeID]));
			}
			ScreenDebug.ShowTeachingMessage(results[Step].message);



			graph.ClearAllManualConnections();
			MakePlayerConnectNodes(connectionsQueue);
			yield return new WaitForEndOfFrame();
		}



		// Завершение работы
		ClearNodesHighlighting();
		graph.ClearAllManualConnections();
		connectionsQueue.Clear();

		string mes = results.Last().message;
		if (!string.IsNullOrWhiteSpace(mes))
			ScreenDebug.ShowTeachingMessage(mes);
		else
			ScreenDebug.ShowTeachingMessage("Работа алгоритма завершена.");
	}

	private void MakePlayerConnectNodes(Queue<(int, int)> connectionsQueue)
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

	public void StepSlider_OnValueChanged()
	{
		if (step == (int)stepSlider.value)
			return;

		step = (int)stepSlider.value - 1;
		stepNumber.text = step.ToString();

		foreach (var (node, method) in subscribedNodes)
			node.OnConnectedWith -= method;

		subscribedNodes.Clear();
		connectionsQueue.Clear();

		teachingCanGo = true;
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
