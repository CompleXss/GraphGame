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
		StopAllCoroutines();

		// Ћева€ панель (верхн€€)
		upLeftPanel.SetActive(true);
		var leftRoutine = StartCoroutine(MoveDragZone(upLeftPanelRT, DragZone.ChangeSizeSide.Left_Or_Bottom, lowerLeftPanelRT.sizeDelta - upLeftPanelRT.sizeDelta));

		StartCoroutine(MovePanel(upLeftPanelRT, Vector2.zero, () =>
		{
			lowerLeftPanel.SetActive(false);
			StopCoroutine(leftRoutine);
		}));



		// ¬ерхн€€ панель
		upPanel.SetActive(true);
		//var upRoutine = StartCoroutine(MoveDragZone(upPanelRT, DragZone.ChangeSizeSide.Right_Or_Top, ));

		StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, 0f), null));
	}
	public void HideAlgorithmTeachingPanel()
	{
		StopAllCoroutines();

		// Ћева€ панель (верхн€€)
		lowerLeftPanel.SetActive(true);
		var leftRoutine = StartCoroutine(MoveDragZone(upLeftPanelRT, DragZone.ChangeSizeSide.Left_Or_Bottom, lowerLeftPanelRT.sizeDelta - upLeftPanelRT.sizeDelta));

		StartCoroutine(MovePanel(upLeftPanelRT, new Vector2(-upLeftPanelRT.rect.width, 0f), () =>
		{
			upLeftPanel.SetActive(false);
			StopCoroutine(leftRoutine);
		}));



		// ¬ерхн€€ панель
		StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, upPanelRT.rect.height), () =>
		{
			upPanel.SetActive(false);
		}));
	}



	private IEnumerator MovePanel(RectTransform panel, Vector2 targetPos, Action callback)
	{
		if (moveSpeed == 0f)
			yield break;

		while (panel.anchoredPosition != targetPos)
		{
			panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, moveSpeed * Time.deltaTime);
			if ((panel.anchoredPosition - targetPos).magnitude < 0.05f)
				panel.anchoredPosition = targetPos;

			//Debug.Log($"moving {panel.name}");

			yield return new WaitForEndOfFrame();
		}

		callback?.Invoke();
	}

	private IEnumerator MoveDragZone(RectTransform panel, DragZone.ChangeSizeSide side, Vector2 stopMovingPosition)
	{
		if (moveSpeed == 0f)
			yield break;

		//Vector2 distPassed = Vector2.zero;

		Vector2 prevPos = stopMovingPosition;

		//  1 if side == Right_Or_Top
		// -1 if side == Left_Or_Bottom
		float moveDirection = Convert.ToSingle(side == DragZone.ChangeSizeSide.Right_Or_Top) * 2 - 1;



		while (true /*stopMovingPosition.magnitude > distPassed.magnitude*/) // while is not stopped from outside
		{
			Vector2 diff = panel.anchoredPosition - prevPos;



			// TODO: условие не робит при движении влево
			if (panel.anchoredPosition.x > stopMovingPosition.x)
			{
				Debug.Log("x: " + panel.name);
				dragZone.ChangeSize(DragZone.SizeAxis.X, side, diff.x * moveDirection);

				//distPassed.x += panel.anchoredPosition.x - prevPosX;
				prevPos.x = panel.anchoredPosition.x;
			}

			if (panel.anchoredPosition.y > stopMovingPosition.y)
			{
				Debug.Log("y: " + panel.name);
				dragZone.ChangeSize(DragZone.SizeAxis.Y, side, diff.y * moveDirection);

				//distPassed.y += panel.anchoredPosition.y - prevPosY;
				prevPos.y = panel.anchoredPosition.y;
			}

			Debug.Log("MoveDragZone");
			yield return new WaitForEndOfFrame();
		}
	}
}
