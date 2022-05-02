using System;
using System.Collections;
using System.Collections.Generic;

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
	}
	void OnDisable()
	{
		Destroy(emptyObj);
	}



	public void ChangeSize(SizeAxis axis, ChangeSizeSide side, float offset)
	{
		if (offset == 0f)
			return;

		var vec = new Vector2(
			Convert.ToSingle(axis == SizeAxis.X) * offset,
			Convert.ToSingle(axis == SizeAxis.Y) * offset);



		if (side == ChangeSizeSide.Left_Or_Bottom)
			ChangePosAndSize(-vec, vec);
		else
			ChangeSize(vec);
	}



	private void SetNewSize(Vector2 newSize)
	{
		var oldParent = graph.parent;
		graph.parent = emptyObj.transform;

		rt.sizeDelta = newSize;

		graph.parent = oldParent;
	}
	private void ChangeSize(Vector2 offset)
	{
		SetNewSize(rt.sizeDelta + offset);
	}



	private void SetNewPosAndSize(Vector2 newPos, Vector2 newSize)
	{
		var oldParent = graph.parent;
		graph.parent = emptyObj.transform;

		rt.anchoredPosition = newPos;
		rt.sizeDelta = newSize;

		graph.parent = oldParent;
	}
	private void ChangePosAndSize(Vector2 posOffset, Vector2 sizeOffset)
	{
		SetNewPosAndSize(rt.anchoredPosition + posOffset, rt.sizeDelta + sizeOffset);
	}




	public enum SizeAxis
	{
		X, Y
	}
	public enum ChangeSizeSide
	{
		Left_Or_Bottom,
		Right_Or_Top
	}
}
