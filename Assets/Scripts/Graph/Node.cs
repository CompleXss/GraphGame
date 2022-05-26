using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Node : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField] private int id;
	[SerializeField] private List<Connection> connections;
	[SerializeField] private LineRenderer chooseLinePrefab;

	[SerializeField] private SpriteRenderer circle;
	[SerializeField] private SpriteRenderer ring;
	[SerializeField] private TextMeshProUGUI IDBox;



	// Public
	public int ID { get; private set; }
	public RectTransform RectTransform { get; private set; }
	public BindingList<Connection> Connections { get; private set; }
	public Node ConnectedTo { get; private set; }
	public event Action<Node> OnConnectedWith;



	// Private variables
	private GraphZoomer graphMover;
	//private static int objectsCount = 0;
	private InputMaster input;
	private Transform parent;
	private LineRenderer line;
	private bool isHighlighted;



	void Awake()
	{
		//ID = objectsCount++;
		ID = id;
		gameObject.name = "Node " + ID;
		IDBox.text = ID.ToString();

		RectTransform = GetComponent<RectTransform>();

		input = new InputMaster();
		parent = gameObject.GetComponentInParent<Graph>().transform;
		graphMover = GetComponentInParent<GraphZoomer>();

		// Init connections
		connections.RemoveAll(x => x.node == this);
		Connections = new BindingList<Connection>(connections);
	}
	void OnEnable()
	{
		input.UI.Enable();
	}
	void OnDisable()
	{
		input.UI.Disable();
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
	public void RemoveHighlighting(Color colorToRemove)
	{
		if (circle.color == colorToRemove)
			ClearHighlighting();
	}



	///// <summary> Zooms node itself and lineText, attached to it. </summary>
	//public void Zoom(float scaleFactor)
	//{
	//	// Zoom node itself
	//	Vector3 localScale = transform.localScale;
	//	Vector3 newScale = new Vector3(localScale.x / scaleFactor, localScale.y / scaleFactor, localScale.z);
	//	transform.localScale = newScale;

	//	//if (lineText == null)
	//	//	return;

	//	//// Zoom lineText
	//	//Vector3 textLocalScale = lineText.transform.localScale;
	//	//Vector3 newTextScale = new Vector3(textLocalScale.x / scaleFactor, textLocalScale.y / scaleFactor, textLocalScale.z);
	//	//lineText.transform.localScale = newTextScale;
	//}


	// Gizmos



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
