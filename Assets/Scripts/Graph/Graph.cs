using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LineDrawer))]
public class Graph : MonoBehaviour
{
	[SerializeField] private UI ui;
	[SerializeField] private OutputGraph outputGraph;

	[Header("Ноды")]
	[SerializeField] private Node startNode;
	[SerializeField] private Node endNode;
	[SerializeField] private NodeColors nodeColors;

	[HideInInspector] public LineDrawer lineDrawer;

	private List<Node> nodes;
	private Queue<LineRenderer> finalPathLines;

	private Coroutine algorithmTeachingRoutine;
	private FindBestPathDelegate findBestPath_teaching;
	private List<ValueTuple<Node, Action<Node>>> subscribedNodes = new List<ValueTuple<Node, Action<Node>>>();

	private bool teachingCanGo = true;



	/// <summary> Is initialized in Start. Get it after Start only! </summary>
	public int[,] Matrix { get; private set; }
	public Node StartNode
	{
		get => startNode;
		private set => startNode = value;
	}
	public Node EndNode
	{
		get => endNode;
		private set => endNode = value;
	}



	// Awake & Start
	void Awake()
	{
		nodes = new List<Node>();
		finalPathLines = new Queue<LineRenderer>();
		lineDrawer = GetComponent<LineDrawer>();

		var children = GetComponentsInChildren<Node>();
		nodes.AddRange(children);
	}

	void Start()
	{
		// Make all connections two-sided
		foreach (var node in nodes)
			foreach (var con in node.Connections)
			{
				if (!con.node.Connections.Any(x => x.node == node))
					con.node.Connections.Add(new Connection(node, con.weight));
			}

		Matrix = GetMatrix();

		// Draw lines
		var len = Matrix.GetLength(0);
		int len2 = 1;

		for (int i = 0; i < len; i++, len2++)
			for (int j = 0; j < len2; j++)
				if (Matrix[i, j] != 0 && Matrix[i, j] != int.MaxValue)
				{
					var firstNode = nodes.Find(x => x.ID == i);
					var secondNode = nodes.Find(x => x.ID == j);

					lineDrawer.DrawLineWithText(firstNode, secondNode);
				}

		StartNode.MarkAs_StartNode(nodeColors.Start);
		EndNode.MarkAs_EndNode(nodeColors.End);
	}



	private void DrawPath(int[] path, Color color)
	{
		for (int i = 0; i < path.Length - 1; i++)
		{
			var fromPos = nodes.Find(x => x.ID == path[i]).transform.localPosition;
			var toPos = nodes.Find(x => x.ID == path[i + 1]).transform.localPosition;

			var line = lineDrawer.DrawLine(fromPos, toPos, color, color);
			finalPathLines.Enqueue(line);
		}
	}

	private void ClearAllManualConnections()
	{
		foreach (var node in nodes)
			node.ClearManualConnection();
	}

	private void ClearFinalPath()
	{
		ClearAllManualConnections();

		while (finalPathLines.Count > 0)
			Destroy(finalPathLines.Dequeue().gameObject);
	}

	private string PathToString(int[] path)
	{
		StringBuilder str = new StringBuilder("");
		foreach (var p in path)
		{
			str.Append(p);
			str.Append(" ");
		}
		return str.ToString();
	}

	private bool ValidateAndPrintPath(int[] path)
	{
		if (ValidateStartEndNodes() && ValidatePath(path))
		{
			DrawPath(path, Color.yellow);
			ScreenDebug.Log("Успешный путь: " + PathToString(path));

			return true;
		}
		else
		{
			ScreenDebug.LogWarning("Ошибка пути: " + PathToString(path));

			return false;
		}
	}



	/// <summary> Ищет и показывает лучший маршрут. </summary>
	public void FindBestPath(FindBestPathDelegate algorithm)
	{
		if (algorithm == null)
			return;

		ClearFinalPath();
		var sw = new System.Diagnostics.Stopwatch();

		sw.Start();
		var path = algorithm(Matrix, StartNode.ID, EndNode.ID);
		sw.Stop();

		ScreenDebug.ShowTime(sw.ElapsedMilliseconds.ToString());

		if (ValidateAndPrintPath(path))
			ScreenDebug.Log("Поиск лучшего маршрута завершен!");
	}



	/// <summary>
	/// Начинает процесс "обучения". Если <paramref name="findBestPathDelegate"/> будет не null, появится возможность увидеть результ работы алгоритма после остановки обучения.
	/// </summary>
	public void StartAlgorithmTeaching(AlgorithmTeaching algorithm, FindBestPathDelegate findBestPathDelegate)
	{
		if (algorithm == null)
			return;

		ScreenDebug.ClearTime();

		findBestPath_teaching = findBestPathDelegate;

		ClearFinalPath();
		ui.ShowAlgorithmTeachingPanel();

		if (algorithmTeachingRoutine != null)
			StopCoroutine(algorithmTeachingRoutine);

		algorithmTeachingRoutine = StartCoroutine(AlgorithmTeaching(algorithm));
	}



