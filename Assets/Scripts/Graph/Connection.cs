[System.Serializable]
public struct Connection
{
	public Node node;
	public int weight;

	public Connection(Node node, int weight)
	{
		this.node = node;
		this.weight = weight;
	}
}
