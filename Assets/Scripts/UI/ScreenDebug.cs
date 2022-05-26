using System;

using UnityEngine;
using TMPro;

public class ScreenDebug : MonoBehaviour
{
	[SerializeField] private DebugEntry newEntryPrefab;
	[SerializeField] private Transform whereToSpawnEntry;

	[Header("Teaching")]
	[SerializeField] private TextMeshProUGUI messageBox;
	[SerializeField] private TextMeshProUGUI timeBox;

	[Header("Icons")]
	[SerializeField] private Sprite messageSprite;
	[SerializeField] private Sprite warningSprite;
	[SerializeField] private Sprite errorSprite;

	public static event Action OnNewLogWarningOrError;



	#region Singleton
	private static ScreenDebug instance;
	void Awake()
	{
		if (instance != null)
			Destroy(this);

		instance = this;
		ClearAllEntries();
	}
	#endregion



	public static void Log(string message)
	{
		instance.Log(message, instance.messageSprite);
	}
	public static void LogWarning(string message)
	{
		instance.Log(message, instance.warningSprite);
		OnNewLogWarningOrError?.Invoke();
	}
	public static void LogError(string message)
	{
		instance.Log(message, instance.errorSprite);
		OnNewLogWarningOrError?.Invoke();
	}

	private void Log(string message, Sprite sprite)
	{
		var entry = Instantiate(newEntryPrefab, whereToSpawnEntry);

		entry.Sprite = sprite;
		entry.Time = DateTime.Now.ToString("HH:mm:ss");
		entry.Text = message;
	}



	public static void ShowTime(string time)
	{
		instance.timeBox.text = time + " мс";
	}
	public static void ClearTime()
	{
		instance.timeBox.text = "-";
	}



	public static void ShowTeachingMessage(string message)
	{
		instance.messageBox.text = message;
	}
	public static void ClearTeachingMessage()
	{
		instance.messageBox.text = "";
	}



	public static void ClearAllEntries()
	{
		foreach (Transform obj in instance.whereToSpawnEntry)
			Destroy(obj.gameObject);
	}
}
