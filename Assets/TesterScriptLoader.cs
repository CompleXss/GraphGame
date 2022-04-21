using UnityEngine;

public class TesterScriptLoader : MonoBehaviour
{
	void Start()
	{
		var loader = new ScriptLoader();

		var algs = loader.LoadAlgorithms();
		foreach (var alg in algs)
		{
			Debug.Log(alg.Name);
			Debug.Log(alg.FindBestPath?.Invoke(new int[0, 0], 0, 0));
			Debug.Log(alg.FindAllPaths?.Invoke(new int[0, 0], 0, 0));

			Debug.Log("==============");
		}
	}
}
