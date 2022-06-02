using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderUI : MonoBehaviour
{
	private Slider slider;

	void Awake()
	{
		slider = GetComponent<Slider>();
	}



	public void GoBack()
	{
		if (slider.value > slider.minValue)
			slider.value--;
	}

	public void GoNext()
	{
		if (slider.value < slider.maxValue)
			slider.value++;
	}
}
