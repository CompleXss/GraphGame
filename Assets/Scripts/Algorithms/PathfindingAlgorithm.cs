public class PathfindingAlgorithm
{
	private const string DEFAULT_NAME = "NoName algorithm";

	public string Name { get; }

	public FindBestPathDelegate FindBestPath { get; }
	public FindAllPathsDelegate FindAllPaths { get; }
	public AlgorithmTeaching AlgorithmTeaching { get; }



	// Constructor
	public PathfindingAlgorithm(string name, FindBestPathDelegate findBestPathMethod, FindAllPathsDelegate findAllPathsMethod, AlgorithmTeaching algorithmTeaching)
	{
		if (string.IsNullOrWhiteSpace(name))
			this.Name = DEFAULT_NAME;
		else
			this.Name = name;

		this.FindBestPath = findBestPathMethod;
		this.FindAllPaths = findAllPathsMethod;
		this.AlgorithmTeaching = algorithmTeaching;
	}
}
