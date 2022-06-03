using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlgorithmButton : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;

	[SerializeField] private Button findBestPathBtn;
	[SerializeField] private Button algorithmTeachingBtn;

	public Button FindBestPathBtn => findBestPathBtn;
	public Button AlgorithmTeachingBtn => algorithmTeachingBtn;

	public string UIName
	{
		get => nameText.text;
		set { nameText.text = value; }
	}

	private List<Button> btnProps;
	void Awake()
	{
		btnProps = new List<Button>();

		foreach (var prop in this.GetType().GetProperties())
			if (prop.PropertyType == typeof(Button))
			{
				btnProps.Add((Button)prop.GetValue(this));
			}
	}



	/// <summary> Передавайте как параметр только кнопки, принадлежащие этому классу. </summary>
	public void SetSubButtonVisibleState(Button button, bool state)
	{
		if (button.gameObject.activeSelf == state || !btnProps.Contains(button))
			return;

		button.gameObject.SetActive(state);

		// Изменение размера кнопки
		var heightDelta = new Vector2(0f, button.GetComponent<RectTransform>().sizeDelta.y);

		if (!state)
			heightDelta.y *= -1f;

		button.transform.parent.GetComponent<RectTransform>().sizeDelta += heightDelta;
	}
}
