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
	//[SerializeField] private LineRenderer linePrefab;
	[SerializeField] private LineInfo lineInfo;
	[SerializeField] private Node startNode;
	[SerializeField] private Node endNode;

	private List<Node> nodes;
	private List<Transform> texts;



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


	//Node currentNode;
	//Node CurrentNode
	//{
	//	get => currentNode;
	//	set { }
	//}



	void Awake()
	{
		nodes = new List<Node>();
		texts = new List<Transform>();

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
					var line = Instantiate(lineInfo.LinePrefab, transform);

					var firstNode = nodes.Find(x => x.ID == i);
					var secondNode = nodes.Find(x => x.ID == j);

					line.SetPosition(0, firstNode.transform.localPosition);
					line.SetPosition(1, secondNode.transform.localPosition);

					DrawLineText(line, firstNode, secondNode);
				}



		//// old не учитывает, что связи двусторонние
		//foreach (var node in nodes)
		//	foreach (var con in node.Connections)
		//	{
		//		var line = Instantiate(linePrefab, transform);
		//		line.SetPosition(0, node.transform.localPosition);
		//		line.SetPosition(1, con.node.transform.localPosition);
		//	}
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


	public void ZoomTexts(float scaleFactor)
	{
		foreach (var text in texts)
		{
			Vector3 localScale = text.localScale;
			Vector3 newScale = new Vector3(localScale.x / scaleFactor, localScale.y / scaleFactor, localScale.z);

			text.localScale = newScale;
		}
	}


	private void DrawPath(int[] path)
	{
		// TODO: отрисовка пути
	}



	public void FindBestPath(FindBestPathDelegate algorithm)
	{
		var path = algorithm(Matrix, StartNode.ID, EndNode.ID);

		if (ValidateStartEndNodes() && ValidatePath(path))
		{
			DrawPath(path);

			// Debug log
			string res = "";
			foreach (var p in path)
				res += p + " ";

			Debug.Log(res);
		}
		else
		{
			Debug.LogWarning("Ошибка пути.");
			string str = "";
			foreach (var p in path)
				str += p + " ";

			Debug.LogWarning(str);

			return;
		}



		Debug.Log("Поиск лучшего маршрута завершен!");
	}

	public void FindAllPaths(FindAllPathsDelegate algorithm)
	{
		var paths = algorithm(Matrix, 0, 0);

		// TODO: Поиск всех маршрутов
		Debug.Log("Поиск всех маршрутов");
	}

	public void AlgorithmTeaching(AlgorithmTeaching algorithm)
	{
		// TODO: algorithmStep
	}

	private bool ValidateStartEndNodes()
	{
		bool validated = true;

		if (StartNode == null)
		{
			Debug.LogWarning("StartNode is null.");
			validated = false;
		}

		if (EndNode == null)
		{
			Debug.LogWarning("EndNode is null.");
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
		// TODO: переделать getMatrix?

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
