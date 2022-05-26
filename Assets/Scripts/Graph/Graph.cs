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
	private AlgorithmTeacher algorithmTeacher;

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

		algorithmTeacher = new AlgorithmTeacher(this, outputGraph, panelMover, nodeColors);
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

	public void ClearAllManualConnections()
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



	/// <summary> Ищет и показывает лучший маршрут. </summary>
	public void FindBestPath(FindBestPathDelegate algorithm)
	{
		if (algorithm == null)
			return;

		ClearFinalPath();

		float startTime = Time.realtimeSinceStartup;
		var path = algorithm(Matrix, startNode.ID, endNode.ID);

		ScreenDebug.ShowTime(((Time.realtimeSinceStartup - startTime) * 1000f).ToString("f2"));

		ValidateAndPrintPath(path);
	}



	/// <summary>
	/// Начинает процесс "обучения". Если <paramref name="findBestPathDelegate"/> будет не null, появится возможность увидеть результ работы алгоритма после остановки обучения.
	/// </summary>
	public void StartAlgorithmTeaching(AlgorithmTeaching algorithm, FindBestPathDelegate findBestPathDelegate)
	{
		if (algorithm == null)
			return;

		ScreenDebug.ClearTime();

		ClearFinalPath();
		panelMover.ShowAlgorithmTeachingPanel();

		if (algorithmTeachingRoutine != null)
			StopCoroutine(algorithmTeachingRoutine);

		algorithmTeachingRoutine = StartCoroutine(algorithmTeacher.AlgorithmTeaching(algorithm, Matrix, startNode, endNode, findBestPathDelegate));
	}



	/// <summary>
	/// Останавливает процесс "обучения", и если переданный ранее алгоритм поиска кратчайшего пути не null, показывает результат работы алгоритма.
	/// </summary>
	public void StopAlgorithmTeaching(bool findBestPath)
	{
		if (algorithmTeachingRoutine != null)
			StopCoroutine(algorithmTeachingRoutine);

		algorithmTeacher.StopAlgorithmTeaching(findBestPath);
	}



	public bool ValidateAndPrintPath(int[] path)
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

	public bool ValidateStartEndNodes(string senderName = "")
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
