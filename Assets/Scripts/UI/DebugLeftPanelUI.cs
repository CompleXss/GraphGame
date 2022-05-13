using System;
using UnityEngine;

public class DebugLeftPanelUI : MonoBehaviour
{
	[SerializeField] private float moveSpeed;

	private RectTransform rectTransform;
	private Coroutine routine;



	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}



	public void OpenDebugMenu()
	{
		this.gameObject.SetActive(true);
		MoveDebugMenu(Vector2.zero);
	}
	public void CloseDebugMenu()
	{
		MoveDebugMenu(new Vector2(-rectTransform.rect.size.x, 0f), () => this.gameObject.SetActive(false));
	}

	private void MoveDebugMenu(Vector2 position, Action callback = null)
	{
		if (routine != null)
			StopCoroutine(routine);

		routine = StartCoroutine(UI.MovePanel(rectTransform, position, moveSpeed, callback));
	}
}
