using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class JKTesting : MonoBehaviour
{
	public Text RemainingPoints;
	public UnitCostListDisplay unitCostListDisplay;
	public int deploymentPoints;



	public List<UnitCost> buildableUnits;

	int remainingPoints;


	void Awake ()
	{
		remainingPoints = deploymentPoints;
		RemainingPoints.text = remainingPoints.ToString ();
	}

	void Start ()
	{
		unitCostListDisplay.Prime (buildableUnits);
		unitCostListDisplay.onListItemClick += OnBuildUnit;

	}

	void OnBuildUnit (UnitCost _unitCost)
	{
		var cost = _unitCost.cost;

		if (cost <= remainingPoints)
		{
			Debug.Log (" Build unit " + _unitCost.unitType);
			remainingPoints = remainingPoints - cost;
			RemainingPoints.text = remainingPoints.ToString ();
		} else
		{
			Debug.Log ("You need more Moneys!");
		}


	}


}


