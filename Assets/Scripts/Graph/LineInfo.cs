using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New LineInfo", menuName = "LineInfo")]
public class LineInfo : ScriptableObject
{
	[SerializeField] private LineRenderer linePrefab;
	[SerializeField] private TextMeshProUGUI lineTextPrefab;
	[SerializeField] private float textUpOffset;

	public LineRenderer LinePrefab => linePrefab;
	public TextMeshProUGUI LineTextPrefab => lineTextPrefab;
	public float TextUpOffset => textUpOffset;
}
