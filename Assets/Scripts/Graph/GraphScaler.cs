using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class GraphScaler
{
	const float ADDITIONAL_SIZE = 200f;



	public static void FitGraphTo(RectTransform graphTransform, List<Node> nodes)
	{
		// Move To center
		var centerX = nodes.Sum(x => x.transform.localPosition.x) / nodes.Count;
		var centerY = nodes.Sum(x => x.transform.localPosition.y) / nodes.Count;

		Vector2 center = new Vector2(centerX, centerY);
		graphTransform.anchoredPosition = center;



		// Scale X
		var sortedNodesX = nodes.OrderBy(x => x.transform.localPosition.x);
		Node leftNode = sortedNodesX.First();
		Node rightNode = sortedNodesX.Last();

		float leftSide = leftNode.transform.localPosition.x - leftNode.RectTransform.sizeDelta.x;
		float rightSide = rightNode.transform.localPosition.x + rightNode.RectTransform.sizeDelta.x;
		float width = rightSide - leftSide;

		// Scale Y
		var sortedNodesY = nodes.OrderBy(x => x.transform.localPosition.y);
		Node bottomNode = sortedNodesY.First();
		Node upNode = sortedNodesY.Last();

		float bottomSide = bottomNode.transform.localPosition.y - bottomNode.RectTransform.sizeDelta.y;
		float upSide = upNode.transform.localPosition.y + upNode.RectTransform.sizeDelta.y;
		float height = upSide - bottomSide;



		// Scale
		graphTransform.sizeDelta = new Vector2(width + ADDITIONAL_SIZE, height + ADDITIONAL_SIZE);
	}
}
