using UnityEngine;

[CreateAssetMenu(fileName = "Node Colors", menuName = "Node Colors")]
public class NodeColors : ScriptableObject
{
	public Color Start = Color.green;
	public Color End = Color.red;
	public Color ConnectionStart = Color.cyan;
	public Color ConnectionEnd = Color.magenta;
	public Color Middle = Color.yellow;
	public Color Additional = Color.grey;
}
