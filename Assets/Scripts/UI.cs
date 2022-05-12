using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField] private DragZone dragZone;
	[SerializeField] private GameObject lowerLeftPanel;
	[SerializeField] private GameObject upLeftPanel;
	[SerializeField] private GameObject upPanel;
	[SerializeField] private float moveSpeed;

	private RectTransform upLeftPanelRT;
	private RectTransform lowerLeftPanelRT;
	private RectTransform upPanelRT;

	private Coroutine upLeftPanelRoutine;
	private Coroutine upPanelRoutine;

	private bool upLeftPanelShown;
	private bool upPanelShown;

	private bool upLeftPanelDragZoneIsSmall;
	private bool upPanelDragZoneIsSmall;



	void Awake()
	{
		upLeftPanelRT = upLeftPanel.GetComponent<RectTransform>();
		lowerLeftPanelRT = lowerLeftPanel.GetComponent<RectTransform>();
		upPanelRT = upPanel.GetComponent<RectTransform>();

		input = new InputMaster();
		input.UI.T.performed += _ => ShowAlgorithmTeachingPanel();
		input.UI.Y.performed += _ => HideAlgorithmTeachingPanel();
	}

	InputMaster input;
	void OnEnable()
	{
		input.Enable();
	}
	void OnDisable()
	{
		input.Disable();
	}



	public void ShowAlgorithmTeachingPanel()
	{
		ShowUpLeftPanel(true);
		ShowUpPanel(true);
	}
	public void HideAlgorithmTeachingPanel()
	{
		ShowUpLeftPanel(false);
		ShowUpPanel(false);
	}



	private void ShowUpLeftPanel(bool state)
	{
		if (upLeftPanelShown == state)
			return;

		upLeftPanelShown = state;

		if (upLeftPanelRoutine != null)
			StopCoroutine(upLeftPanelRoutine);



		// Показать
		if (state)
		{
			upLeftPanel.SetActive(true);

			upLeftPanelRoutine = StartCoroutine(MovePanel(upLeftPanelRT, Vector2.zero, () =>
			{
				lowerLeftPanel.SetActive(false);

				dragZone.ChangeSize(DragZone.ChangeSizeSide.Left, lowerLeftPanelRT.sizeDelta.x - upLeftPanelRT.sizeDelta.x);
				upLeftPanelDragZoneIsSmall = true;
			}));
		}

		// Скрыть
		else
		{
			lowerLeftPanel.SetActive(true);

			if (upLeftPanelDragZoneIsSmall)
			{
				// Увеличить Драг Зону
				dragZone.ChangeSize(DragZone.ChangeSizeSide.Left, upLeftPanelRT.sizeDelta.x - lowerLeftPanelRT.sizeDelta.x);
				upLeftPanelDragZoneIsSmall = false;
			}

			upLeftPanelRoutine = StartCoroutine(MovePanel(upLeftPanelRT, new Vector2(-upLeftPanelRT.rect.width, 0f), () =>
			{
				upLeftPanel.SetActive(false);
			}));
		}
	}

	private void ShowUpPanel(bool state)
	{
		if (upPanelShown == state)
			return;

		upPanelShown = state;

		if (upPanelRoutine != null)
			StopCoroutine(upPanelRoutine);



		// Показать
		if (state)
		{
			upPanel.SetActive(true);

			upPanelRoutine = StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, 0f), () =>
			{
				dragZone.ChangeSize(DragZone.ChangeSizeSide.Top, -upPanelRT.rect.height);
				upPanelDragZoneIsSmall = true;
			}));
		}

		// Скрыть
		else
		{
			if (upPanelDragZoneIsSmall)
			{
				// Увеличить Драг Зону
				dragZone.ChangeSize(DragZone.ChangeSizeSide.Top, upPanelRT.rect.height);
				upPanelDragZoneIsSmall = false;
			}

			upPanelRoutine = StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, upPanelRT.rect.height), () =>
			{
				upPanel.SetActive(false);
			}));
		}
	}



	private IEnumerator MovePanel(RectTransform panel, Vector2 targetPos, Action callback)
	{
		if (moveSpeed == 0f)
			yield break;

		while (panel.anchoredPosition != targetPos)
		{
			panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, moveSpeed * Time.deltaTime);
			if ((panel.anchoredPosition - targetPos).magnitude < 0.06f)
				panel.anchoredPosition = targetPos;

			yield return new WaitForEndOfFrame();
		}

		callback?.Invoke();
	}

	//private void MoveDragZone(Vector2 panelPos, Vector2 prevPanelPos, DragZone.ChangeSizeSide side, Vector2 stopMovingPosition)
	//{
	//	float moveDirection = Convert.ToSingle(side == DragZone.ChangeSizeSide.Right_Or_Top) * 2 - 1;

	//	Vector2 diff = panelPos - prevPanelPos;

	//	// X
	//	if (panelPos.x > stopMovingPosition.x)
	//	{
	//		if (prevPanelPos.x < stopMovingPosition.x)
	//			diff.x = panelPos.x - stopMovingPosition.x;

	//		dragZone.ChangeSize(DragZone.SizeAxis.X, side, diff.x * moveDirection);
	//	}
	//	else if (prevPanelPos.x > stopMovingPosition.x)
	//	{
	//		diff.x = stopMovingPosition.x - prevPanelPos.x;

	//		dragZone.ChangeSize(DragZone.SizeAxis.X, side, diff.x * moveDirection);
	//	}

	//	// Y
	//	if (panelPos.y > stopMovingPosition.y)
	//	{
	//		if (prevPanelPos.y < stopMovingPosition.y)
	//			diff.y = panelPos.y - stopMovingPosition.y;

	//		dragZone.ChangeSize(DragZone.SizeAxis.Y, side, diff.y * moveDirection);
	//	}
	//	else if (prevPanelPos.y > stopMovingPosition.y)
	//	{
	//		diff.y = stopMovingPosition.y - prevPanelPos.y;

	//		dragZone.ChangeSize(DragZone.SizeAxis.Y, side, diff.y * moveDirection);
	//	}
	//}	
}
