using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugEntry : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI timeText;
	[SerializeField] private TextMeshProUGUI messageText;
	[SerializeField] private Image image;



	public string Time
	{
		get => timeText.text;
		set => timeText.text = value.Trim();
	}

	public string Text
	{
		get => messageText.text;
		set => messageText.text = value.Trim();
	}

	public Sprite Sprite
	{
		get => image.sprite;
		set => image.sprite = value;
	}
}
