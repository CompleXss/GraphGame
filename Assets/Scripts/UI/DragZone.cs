using System;
using UnityEngine;

public class DragZone : MonoBehaviour
{
	[SerializeField] private Transform graph;

	private RectTransform rt;
	private GameObject emptyObj;

	void Awake()
	{
		rt = GetComponent<RectTransform>();
	}
	void OnEnable()
	{
		emptyObj = new GameObject("EmptyObject");
		emptyObj.transform.SetParent(transform.parent);
	}
	void OnDisable()
	{
		Destroy(emptyObj);
	}



	public void ChangeSize(ChangeSizeSide side, float offset)
	{
		if (offset == 0f)
			return;

		bool axis_X = (int)side % 2 == 1;

		var vec = new Vector2(
			Convert.ToSingle(axis_X) * offset,
			Convert.ToSingle(!axis_X) * offset);

		if (side == ChangeSizeSide.Left || side == ChangeSizeSide.Bottom)
			ChangePosAndSize(-vec, vec);
		else
			ChangeSize(vec);
	}



	private void SetNewSize(Vector2 newSize)
	{
		var oldParent = graph.parent;
		graph.SetParent(emptyObj.transform);

		rt.sizeDelta = newSize;

		graph.SetParent(oldParent);
	}
	private void ChangeSize(Vector2 offset)
	{
		SetNewSize(rt.sizeDelta + offset);
	}



	private void SetNewPosAndSize(Vector2 newPos, Vector2 newSize)
	{
		var oldParent = graph.parent;
		graph.SetParent(emptyObj.transform);

		rt.anchoredPosition = newPos;
		rt.sizeDelta = newSize;

		graph.SetParent(oldParent);
	}
	private void ChangePosAndSize(Vector2 posOffset, Vector2 sizeOffset)
	{
		SetNewPosAndSize(rt.anchoredPosition + posOffset, rt.sizeDelta + sizeOffset);
	}



	public enum ChangeSizeSide
	{
		// Порядок важен!
		Bottom, Left, Top, Right
	}
}
