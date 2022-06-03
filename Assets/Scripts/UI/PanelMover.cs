using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class PanelMover : MonoBehaviour
{
	[SerializeField] private DragZone dragZone;
	[SerializeField] private GameObject lowerLeftPanel;
	[SerializeField] private GameObject upLeftPanel;
	[SerializeField] private GameObject upPanel;
	public float moveSpeed;

	private RectTransform upLeftPanelRT;
	private RectTransform lowerLeftPanelRT;
	private RectTransform upPanelRT;

	private Coroutine upLeftPanelRoutine;
	private Coroutine upPanelRoutine;

	private bool upLeftPanelShown;
	private bool upPanelShown;



	void Awake()
	{
		upLeftPanelRT = upLeftPanel.GetComponent<RectTransform>();
		lowerLeftPanelRT = lowerLeftPanel.GetComponent<RectTransform>();
		upPanelRT = upPanel.GetComponent<RectTransform>();
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

		SetButtonsActiveState(lowerLeftPanelRT, !state);



		// ��������
		if (state)
		{
			upLeftPanel.SetActive(true);

			// ��������� ���� ����
			dragZone.ChangeSize(DragZone.ChangeSizeSide.Left, lowerLeftPanelRT.sizeDelta.x - upLeftPanelRT.sizeDelta.x);

			upLeftPanelRoutine = StartCoroutine(MovePanel(upLeftPanelRT, Vector2.zero, moveSpeed, () =>
			{
				lowerLeftPanel.SetActive(false);
			}));
		}

		// ������
		else
		{
			lowerLeftPanel.SetActive(true);

			// ��������� ���� ����
			dragZone.ChangeSize(DragZone.ChangeSizeSide.Left, upLeftPanelRT.sizeDelta.x - lowerLeftPanelRT.sizeDelta.x);

			upLeftPanelRoutine = StartCoroutine(MovePanel(upLeftPanelRT, new Vector2(-upLeftPanelRT.rect.width, 0f), moveSpeed, () =>
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



		// ��������
		if (state)
		{
			upPanel.SetActive(true);

			// ��������� ���� ����
			//dragZone.ChangeSize(DragZone.ChangeSizeSide.Top, -upPanelRT.rect.height);

			upPanelRoutine = StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, 0f), moveSpeed, null));
		}

		// ������
		else
		{
			// ��������� ���� ����
			//dragZone.ChangeSize(DragZone.ChangeSizeSide.Top, upPanelRT.rect.height);

			upPanelRoutine = StartCoroutine(MovePanel(upPanelRT, new Vector2(upPanelRT.anchoredPosition.x, upPanelRT.rect.height), moveSpeed, () =>
			{
				upPanel.SetActive(false);
			}));
		}
	}

	private void SetButtonsActiveState(Transform buttonsParentObj, bool state)
	{
		foreach (Button btn in buttonsParentObj.GetComponentsInChildren<Button>())
			btn.enabled = state;
	}



	public IEnumerator MovePanel(RectTransform panel, Vector2 targetPos, float moveSpeed, Action callback)
	{
		if (moveSpeed <= 0f)
			yield break;

		float lastMoveSpeed = moveSpeed * 40;
		float startMovingLinearDistance = (panel.anchoredPosition - targetPos).magnitude / 12;

		while (panel.anchoredPosition != targetPos)
		{
			panel.anchoredPosition = (panel.anchoredPosition - targetPos).magnitude > startMovingLinearDistance
				? panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, moveSpeed * Time.deltaTime)
				: panel.anchoredPosition = Vector2.MoveTowards(panel.anchoredPosition, targetPos, lastMoveSpeed * Time.deltaTime);

			yield return new WaitForEndOfFrame();
		}

		callback?.Invoke();
	}
}