	/// <summary>
	/// Останавливает процесс "обучения", и если переданный ранее алгоритм поиска кратчайшего пути не null, показывает результат работы алгоритма.
	/// </summary>
	public void StopAlgorithmTeaching(bool findBestPath)
	{
		if (algorithmTeachingRoutine != null)
			StopCoroutine(algorithmTeachingRoutine);

		ClearNodesHighlighting();

		foreach (var tuple in subscribedNodes)
		{
			var node = tuple.Item1;
			var method = tuple.Item2;

			node.OnConnectedWith -= method;
		}
		teachingCanGo = true;

		if (findBestPath && findBestPath_teaching != null)
		{
			FinaleAlgorithmTeachng(findBestPath_teaching);
			findBestPath_teaching = null;
		}
	}

	private void FinaleAlgorithmTeachng(FindBestPathDelegate findBestPathAlgorithm)
	{
		if (findBestPathAlgorithm != null)
			FindBestPath(findBestPathAlgorithm);

		ui.HideAlgorithmTeachingPanel();
	}



	private IEnumerator AlgorithmTeaching(AlgorithmTeaching algorithm)
	{
		int[,] graph = null;
		object dataToSave = null;
		string message = null;

		var connectionsQueue = new Queue<ValueTuple<int, int>>();

		while (true)
		{
			if (teachingCanGo)
			{
				outputGraph.Show(graph);

				var path = algorithm((int[,])Matrix.Clone(), StartNode.ID, EndNode.ID, out graph, ref dataToSave, out message, out bool isAlgorithmFinished, out int nodeToHighlight);

				if (isAlgorithmFinished)
				{
					ValidateAndPrintPath(path);
					ui.HideAlgorithmTeachingPanel();
					break;
				}

				if (path.Length < 2)
				{
					ScreenDebug.LogWarning("В процессе обучения получен путь длиной меньше 2.");
					yield return new WaitForEndOfFrame();
				}



				ClearNodesHighlighting();

				HighlightNode(path[0], nodeColors.Start);
				HighlightNode(path[path.Length - 1], nodeColors.End);
				HighlightNode(nodeToHighlight, nodeColors.Middle);

				if (path.Length > 2 && nodeToHighlight == -1 || path.Length > 3)
				{
					for (int i = 1; i < path.Length - 1; i++)
						HighlightNode(path[i], nodeColors.Additional);
				}



				for (int nodeID = 1; nodeID < path.Length; nodeID++)
				{
					connectionsQueue.Enqueue(ValueTuple.Create(path[nodeID - 1], path[nodeID]));
				}
				ScreenDebug.ShowTeachingMessage(message);



				//Debug.Log(PathToString(path));



				ClearAllManualConnections();
				MakePlayerConnectNodes(connectionsQueue);
			}

			yield return new WaitForEndOfFrame();
		}



		// Завершение работы
		ClearNodesHighlighting();

		if (!string.IsNullOrWhiteSpace(message))
			ScreenDebug.ShowTeachingMessage(message);
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

		Node fromNode = nodes.Find(x => x.ID == fromNodeID);
		Node toNode = nodes.Find(x => x.ID == toNodeID);

		if (!ValidateStartEndNodes("Алгоритм обучения"))
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
		foreach (var node in nodes)
			node.ClearHighlighting();
	}

	public void HighlightNode(Node node, Color color)
	{
		if (node != null)
			node.Highlight(color);
	}
	public void HighlightNode(int nodeID, Color color)
	{
		HighlightNode(nodes.Find(x => x.ID == nodeID), color);
	}
	#endregion



	private bool ValidateStartEndNodes(string senderName = "")
	{
		bool validated = true;

		if (StartNode == null)
		{
			ScreenDebug.LogWarning(senderName + " | StartNode is null.");
			validated = false;
		}

		if (EndNode == null)
		{
			ScreenDebug.LogWarning(senderName + " | EndNode is null.");
			validated = false;
		}

		return validated;
	}

	private bool ValidatePath(int[] path)
	{
		if (path == null
			|| path.Length < 1
			|| path[0] != StartNode.ID
			|| path[path.Length - 1] != EndNode.ID)
			return false;

		// Проверка на упоминание ноды всего раз
		var nodeVisitedTimes = new Dictionary<int, int>();

		foreach (var p in path)
		{
			if (nodeVisitedTimes.ContainsKey(p))
				nodeVisitedTimes[p]++;
			else
				nodeVisitedTimes[p] = 1;
		}
		if (!nodeVisitedTimes.Values.ToList().TrueForAll(x => x < 2))
			return false;

		// Проверка на допустимость маршрута (такие пути существуют)
		for (int i = 0; i < path.Length - 1; i++)
		{
			if (!nodes.Find(x => x.ID == path[i]).Connections.Any(x => x.node.ID == path[i + 1]))
				return false;
		}



		// TODO: доп валидация?

		return true;
	}



	private int[,] GetMatrix()
	{
		// Matrix Init
		int[,] distancies = new int[nodes.Count, nodes.Count];
		for (int i = 0; i < nodes.Count; i++)
			for (int j = 0; j < nodes.Count; j++)
			{
				distancies[i, j] = i == j ? 0 : int.MaxValue;
			}

		// Matrix Fill
		foreach (var node in nodes)
			foreach (var con in node.Connections)
			{
				distancies[node.ID, con.node.ID] = con.weight;
			}

		return distancies;
	}
	private void LogMatrix()
	{
		for (int i = 0; i < Matrix.GetLength(0); i++)
			for (int j = 0; j < Matrix.GetLength(1); j++)
			{
				Debug.Log($"{i}_{j}: {Matrix[i, j]}");
			}
	}
}
