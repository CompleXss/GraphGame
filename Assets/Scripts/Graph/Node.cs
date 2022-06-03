using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField] private List<Connection> connections;
	[SerializeField] private LineRenderer chooseLinePrefab;

	[SerializeField] private SpriteRenderer circle;
	[SerializeField] private SpriteRenderer ring;
	[SerializeField] private TextMeshProUGUI IDBox;



	private int id;
	public int ID
	{
		get => id;
		private set
		{
			id = value;
			gameObject.name = "Node " + id;
			IDBox.text = id.ToString();
		}
	}

	// Public
	/// <summary> Are initialized in Awake. Get it after Awake only! </summary>
	public List<Connection> Connections => connections;
	public RectTransform RectTransform { get; private set; }
	public Node ConnectedTo { get; private set; }
	public event Action<Node> OnConnectedWith;



	// Private variables	
	private GraphZoomer graphMover;
	private static int objectsCount = 0;
	private Transform parent;
	private LineRenderer line;
	private bool isHighlighted;
	private bool isIdInitialized;



	void Awake()
	{
		if (ID == 0)
			ID = objectsCount++;

		RectTransform = GetComponent<RectTransform>();

		parent = gameObject.GetComponentInParent<Graph>().transform;
		graphMover = GetComponentInParent<GraphZoomer>();

		// Init connections
		connections.RemoveAll(x => x.node == this);
	}

	/// <summary> При попытке вызвать повторно ничего не произойдет. </summary>
	public void InitID(int value)
	{
		if (isIdInitialized)
		{
			Debug.LogWarning($"Попытка установить NodeID второй раз | Текущий ID: {ID} | ID который хотели установить: {value}");
			return;
		}

		ID = value;
		isIdInitialized = true;
	}



	#region Drawing Line
	private void StartDrawingLine()
	{
		if (line == null)
			line = Instantiate(chooseLinePrefab, parent);

		line.SetPosition(0, transform.localPosition);
		line.SetPosition(1, transform.localPosition);

		line.enabled = true;

		//if (lineText != null)
		//	lineText.enabled = false;
	}

	private void StopDrawingLine(out Node connectedNode)
	{
		Node conNode;

		if (CursorRaycaster.IsCursorOverObjectWithTag(graphMover.GetRawCursorPos(), "GraphNode", out GameObject obj)
			&& obj != this.gameObject)
		{
			conNode = obj.GetComponent<Node>();

			if (this.Connections.Any(x => x.node.ID == conNode.ID)) // if can connect
			{
				if (conNode.ConnectedTo == this)
					conNode.ClearManualConnection();

				line.SetPosition(1, obj.transform.localPosition);
				connectedNode = conNode;

				//DrawLineText(conNode);
				return;
			}
		}

		// Else
		line.enabled = false;
		connectedNode = null;
	}
	#endregion

	public void ClearManualConnection()
	{
		if (line != null)
			line.enabled = false;

		ConnectedTo = null;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		StartDrawingLine();
		ConnectedTo = null;
	}

	public void OnDrag(PointerEventData eventData)
	{
		line.SetPosition(1, GetCursorPos());
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		StopDrawingLine(out Node connectedTo);
		ConnectedTo = connectedTo;

		OnConnectedWith?.Invoke(connectedTo);
	}



	public void MarkAs_StartNode(Color color)
	{
		ring.color = color;
	}
	public void MarkAs_EndNode(Color color)
	{
		ring.color = color;
	}
	public void UnMarkAs_StartEnd()
	{
		ring.color = Color.white;
	}



	public void Highlight(Color color)
	{
		if (circle.color == color)
			return;

		if (!isHighlighted)
		{
			ring.transform.localScale *= 1.1f;
			isHighlighted = true;
		}

		circle.color = color;
	}
	public void ClearHighlighting()
	{
		if (!isHighlighted)
			return;

		ring.transform.localScale /= 1.1f;
		circle.color = Color.white;

		isHighlighted = false;
	}
	public void ClearHighlighting(Color colorToRemove)
	{
		if (circle.color == colorToRemove)
			ClearHighlighting();
	}



	#region Cursor
	private Vector2 GetCursorPos()
	{
		var cursorPos = graphMover.GetCursorPos();
		cursorPos -= (Vector2)parent.localPosition;
		cursorPos /= parent.localScale;

		return cursorPos;
	}
	#endregion



	void OnDrawGizmosSelected()
	{
		foreach (var con in connections)
			Gizmos.DrawLine(transform.position, con.node.transform.position);
	}
}
