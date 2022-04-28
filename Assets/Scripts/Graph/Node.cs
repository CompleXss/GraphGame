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
	[SerializeField] private List<Connection> connections;
	[SerializeField] private LineRenderer chooseLinePrefab;
	//[SerializeField] private LineInfo chooseLineInfo;



	// Public
	public int ID { get; private set; }
	public BindingList<Connection> Connections { get; private set; }
	public Node ConnectedTo { get; private set; }

	// Private variables
	private static int objectsCount = 0;
	private InputMaster input;
	private Transform parent;
	private LineRenderer line;
	//private TextMeshProUGUI lineText;
	private Canvas canvas;
	private RectTransform canvasRectTransform;
	private bool isHolding;



	void Awake()
	{
		ID = objectsCount++;
		gameObject.name = "Node " + ID;

		input = new InputMaster();
		parent = gameObject.GetComponentInParent<Graph>().transform;
		canvas = GetComponentInParent<GraphMover>().canvas;
		canvasRectTransform = canvas.GetComponent<RectTransform>();

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

		if (CursorRaycaster.IsCursorOverObjectWithTag(GetRawCursorPos(), "GraphNode", out GameObject obj)
			&& obj != this.gameObject)
		{
			conNode = obj.GetComponent<Node>();

			if (this.Connections.Any(x => x.node.ID == conNode.ID) // if can connect
				&& conNode.ConnectedTo != this)
			{
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

	private void Hold()
	{
		// If is still holding
		if (isHolding)
		{
			Debug.Log("Hold is Done");
		}

		isHolding = false;
	}



	public void OnPointerDown(PointerEventData eventData)
	{
		StartDrawingLine();
		ConnectedTo = null;

		//isHolding = true;
		//Invoke(nameof(Hold), 1.5f);
	}

	public void OnDrag(PointerEventData eventData)
	{
		line.SetPosition(1, GetCursorPos());
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		StopDrawingLine(out Node connectedTo);
		ConnectedTo = connectedTo;

		//isHolding = false;
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
	void OnDrawGizmosSelected()
	{
		foreach (var con in connections)
			Gizmos.DrawLine(transform.position, con.node.transform.position);
	}



	#region Cursor
	private Vector2 GetCursorPos()
	{
		var cursorPos = GetRawCursorPos();
		cursorPos /= canvas.scaleFactor;

		var rectSize = canvasRectTransform.rect.size;
		cursorPos -= rectSize / 2;

		cursorPos -= (Vector2)parent.localPosition;
		cursorPos /= parent.localScale;

		return cursorPos;
	}

	private Vector2 GetRawCursorPos()
	{
		return input.UI.Position.ReadValue<Vector2>();
	}
	#endregion
}
