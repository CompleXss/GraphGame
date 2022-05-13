using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class OutputGraph : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI noGraphText;
	[SerializeField] private MatrixCell cellPrefab;

	[Header("Colors")]
	[SerializeField] private Color headerCellsColor;
	[SerializeField] private Color insideCellsColor;

	private GridLayoutGroup grid;
	private RectTransform rectTransform;
	private MatrixCell[,] cells;
	private int matrixSize;

	void Awake()
	{
		grid = GetComponent<GridLayoutGroup>();
		rectTransform = GetComponent<RectTransform>();
	}



	public void CreateGrid(int[,] graph)
	{
		if (graph == null || graph.Length < 1)
			return;

		matrixSize = graph.GetLength(0);
		SetRTSize(matrixSize);
		cells = new MatrixCell[matrixSize, matrixSize];



		// Cells
		var emptyCell = Instantiate(cellPrefab, transform);
		emptyCell.Color = headerCellsColor;
		emptyCell.Text = "";

		// First line
		for (int i = 0; i < matrixSize; i++)
		{
			var cell = Instantiate(cellPrefab, transform);
			cell.Color = headerCellsColor;
			cell.Text = i.ToString();
		}

		// Other lines
		for (int line = 0; line < matrixSize; line++)
		{
			var firstCell = Instantiate(cellPrefab, transform);
			firstCell.Color = headerCellsColor;
			firstCell.Text = line.ToString();

			for (int col = 0; col < matrixSize; col++)
			{
				var cell = Instantiate(cellPrefab, transform);
				cells[line, col] = cell;

				cell.Color = insideCellsColor;

				cell.Text = graph[line, col] == int.MaxValue
					? "inf"
					: graph[line, col].ToString();
			}
		}

		noGraphText.enabled = false;
	}

	private void SetRTSize(int matrixSize)
	{
		Vector2 size = (matrixSize + 1) * (grid.cellSize + grid.spacing);

		rectTransform.sizeDelta = size;
	}



	public void Show(int[,] graph)
	{
		if (graph == null)
			return;

		if (matrixSize == 0 || cells == null || cells.GetLength(0) < matrixSize)
		{
			CreateGrid(graph);
			return;
		}
		if (graph.Length < matrixSize)
			return;



		for (int line = 0; line < matrixSize; line++)
			for (int col = 0; col < matrixSize; col++)
			{
				cells[line, col].Text = graph[line, col] == int.MaxValue
					? "inf"
					: graph[line, col].ToString();
			}
	}

	public void DeleteGrid()
	{
		noGraphText.enabled = true;

		matrixSize = 0;
		cells = null;

		foreach (Transform obj in transform)
			Destroy(obj.gameObject);
	}
}
