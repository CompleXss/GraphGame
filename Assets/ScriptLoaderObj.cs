using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScriptLoaderObj : MonoBehaviour
{
	[SerializeField] private Graph graph;

	[SerializeField] private Transform algsGroupObj;
	[SerializeField] private AlgorithmButton algButtonsPrefab;
	[SerializeField] private List<Transform> undeletableAlgs;

	void Start()
	{
		LoadAlgorithms();
	}



	public void LoadAlgorithms()
	{
		var loader = new ScriptLoader();
		var algorithms = loader.LoadAlgorithms();

		// Удаление старых кнопок
		DestroyAlgButtons();

		// Спавн новых кнопок
		foreach (var alg in algorithms)
		{
			var algObj = Instantiate(algButtonsPrefab, algsGroupObj);
			algObj.UIName = alg.Name;



			// Отключение кнопок, если соответствующего метода нет
			if (alg.FindBestPath == null)
				algObj.DisableSubButton("Find Best Path");
			else
			{
				algObj.FindBestPathBtn.onClick.AddListener(() => graph.FindBestPath(alg.FindBestPath));
			}

			if (alg.FindAllPaths == null)
				algObj.DisableSubButton("Find More Paths");
			else
			{
				algObj.FindAllPathsBtn.onClick.AddListener(() => graph.FindAllPaths(alg.FindAllPaths));
			}

			if (true) // TODO: if (true)
				algObj.DisableSubButton("Algorithm teaching");
			else
			{
				// TODO: алгоритм обучения
			}
		}
	}

	private void DestroyAlgButtons()
	{
		foreach (Transform alg in algsGroupObj)
		{
			if (!undeletableAlgs.Contains(alg))
				Destroy(alg.gameObject);
		}
	}
}
