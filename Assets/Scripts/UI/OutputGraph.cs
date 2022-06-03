using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class OutputGraph : MonoBehaviour
{
	[SerializeField] private GameObject noGraphText;
	[SerializeField] private MatrixCell cellPrefab;

	[Header("Colors")]
	[SerializeField] private Color headerCellsColor;
	[SerializeField] private Color insideCellsColor;
	[SerializeField] private Color diagonalTextColor;

	private GridLayoutGroup grid;
	private RectTransform rectTransform;
	private MatrixCell[,] cells;
	private Vector2Int matrixSize;

	void Awake()
	{
		grid = GetComponent<GridLayoutGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	// TODO: Подстветка текста клеток красным ?


	private void CreateGrid(int[,] graph)
	{
		DeleteGrid();

		matrixSize.x = graph.GetLength(1);
		matrixSize.y = graph.GetLength(0);

		SetRTSize(matrixSize);
		cells = new MatrixCell[matrixSize.y, matrixSize.x];



		// // Cells // //
		var emptyCell = Instantiate(cellPrefab, transform);
		emptyCell.Color = headerCellsColor;
		emptyCell.Text = "";

		// First line
		for (int col = 0; col < matrixSize.x; col++)
		{
			var cell = Instantiate(cellPrefab, transform);
			cell.Color = headerCellsColor;
			cell.Text = col.ToString();
		}

		// Other lines
		for (int line = 0; line < matrixSize.y; line++)
		{
			var firstCell = Instantiate(cellPrefab, transform);
			firstCell.Color = headerCellsColor;
			firstCell.Text = line.ToString();

			for (int col = 0; col < matrixSize.x; col++)
			{
				var cell = Instantiate(cellPrefab, transform);
				cell.Color = insideCellsColor;

				if (line == col)
					cell.TextColor = diagonalTextColor;

				cells[line, col] = cell;
			}
		}

		noGraphText.SetActive(false);
	}

	private void SetRTSize(Vector2Int matrixSize)
	{
		Vector2 size = grid.cellSize + grid.spacing;
		size.x *= matrixSize.x + 1;
		size.y *= matrixSize.y + 1;

		size -= grid.spacing; // Чтобы не было лишнего спейсинга в конце

		rectTransform.sizeDelta = size;
	}



	/// <summary> Отображает матрицу смежности в виде таблицы. </summary>
	public void Show(int[,] graph)
	{
		noGraphText.SetActive(graph == null);

		if (graph == null)
			return;

		if (graph.GetLength(0) < 1 || graph.GetLength(1) < 1)
		{
			ScreenDebug.LogWarning("Показ матрицы: входной параметр недействителен.");
			return;
		}



		if (cells == null || matrixSize == Vector2Int.zero || graph.GetLength(0) != matrixSize.y || graph.GetLength(1) != matrixSize.x)
		{
			CreateGrid(graph);
		}

		for (int line = 0; line < matrixSize.y; line++)
			for (int col = 0; col < matrixSize.x; col++)
			{
				cells[line, col].Text = graph[line, col] == int.MaxValue
					? "inf"
					: graph[line, col].ToString();
			}
	}

	public void DeleteGrid()
	{
		noGraphText.SetActive(true);

		matrixSize = Vector2Int.zero;
		cells = null;

		foreach (Transform obj in transform)
			Destroy(obj.gameObject);
	}
}
