using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitCostListDisplay : MonoBehaviour
{
	public Transform listPanel;
	public UnitCostDisplay unitCostDisplayPrefab;

	public delegate void unitCostListDisplayDelegate (UnitCost _unitCost);

	public event unitCostListDisplayDelegate onListItemClick;

	List<UnitCostDisplay> UnitCostDisplays = new List<UnitCostDisplay> ();

	List<UnitCost> unitCosts;

	public void Prime (List<UnitCost> _unitCosts)
	{
		unitCosts = _unitCosts;
		clearUnitCostDisplays ();

		if (listPanel != null)
		{
			foreach (var item in unitCosts)
			{
				var unitCostDisplay = (UnitCostDisplay)Instantiate (unitCostDisplayPrefab);
				unitCostDisplay.transform.SetParent (listPanel);
				unitCostDisplay.Prime (item);
				unitCostDisplay.onClick += UnitCostDisplay_onClick;
				UnitCostDisplays.Add (unitCostDisplay);
			}
				
		}

	}

	void UnitCostDisplay_onClick (UnitCost _unitCost)
	{
		if (onListItemClick != null)
			onListItemClick.Invoke (_unitCost);
	}

	void clearUnitCostDisplays ()
	{
		foreach (var item 	in UnitCostDisplays)
		{
			item.onClick -= UnitCostDisplay_onClick;
		}

		for (int i = 0; i < listPanel.childCount; i++)
		{
			Destroy (listPanel.GetChild (i).gameObject);
		}

	}

	public void HighlightDisplay (string _unitName)
	{
		foreach (var item 	in UnitCostDisplays)
		{
			if (item.unitName.text == _unitName)
				item.Highlight ();
			else
				item.UnHighlight ();
		}
	}


	void onDestroy ()
	{
		clearUnitCostDisplays ();
	}
}
