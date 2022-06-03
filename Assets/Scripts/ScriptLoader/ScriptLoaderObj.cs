using System.Collections.Generic;
using UnityEngine;

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
			if (alg.FindBestPath == null && alg.AlgorithmTeaching == null)
				continue;

			var algObj = Instantiate(algButtonsPrefab, algsGroupObj);
			algObj.UIName = alg.Name;
			algObj.name = alg.Name;



			#region Настройка кнопок или их отключение, если соответствующего метода нет

			// FindBestPath
			if (alg.FindBestPath == null)
				algObj.SetSubButtonVisibleState(algObj.FindBestPathBtn, false);
			else
			{
				algObj.SetSubButtonVisibleState(algObj.FindBestPathBtn, true);
				algObj.FindBestPathBtn.onClick.AddListener(() => graph.FindBestPath(alg.FindBestPath));
			}

			// AlgorithmTeaching
			if (alg.AlgorithmTeaching == null)
				algObj.SetSubButtonVisibleState(algObj.AlgorithmTeachingBtn, false);
			else
			{
				algObj.SetSubButtonVisibleState(algObj.AlgorithmTeachingBtn, true);
				algObj.AlgorithmTeachingBtn.onClick.AddListener(() => graph.StartAlgorithmTeaching(alg.AlgorithmTeaching, alg.FindBestPath));
			}
			#endregion
		}
	}



	private void InitConstantAlgs()
	{
		// Дейкстра
		var alg = algsGroupObj.GetComponentInChildren<AlgorithmButton>();

		// FindBestPath
		alg.FindBestPathBtn.onClick.AddListener(() => graph.FindBestPath(DijkstraAlgorithm.FindBestPath));
		alg.SetSubButtonVisibleState(alg.FindBestPathBtn, true);

		// AlgorithmTeaching
		alg.AlgorithmTeachingBtn.onClick.AddListener(() => graph.StartAlgorithmTeaching(DijkstraAlgorithm.GetAlgorithmStep, DijkstraAlgorithm.FindBestPath));
		alg.SetSubButtonVisibleState(alg.AlgorithmTeachingBtn, true);
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
