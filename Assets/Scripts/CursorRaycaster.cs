using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public static class CursorRaycaster
{
	public static List<RaycastResult> GetRaycastResults(Vector2 cursorPos)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
		{
			position = cursorPos
		};

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, raycastResults);

		return raycastResults;
	}

	/// <summary> Note that "out Gameobject" is null if this method returns false. </summary>
	public static bool IsCursorOverObjectWithTag(List<RaycastResult> raycastResults, string tag, out GameObject obj)
	{
		foreach (var result in raycastResults)
			if (result.gameObject.CompareTag(tag))
			{
				obj = result.gameObject;
				return true;
			}

		obj = null;
		return false;
	}

	/// <summary> Note that "out Gameobject" is null if this method returns false. </summary>
	public static bool IsCursorOverObjectWithTag(Vector2 cursorPos, string tag, out GameObject obj)
	{
		var raycastResults = GetRaycastResults(cursorPos);

		return IsCursorOverObjectWithTag(raycastResults, tag, out obj);
	}
}
