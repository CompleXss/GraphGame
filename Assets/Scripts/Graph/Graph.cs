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
	[SerializeField] private PanelMover panelMover;
	[SerializeField] private OutputGraph outputGraph;

	[Header("Nodes")]
	[SerializeField] private Node startNode;
	[SerializeField] private Node endNode;
	[SerializeField] private NodeColors nodeColors;

	[HideInInspector] public LineDrawer lineDrawer;

	public List<Node> Nodes { get; private set; }

	private RectTransform rectTransform;
	private Queue<LineRenderer> finalPathLines;
	private Coroutine algorithmTeachingRoutine;
	private FindBestPathDelegate findBestPath_teaching;
	private readonly List<ValueTuple<Node, Action<Node>>> subscribedNodes = new List<ValueTuple<Node, Action<Node>>>();

	private bool teachingCanGo = true;

	/// <summary> Is initialized in Start. Get it after Start only! </summary>
	public int[,] Matrix { get; private set; }



	// Awake & Start
	void Awake()
	{
		Nodes = new List<Node>();
		finalPathLines = new Queue<LineRenderer>();
		lineDrawer = GetComponent<LineDrawer>();
		rectTransform = GetComponent<RectTransform>();

		var children = GetComponentsInChildren<Node>();
		Nodes.AddRange(children);
	}

	void Start()
	{
		// Make all connections two-sided
		foreach (var node in Nodes)
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
					var firstNode = Nodes.Find(x => x.ID == i);
					var secondNode = Nodes.Find(x => x.ID == j);

					lineDrawer.DrawLineWithText(firstNode, secondNode);
				}

		startNode.MarkAs_StartNode(nodeColors.Start);
		endNode.MarkAs_EndNode(nodeColors.End);

		GraphScaler.FitGraphTo(rectTransform, Nodes);
	}



	private void DrawPath(int[] path, Color color)
	{
		for (int i = 0; i < path.Length - 1; i++)
		{
			var fromPos = Nodes.Find(x => x.ID == path[i]).transform.localPosition;
			var toPos = Nodes.Find(x => x.ID == path[i + 1]).transform.localPosition;

			var line = lineDrawer.DrawLine(fromPos, toPos, color, color);
			finalPathLines.Enqueue(line);
		}
	}

	private void ClearAllManualConnections()
	{
		foreach (var node in Nodes)
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
		var path = algorithm(Matrix, startNode.ID, endNode.ID);
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
		panelMover.ShowAlgorithmTeachingPanel();

		if (algorithmTeachingRoutine != null)
			StopCoroutine(algorithmTeachingRoutine);

		teachingCanGo = true;
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

		panelMover.HideAlgorithmTeachingPanel();
	}



	private IEnumerator AlgorithmTeaching(AlgorithmTeaching algorithm)
	{		
		var connectionsQueue = new Queue<(int, int)>();
		var results = algorithm((int[,])Matrix.Clone(), startNode.ID, endNode.ID);

		// TODO: возможность перематывать туда-сюда по шагам

		for (int i = 0; i < results.Length; i++)
		{
			while (!teachingCanGo)			
				yield return new WaitForEndOfFrame();			

			outputGraph.Show(results[i].graphCopy);
			int[] path = results[i].path;

			if (i == results.Length - 1)
			{
				ValidateAndPrintPath(path);
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



			ClearAllManualConnections();
			MakePlayerConnectNodes(connectionsQueue);
			yield return new WaitForEndOfFrame();
		}



		// Завершение работы
		ClearNodesHighlighting();
		ClearAllManualConnections();

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

		Node fromNode = Nodes.Find(x => x.ID == fromNodeID);
		Node toNode = Nodes.Find(x => x.ID == toNodeID);

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
		foreach (var node in Nodes)
			node.ClearHighlighting();
	}

	public void HighlightNode(Node node, Color color)
	{
		if (node != null)
			node.Highlight(color);
	}
	public void HighlightNode(int nodeID, Color color)
	{
		HighlightNode(Nodes.Find(x => x.ID == nodeID), color);
	}
	#endregion



	private bool ValidateStartEndNodes(string senderName = "")
	{
		bool validated = true;

		if (startNode == null)
		{
			ScreenDebug.LogWarning(senderName + " | StartNode is null.");
			validated = false;
		}

		if (endNode == null)
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
			|| path[0] != startNode.ID
			|| path[path.Length - 1] != endNode.ID)
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
			if (!Nodes.Find(x => x.ID == path[i]).Connections.Any(x => x.node.ID == path[i + 1]))
				return false;
		}



		// TODO: доп валидация?

		return true;
	}



	private int[,] GetMatrix()
	{
		// Matrix Init
		int[,] distancies = new int[Nodes.Count, Nodes.Count];
		for (int i = 0; i < Nodes.Count; i++)
			for (int j = 0; j < Nodes.Count; j++)
			{
				distancies[i, j] = i == j ? 0 : int.MaxValue;
			}

		// Matrix Fill
		foreach (var node in Nodes)
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
