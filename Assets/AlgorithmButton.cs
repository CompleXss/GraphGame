using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlgorithmButton : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;

	[SerializeField] private Button findBestPathBtn;
	[SerializeField] private Button findAllPathsBtn;
	[SerializeField] private Button algorithmTeachingBtn;

	public Button FindBestPathBtn => findBestPathBtn;
	public Button FindAllPathsBtn => findAllPathsBtn;
	public Button AlgorithmTeachingBtn => algorithmTeachingBtn;

	public string UIName
	{
		get => nameText.text;
		set { nameText.text = value; }
	}



	public void DisableSubButton(string subBtnName)
	{
		var btn = transform.Find(subBtnName);

		if (btn != null)
			btn.gameObject.SetActive(false);
	}
}
