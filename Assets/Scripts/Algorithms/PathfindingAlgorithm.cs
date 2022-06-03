public class PathfindingAlgorithm
{
	private const string DEFAULT_NAME = "NoName algorithm";

	public string Name { get; }

	public FindBestPathDelegate FindBestPath { get; }
	public AlgorithmTeaching AlgorithmTeaching { get; }



	// Constructor
	public PathfindingAlgorithm(string name, FindBestPathDelegate findBestPathMethod, AlgorithmTeaching algorithmTeaching)
	{
		if (string.IsNullOrWhiteSpace(name))
			this.Name = DEFAULT_NAME;
		else
			this.Name = name;



		this.FindBestPath = findBestPathMethod;
		this.AlgorithmTeaching = algorithmTeaching;
	}
}

public delegate int[] FindBestPathDelegate(int[,] graph, int startNodeID, int endNodeID);
public delegate (int[] path, int[,] graphCopy, string message, int nodeToHighlight)[] AlgorithmTeaching(int[,] inputGraph, int startNodeID, int endNodeID);
