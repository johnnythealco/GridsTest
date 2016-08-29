using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitCostDisplay : MonoBehaviour
{

	public Text unitName;
	public Image Icon;
	public Text unitCostValue;


	UnitCost unitCost;

	public delegate void unitCostDisplayDelegate (UnitCost _unitCost);

	public event unitCostDisplayDelegate onClick;


	public void Prime (UnitCost _unitCost)
	{
		unitCost = _unitCost; 
		var unit = Game.Register.GetUnitType (unitCost.unitType); 

		if (unitName != null)
			unitName.text = unit.DsiplayName; 
		if (Icon != null)
			Icon.sprite = unit.Icon; 
		if (unitCostValue != null)
			unitCostValue.text = unitCost.cost.ToString (); 



	}

	public void OnClick ()
	{
		if (onClick != null)
			onClick.Invoke (unitCost);
	}

	public void Highlight ()
	{
		unitName.fontStyle = FontStyle.Bold;
	}

	public void UnHighlight ()
	{
		unitName.fontStyle = FontStyle.Normal;
	}


}
