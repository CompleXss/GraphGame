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
		InitConstantAlgs();
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
			algObj.name = alg.Name;



			// Настройка кнопок или их отключение, если соответствующего метода нет
			if (alg.FindBestPath == null)
				algObj.SetSubButtonVisibleState(algObj.FindBestPathBtn, false);
			else
			{
				algObj.SetSubButtonVisibleState(algObj.FindBestPathBtn, true);
				algObj.FindBestPathBtn.onClick.AddListener(() => graph.FindBestPath(alg.FindBestPath));
			}

			if (alg.FindAllPaths == null)
				algObj.SetSubButtonVisibleState(algObj.FindAllPathsBtn, false);
			else
			{
				algObj.SetSubButtonVisibleState(algObj.FindAllPathsBtn, true);
				algObj.FindAllPathsBtn.onClick.AddListener(() => graph.FindAllPaths(alg.FindAllPaths));
			}

			if (true) // TODO: if (true)
				algObj.SetSubButtonVisibleState(algObj.AlgorithmTeachingBtn, false);
			else
			{
				algObj.SetSubButtonVisibleState(algObj.AlgorithmTeachingBtn, true);
				// TODO: алгоритм обучения
			}
		}
	}



	private void InitConstantAlgs()
	{
		var alg = algsGroupObj.GetComponentInChildren<AlgorithmButton>();

		alg.FindBestPathBtn.onClick.AddListener(() => graph.FindBestPath(DijkstraAlgorithm.FindBestPath));
		alg.SetSubButtonVisibleState(alg.FindBestPathBtn, true);
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
