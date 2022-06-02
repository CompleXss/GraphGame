using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class MatrixCell : MonoBehaviour
{
	[SerializeField] private Image image;
	[SerializeField] private TextMeshProUGUI textMeshPro;



	public Color Color
	{
		get => image.color;
		set => image.color = value;
	}

	public Color TextColor
	{
		get => textMeshPro.color;
		set => textMeshPro.color = value;
	}

	public string Text
	{
		get => textMeshPro.text;
		set => textMeshPro.text = value.Trim();
	}
}
