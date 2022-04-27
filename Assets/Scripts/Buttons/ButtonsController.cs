using UnityEngine;

public class ButtonsController : MonoBehaviour
{
	public void HideAllSubButtons()
	{
		foreach (var btn in GetComponentsInChildren<BtnShowOthers>())
			btn.HideSubButtons();
	}
}
