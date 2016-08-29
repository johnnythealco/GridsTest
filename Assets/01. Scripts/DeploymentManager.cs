using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeploymentManager : MonoBehaviour
{
	public Text RemainingPoints;
	public UnitCostListDisplay unitCostListDisplay;
	public int deploymentPoints;




	public List<UnitCost> buildableUnits;
	int remainingPoints;
	UnitCost selectedUnit;

	public delegate void DeploymentManagerDelegate_Unit (string _unit);

	public delegate void DeploymentManagerDelegate ();

	public event DeploymentManagerDelegate_Unit onSelectUnit;
	public event DeploymentManagerDelegate onEndDeployment;


	void Awake ()
	{
		remainingPoints = deploymentPoints;
		RemainingPoints.text = remainingPoints.ToString ();
	}

	void Start ()
	{
		unitCostListDisplay.Prime (buildableUnits);
		unitCostListDisplay.onListItemClick += OnClickUnit;

	}

	void OnClickUnit (UnitCost _unitCost)
	{
		selectedUnit = _unitCost;
		unitCostListDisplay.HighlightDisplay (selectedUnit.unitType);

		if (onSelectUnit != null)
			onSelectUnit.Invoke (selectedUnit.unitType);

	}

	public bool BuildUnit ()
	{

		if (selectedUnit.cost <= remainingPoints)
		{
			Debug.Log (" Build unit " + selectedUnit.unitType);
			remainingPoints = remainingPoints - selectedUnit.cost;
			RemainingPoints.text = remainingPoints.ToString ();
			return true;
		} else
		{
			Debug.Log ("You need more Moneys!");
		}
		return false;
	}

	public void EndDeployment ()
	{
		if (onEndDeployment != null)
			onEndDeployment.Invoke ();
	}


}


