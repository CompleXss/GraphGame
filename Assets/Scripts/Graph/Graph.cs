using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Graph : MonoBehaviour
{
	[SerializeField] private UI ui;

	[Space]
	[SerializeField] private Node startNode;
	[SerializeField] private Node endNode;

	[Header("Отрисовка линий")]
	[SerializeField] private LineInfo lineInfo;
	[SerializeField] private Transform linesParent;

	[Header("Подсветка нод")]
	[SerializeField] private Color Start_NodeColor = Color.green;
	[SerializeField] private Color End_NodeColor = Color.red;
	[SerializeField] private Color ConnectionStart_NodeColor = Color.cyan;
	[SerializeField] private Color ConnectionEnd_NodeColor = Color.magenta;
	[SerializeField] private Color HighlightMiddle_NodeColor = Color.yellow;

	private List<Node> nodes;
	private List<Transform> texts;
	private Queue<LineRenderer> finalPathLines;

	private Coroutine algorithmTeachingRoutine;
	private List<ValueTuple<Node, Action<Node>>> subscribedNodes = new List<ValueTuple<Node, Action<Node>>>();

	private Node highlightedMiddleNode;
	private Node highlightedConnectionStartNode;
	private Node highlightedConnectionEndNode;
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



	void Awake()
	{
		nodes = new List<Node>();
		texts = new List<Transform>();
		finalPathLines = new Queue<LineRenderer>();

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

					var line = DrawLine(firstNode.transform.localPosition, secondNode.transform.localPosition);
					DrawLineText(line, firstNode, secondNode);
				}



		//// old не учитывает, что связи двусторонние
		//foreach (var node in nodes)
		//	foreach (var con in node.Connections)
		//	{
		//		var line = GetInstantiatedLine();
		//		line.SetPosition(0, node.transform.localPosition);
		//		line.SetPosition(1, con.node.transform.localPosition);
		//	}
	}

	public void ZoomTexts(float scaleFactor)
	{
		foreach (var text in texts)
		{
			Vector3 localScale = text.localScale;
			Vector3 newScale = new Vector3(localScale.x / scaleFactor, localScale.y / scaleFactor, localScale.z);

			text.localScale = newScale;
		}
	}



	private void DrawPath(int[] path, Color color)
	{
		// TODO: проверить отрисовку пути
		for (int i = 0; i < path.Length - 1; i++)
		{
			var fromPos = nodes.Find(x => x.ID == path[i]).transform.localPosition;
			var toPos = nodes.Find(x => x.ID == path[i + 1]).transform.localPosition;

			var line = DrawLine(fromPos, toPos, color, color);
			finalPathLines.Enqueue(line);
		}
	}

	private void ClearFinalPath()
	{
		while (finalPathLines.Count > 0)
			Destroy(finalPathLines.Dequeue().gameObject);
	}



	#region Drawing Lines
	private LineRenderer GetInstantiatedLine()
	{
		return Instantiate(lineInfo.LinePrefab, linesParent);
	}
	private LineRenderer DrawLine(Vector3 from, Vector3 to, Color startColor, Color endColor)
	{
		var line = GetInstantiatedLine();
		line.startColor = startColor;
		line.endColor = endColor;

		line.SetPosition(0, from);
		line.SetPosition(1, to);

		return line;
	}
	private LineRenderer DrawLine(Vector3 from, Vector3 to)
	{
		var line = GetInstantiatedLine();

		line.SetPosition(0, from);
		line.SetPosition(1, to);

		return line;
	}
	private void DrawLineText(LineRenderer line, Node firstNode, Node secondNode)
	{
		// Weight text
		var lineText = Instantiate(lineInfo.LineTextPrefab, line.transform);
		texts.Add(lineText.transform);

		var firstPos = firstNode.transform.localPosition;
		var secondPos = secondNode.transform.localPosition;

		Vector2 normale = secondPos - firstPos;
		normale = new Vector2(-normale.y, normale.x); // Разворот на 90* против часовой

		Vector2 textPos = new Vector2((firstPos.x + secondPos.x) / 2, (firstPos.y + secondPos.y) / 2);
		textPos += normale.normalized * lineInfo.TextUpOffset;

		float rotation = Mathf.Atan((secondPos.y - firstPos.y) / (secondPos.x - firstPos.x)) * Mathf.Rad2Deg;
		lineText.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

		lineText.transform.localPosition = new Vector3(textPos.x, textPos.y, lineText.transform.localPosition.z);
		lineText.text = firstNode.Connections.First(x => x.node.ID == secondNode.ID).weight.ToString();
		lineText.enabled = true;
	}
	#endregion



	public void FindBestPath(FindBestPathDelegate algorithm)
	{
		if (algorithm == null)
			return;

		ClearFinalPath();
		var path = algorithm(Matrix, StartNode.ID, EndNode.ID);

		if (ValidateAndPrintPath(path))
			Debug.Log("Поиск лучшего маршрута завершен!");
	}
	private bool ValidateAndPrintPath(int[] path)
	{
		if (ValidateStartEndNodes() && ValidatePath(path))
		{
			DrawPath(path, Color.yellow);

			// Debug log
			string res = "";
			foreach (var p in path)
				res += p + " ";

			Debug.Log(res);
			return true;
		}
		else
		{
			Debug.LogWarning("Ошибка пути.");

			// Debug logWarning
			string str = "";
			foreach (var p in path)
				str += p + " ";

			Debug.LogWarning(str);
			return false;
		}
	}

	public void FindAllPaths(FindAllPathsDelegate algorithm)
	{
		if (algorithm == null)
			return;

		var paths = algorithm(Matrix, 0, 0);

		// TODO: Поиск всех маршрутов
		Debug.Log("Поиск всех маршрутов");
	}

	public void StartAlgorithmTeaching(AlgorithmTeaching algorithm)
	{
		if (algorithm == null)
			return;

		ClearFinalPath();
		ui.ShowAlgorithmTeachingPanel();

		algorithmTeachingRoutine = StartCoroutine(AlgorithmTeaching(algorithm));
	}



	/// <summary>
	/// Останавливает процесс "обучения", и если <paramref name="findBestPathAlgorithm"/> не null, показывает результат работы алгоритма.
	/// </summary>
	public void StopAlgorithmTeaching(FindBestPathDelegate findBestPathAlgorithm)
	{
		StopCoroutine(algorithmTeachingRoutine);

		foreach (var tuple in subscribedNodes)
		{
			var node = tuple.Item1;
			var method = tuple.Item2;

			node.OnConnectedWith -= method;
		}

		FinaleAlgorithmTeachng(findBestPathAlgorithm);
	}

	private void FinaleAlgorithmTeachng(FindBestPathDelegate findBestPathAlgorithm)
	{
		if (findBestPathAlgorithm != null)
			FindBestPath(findBestPathAlgorithm);

		ui.HideAlgorithmTeachingPanel();
	}



	private IEnumerator AlgorithmTeaching(AlgorithmTeaching algorithm)
	{
		object dataToSave = null;
		string message = null;

		var connectionsQueue = new Queue<ValueTuple<int, int>>();

		while (true)
		{
			if (teachingCanGo)
			{
				// TODO: завершает работу после проверки 1 ноды
				var path = algorithm((int[,])Matrix.Clone(), StartNode.ID, EndNode.ID, ref dataToSave, out message, out bool isAlgorithmFinished, out int nodeToHighlight);

				if (isAlgorithmFinished)
				{
					ValidateAndPrintPath(path);
					ui.HideAlgorithmTeachingPanel();
					break;
				}

				HighlightNodeAs_Middle(nodeToHighlight);
				for (int nodeID = 1; nodeID < path.Length; nodeID++)
				{
					connectionsQueue.Enqueue(ValueTuple.Create(path[nodeID - 1], path[nodeID]));
				}
				Debug.Log(message);



				foreach (var item in path)
					Debug.Log(item);



				MakePlayerConnectNodes(connectionsQueue);
			}

			yield return new WaitForEndOfFrame();
		}



		// Завершение работы
		RemoveNodeHighlighting(highlightedConnectionStartNode);
		RemoveNodeHighlighting(highlightedConnectionEndNode);
		RemoveNodeHighlighting(highlightedMiddleNode);

		if (!string.IsNullOrWhiteSpace(message))
			Debug.Log(message); // TODO: вывод на label
		else
			Debug.Log("else: Работа алгоритма завершена."); // TODO: вывод на label
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
			// TODO: дебажить в лейбел?
			teachingCanGo = true;
			return;
		}

		HighlightNodeAs_ConnectionStart(fromNode);
		HighlightNodeAs_ConnectionEnd(toNode);



		fromNode.OnConnectedWith += CheckIfNodeIsRight;
		subscribedNodes.Add((fromNode, CheckIfNodeIsRight));

		void CheckIfNodeIsRight(Node conNode)
		{
			if (conNode != null && conNode.ID == toNodeID)
			{
				teachingCanGo = true;
				fromNode.OnConnectedWith -= CheckIfNodeIsRight;
				subscribedNodes.Remove((fromNode, CheckIfNodeIsRight));

				// TODO: clear connection made by player

				MakePlayerConnectNodes(connectionsQueue);
			}
		}
	}



	#region NodeHighlighting
	private void HighlightNodeAs(Node nodeToHighlight, ref Node asWhatNode, Color color)
	{
		RemoveNodeHighlighting(asWhatNode);

		if (nodeToHighlight != null)
		{
			nodeToHighlight.Highlight(color);
			asWhatNode = nodeToHighlight;
		}
	}

	private void HighlightNodeAs_ConnectionStart(Node nodeToHighlight)
	{
		HighlightNodeAs(nodeToHighlight, ref highlightedConnectionStartNode, ConnectionStart_NodeColor);
	}
	private void HighlightNodeAs_ConnectionEnd(Node nodeToHighlight)
	{
		HighlightNodeAs(nodeToHighlight, ref highlightedConnectionEndNode, ConnectionEnd_NodeColor);
	}
	private void HighlightNodeAs_Middle(Node nodeToHighlight)
	{
		HighlightNodeAs(nodeToHighlight, ref highlightedMiddleNode, HighlightMiddle_NodeColor);
	}
	private void HighlightNodeAs_Middle(int nodeToHighlightID)
	{
		if (nodeToHighlightID >= 0)
			HighlightNodeAs_Middle(nodes.Find(x => x.ID == nodeToHighlightID));
	}

	private void RemoveNodeHighlighting(Node nodeToRemoveHighlightingFrom)
	{
		if (nodeToRemoveHighlightingFrom != null)
			nodeToRemoveHighlightingFrom.RemoveHighlighting();
	}
	#endregion



	private bool ValidateStartEndNodes(string senderName = "")
	{
		bool validated = true;

		if (StartNode == null)
		{
			Debug.LogWarning(senderName + " | StartNode is null.");
			validated = false;
		}

		if (EndNode == null)
		{
			Debug.LogWarning(senderName + " | EndNode is null.");
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






	//private struct TwoNodes
	//{
	//	public Node node1;
	//	public Node node2;

	//	public TwoNodes(Node node1, Node node2)
	//	{
	//		this.node1 = node1;
	//		this.node2 = node2;
	//	}
	//}
}
