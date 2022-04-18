using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class GraphMover : MonoBehaviour, IDragHandler
{
	[SerializeField] private Camera cam;
	[SerializeField] public Canvas canvas;
	[SerializeField] private RectTransform dragZone;
	[SerializeField]
	private float
		zoomStep = 10f,
		minZoomSize = 100f,
		maxZoomSize = 300f;

	private RectTransform canvasRectTransform;
	private List<Node> nodes;
	private Graph graph;
	private InputMaster input;
	private RectTransform rt;
	private Vector2 zoomCenter;



	void Awake()
	{
		graph = GetComponent<Graph>();
		rt = GetComponent<RectTransform>();

		// Поиск всех детей
		nodes = new List<Node>();

		var children = GetComponentsInChildren<Node>();
		nodes.AddRange(children);

		input = new InputMaster();
		input.UI.ScrollWheel.performed += _ => Zoom(input.UI.ScrollWheel.ReadValue<Vector2>().y);
	}
	void Start()
	{
		canvasRectTransform = canvas.GetComponent<RectTransform>();
		zoomCenter = dragZone.anchoredPosition + (dragZone.rect.size - canvasRectTransform.rect.size) / 2;

		transform.localPosition = ClampPosition(transform.localPosition);
	}
	void OnEnable()
	{
		input.UI.Enable();
	}
	void OnDisable()
	{
		input.UI.Disable();
	}



	private Vector2 GetScreenSizeOfRT()
	{
		var rect = rt.rect;
		var scale = rt.localScale;

		var x = rect.width * scale.x;
		var y = rect.height * scale.y;

		return new Vector2(x, y);
	}

	private void CenterRtIfNeeded()
	{
		var rtScreenSize = GetScreenSizeOfRT();
		var dragZoneSize = dragZone.rect.size;

		// Отцентровка по X
		if (rtScreenSize.x < dragZoneSize.x)
		{
			var rtPos = rt.localPosition;
			float x = zoomCenter.x;

			rt.localPosition = new Vector3(x, rtPos.y, rtPos.z);
		}

		// Отцентровка по Y
		if (rtScreenSize.y < dragZoneSize.y)
		{
			var rtPos = rt.localPosition;
			float y = zoomCenter.y;

			rt.localPosition = new Vector3(rtPos.x, y, rtPos.z);
		}
	}



	#region Drag
	public void OnDrag(PointerEventData eventData)
	{
		rt.anchoredPosition += eventData.delta / canvas.scaleFactor;

		rt.localPosition = ClampPosition(rt.localPosition);
		CenterRtIfNeeded();
	}

	private Vector3 ClampPosition(Vector3 position)
	{
		Vector2 maxPos = dragZone.anchoredPosition;
		Vector2 minPos = maxPos + dragZone.rect.size - canvasRectTransform.rect.size;

		// Увеличиваем max и min координаты соответсвенно размеру объекта
		Vector2 add = rt.rect.size / 2 * ((Vector2)transform.localScale - Vector2.one);
		minPos.x += add.x * Mathf.Sign(minPos.x);
		minPos.y += add.y * Mathf.Sign(minPos.y);
		maxPos.x += add.x * Mathf.Sign(maxPos.x);
		maxPos.y += add.y * Mathf.Sign(maxPos.y);

		float x = Mathf.Clamp(position.x, minPos.x, maxPos.x);
		float y = Mathf.Clamp(position.y, minPos.y, maxPos.y);

		return new Vector3(x, y, position.z);
	}
	#endregion



	#region Zoom
	public void Zoom(float sign)
	{
		if (sign == 0 || CursorRaycaster.IsCursorOverObjectWithTag(GetRawCursorPos(), "LeftUIPanel", out _))
			return;

		float size = Mathf.Clamp(transform.localScale.x + zoomStep * sign, minZoomSize, maxZoomSize);
		if (size == transform.localScale.x)
			return;

		ZoomNodes(size / transform.localScale.x);
		ZoomAround(zoomCenter + GetCursorOffset(), size);

		transform.localPosition = ClampPosition(transform.localPosition);
		CenterRtIfNeeded();
	}

	private void ZoomAround(Vector2 point, float size)
	{
		// Сохраняем предыдущее состояние
		float oldScale = transform.localScale.x;
		Vector2 oldPos = (Vector2)transform.localPosition;

		// Двигаем в точку, вокруг которой скейлим
		transform.localPosition = point;
		transform.localScale = new Vector3(size, size, 1f);

		// Двигаем обратно
		Vector2 diff = point - oldPos;
		float scaleFactor = size / oldScale;

		transform.localPosition -= (Vector3)diff * scaleFactor;
	}

	private void ZoomNodes(float scaleFactor)
	{
		foreach (var node in nodes)
		{
			Vector3 localScale = node.transform.localScale;
			Vector3 newScale = new Vector3(localScale.x / scaleFactor, localScale.y / scaleFactor, localScale.z);
			node.transform.localScale = newScale;

			//node.Zoom(scaleFactor);
		}

		graph.ZoomTexts(scaleFactor);
	}
	#endregion



	#region Cursor
	private Vector2 GetCursorOffset()
	{
		var cursorPos = GetRawCursorPos();
		if (!CursorRaycaster.IsCursorOverObjectWithTag(cursorPos, "DragZone", out _))
			return Vector2.zero;

		cursorPos /= canvas.scaleFactor;
		cursorPos -= canvasRectTransform.rect.size / 2; // Перемещение нуля в центр

		return cursorPos - zoomCenter;
	}
	private Vector2 GetRawCursorPos()
	{
		return input.UI.Position.ReadValue<Vector2>();
	}
	#endregion
}
