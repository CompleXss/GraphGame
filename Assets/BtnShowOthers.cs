using UnityEngine;

public class BtnShowOthers : MonoBehaviour
{
	[SerializeField] private GameObject objToShow;
	[SerializeField] private RectTransform objSizeToChange;

	private ButtonsController btnController;

	void Awake()
	{
		btnController = GetComponentInParent<ButtonsController>();
	}

	public void ShowHideSubButtons()
	{
		var heightDelta = new Vector2(0f, objToShow.GetComponent<RectTransform>().sizeDelta.y);
		if (objToShow.activeSelf)
		{
			heightDelta *= -1f; // ��������� ������ ������
		}
		else if (btnController != null)
			btnController.HideAllSubButtons(); // ���� ���������� ��� ������, ������ ������

		objToShow.SetActive(!objToShow.activeSelf);
		objSizeToChange.sizeDelta += heightDelta;
	}

	public void HideSubButtons()
	{
		if (objToShow.activeSelf)
			ShowHideSubButtons();
	}
}
