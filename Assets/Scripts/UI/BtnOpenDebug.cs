using UnityEngine;
using TMPro;

public class BtnOpenDebug : MonoBehaviour
{
	[SerializeField] private GameObject notifCircle;
	[SerializeField] private TextMeshProUGUI notifText;
	private int notificationsCount;

	void Awake()
	{
		ClearNotifications();
	}

	void OnEnable()
	{
		ScreenDebug.OnNewLogWarningOrError += Notify;
	}
	void OnDisable()
	{
		ScreenDebug.OnNewLogWarningOrError -= Notify;
	}



	private void Notify()
	{
		notificationsCount++;

		notifText.text = notificationsCount.ToString();
		notifCircle.SetActive(true);
	}

	public void ClearNotifications()
	{
		notificationsCount = 0;

		notifText.text = "";
		notifCircle.SetActive(false);
	}
}
