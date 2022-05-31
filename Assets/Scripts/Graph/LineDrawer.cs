using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class LineDrawer : MonoBehaviour
{
	[SerializeField] private LineInfo lineInfo;
	[SerializeField] private Transform linesParent;

	[HideInInspector] public readonly List<Transform> texts = new List<Transform>();



	private LineRenderer GetInstantiatedLine()
	{
		return Instantiate(lineInfo.LinePrefab, linesParent);
	}

	public LineRenderer DrawLine(Vector3 from, Vector3 to, Color startColor, Color endColor)
	{
		var line = GetInstantiatedLine();
		line.startColor = startColor;
		line.endColor = endColor;

		line.SetPosition(0, from);
		line.SetPosition(1, to);

		return line;
	}

	public LineRenderer DrawLine(Vector3 from, Vector3 to)
	{
		var line = GetInstantiatedLine();

		line.SetPosition(0, from);
		line.SetPosition(1, to);

		return line;
	}

	public void DrawLineWithText(Node firstNode, Node secondNode)
	{
		var line = DrawLine(firstNode.transform.localPosition, secondNode.transform.localPosition);

		DrawLineText(line, firstNode, secondNode);
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
}
