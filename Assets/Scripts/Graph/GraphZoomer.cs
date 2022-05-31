using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineDrawer))]
public class GraphZoomer : MonoBehaviour
{
	public Canvas canvas; // TODO: private ?
	[SerializeField] private RectTransform dragZone;
	[SerializeField]
	private float
		zoomStep = 0.1f,
		minZoomSize = 0.4f,
		maxZoomSize = 3f;

	private List<Node> nodes;
	private LineDrawer lineDrawer;
	private InputMaster input;
	private Vector2 zoomCenter;



	void Awake()
	{
		lineDrawer = GetComponent<LineDrawer>();

		// Поиск всех детей
		nodes = new List<Node>();

		var children = GetComponentsInChildren<Node>();
		nodes.AddRange(children);

		input = new InputMaster();
		input.UI.ScrollWheel.performed += _ => Zoom(input.UI.ScrollWheel.ReadValue<Vector2>().y);
	}
	void Start()
	{
		zoomCenter = dragZone.sizeDelta / 2;
	}
	void OnEnable()
	{
		input.UI.Enable();
	}
	void OnDisable()
	{
		input.UI.Disable();
	}



	#region Drag
	//public void OnDrag(PointerEventData eventData)
	//{
	//	//rt.anchoredPosition += eventData.delta / canvas.scaleFactor;

	//	//rt.localPosition = ClampPosition(rt.localPosition);
	//	//CenterRtIfNeeded();
	//}

	//private Vector3 ClampPosition(Vector3 position)
	//{
	//	Vector2 maxPos = dragZone.anchoredPosition;
	//	Vector2 minPos = maxPos + dragZone.rect.size - canvasRectTransform.rect.size;

	//	// Увеличиваем max и min координаты соответсвенно размеру объекта
	//	Vector2 add = rt.rect.size / 2 * ((Vector2)transform.localScale - Vector2.one);
	//	minPos.x += add.x * Mathf.Sign(minPos.x);
	//	minPos.y += add.y * Mathf.Sign(minPos.y);
	//	maxPos.x += add.x * Mathf.Sign(maxPos.x);
	//	maxPos.y += add.y * Mathf.Sign(maxPos.y);

	//	float x = Mathf.Clamp(position.x, minPos.x, maxPos.x);
	//	float y = Mathf.Clamp(position.y, minPos.y, maxPos.y);

	//	return new Vector3(x, y, position.z);
	//}
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
		}

		ZoomTexts(scaleFactor);
	}

	private void ZoomTexts(float scaleFactor)
	{
		foreach (var text in lineDrawer.texts)
		{
			Vector3 localScale = text.localScale;
			Vector3 newScale = new Vector3(localScale.x / scaleFactor, localScale.y / scaleFactor, localScale.z);

			text.localScale = newScale;
		}
	}
	#endregion



	#region Cursor
	private Vector2 GetCursorOffset()
	{
		var cursorPos = GetRawCursorPos();
		if (!CursorRaycaster.IsCursorOverObjectWithTag(cursorPos, "DragZone", out _))
			return Vector2.zero;

		return GetCursorPos(cursorPos) - zoomCenter;
	}

	public Vector2 GetCursorPos()
	{
		var rawCursorPos = GetRawCursorPos();

		return GetCursorPos(rawCursorPos);
	}
	public Vector2 GetCursorPos(Vector2 rawCursorPos)
	{
		ref var cursorPos = ref rawCursorPos;

		cursorPos /= canvas.scaleFactor;
		cursorPos -= dragZone.anchoredPosition; // Переход к локальным кордам dragZone'ы

		return cursorPos;
	}

	public Vector2 GetRawCursorPos()
	{
		return input.UI.Position.ReadValue<Vector2>();
	}
	#endregion
}
