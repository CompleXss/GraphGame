using UnityEngine;

public class StartEndNodeSwitcher : MonoBehaviour
{
	[SerializeField] private Graph graph;



	public void ChangeStartNode(string value)
	{
		if (!int.TryParse(value, out int result) || result < 0 || result >= graph.Nodes.Count || result == graph.EndNode.ID || result == graph.StartNode.ID)
			return;

		var startNode = graph.Nodes.Find(x => x.ID == result);
		if (startNode != null)
			graph.StartNode = startNode;
	}

	public void ChangeEndNode(string value)
	{
		if (!int.TryParse(value, out int result) || result < 0 || result >= graph.Nodes.Count || result == graph.StartNode.ID || result == graph.EndNode.ID)
			return;

		var endNode = graph.Nodes.Find(x => x.ID == result);
		if (endNode != null)
			graph.EndNode = endNode;
	}
}
