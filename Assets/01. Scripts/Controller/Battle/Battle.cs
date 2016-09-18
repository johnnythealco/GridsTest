using UnityEngine;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;


public class Battle : MonoBehaviour
{
	#region Variables

	public FlatHex flatHexPrefab;
	public SpriteRenderer cellBorder;
	public DeploymentManager deploymentManager;
	public UnitModelDisplay unitDisplay;
	public CameraCTRL cameraCTRL;
	public JKLog gameLog;


	public FlatHex BattleGrid{ get; set; }

	public static Turn TurnManager{ get; set; }

	public static UnitModel SelectedUnit{ get; set; }

	public static List<UnitModel> AllUnits{ get; set; }


	SpriteRenderer gridCursor;
	SpriteRenderer selectedUnitCursor;
    SpriteRenderer selectedTargetCursor;
    List<SpriteRenderer> PathSteps = new List<SpriteRenderer>();

	public List<Vector3> line = new List<Vector3> ();


	#endregion

	#region Start & Update

	void Start ()
	{
		Game.BattleManager = this;
		BattleGrid = Instantiate (flatHexPrefab) as FlatHex;
		BattleGrid.BuildGrid ();
		BattleGrid.onClickCell += BattleGrid_onClickCell;
		BattleGrid.onMouseOverCell += BattleGrid_onMouseOverCell;
		BattleGrid.onRightClickCell += BattleGrid_onRightClickCell;
		BattleGrid.onNoCellSelected += BattleGrid_onNoCellSelected;

		Battle.TurnManager.onUnitStartTrun += OnUnitStartTrun;
		Battle.TurnManager.onUnitEndTrun += OnUnitEndTrun;


		gameLog.gameObject.SetActive (true);


		gridCursor = Instantiate (cellBorder) as SpriteRenderer;
	    selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
        selectedTargetCursor = Instantiate(cellBorder) as SpriteRenderer;
        selectedUnitCursor.gameObject.SetActive (false);
        selectedTargetCursor.gameObject.SetActive(false);
        gridCursor.gameObject.SetActive(false);

    }



	#endregion

	#region Event handlers

	void BattleGrid_onRightClickCell (Vector3 _point)
	{
		//foreach (var p in line)
		//{
		//	var cell = BattleGrid.GetCell (p);
		//	cell.Color = Color.white;
		//}

		//line.Clear ();

		//var start = TurnManager.activeUnit.transform.position;

		//line = BattleGrid.GetLine (start, _point);

		//foreach (var p in line)
		//{
		//	var cell = BattleGrid.GetCell (p);
		//	cell.Color = Color.cyan;
		//}

	}

	void BattleGrid_onMouseOverCell (Vector3 _point)
	{
        if (BattleAction.LegalMoves == null)
            return;

        if (BattleAction.LegalMoves.Contains(_point))
        {
            highlightLegalMove(_point);
            return;
        }

        if (BattleAction.LegalTargets != null && BattleAction.LegalTargets.Contains(_point))
        {
            highlightTarget(_point);
            return;
        }

        highlightEmptyCell (_point);
	}

	void BattleGrid_onClickCell (Vector3 _point)
	{
        if (BattleAction.LegalMoves == null)
            return;

        if (BattleAction.LegalMoves.Contains(_point))
        {
            BattleAction.Destination = _point;
            highlightMovePath();
            return;
        }

        //if(BattleAction.LegalTargets.Contains(_point))
        //{

        //}

        if(!BattleAction.LegalMoves.Contains(_point))
        {
            BattleAction.Destination = BattleAction.ActiveUnit.transform.position;
            if (PathSteps.Count() > 0)
                ClearPathSteps();
        }





    }

	void BattleGrid_onNoCellSelected ()
	{
		gridCursor.gameObject.SetActive (false);

	}

	
	void OnUnitStartTrun ()
	{
        var _point = BattleAction.ActiveUnit.transform.position;

        BattleAction.GetLegalMoves(BattleAction.ActiveUnit);

        unitDisplay.Prime(BattleAction.ActiveUnit);
        unitDisplay.onChangeAction += ActiveUnit_onChangeAction;
        selectedUnitCursor.gameObject.SetActive(true);
        selectedUnitCursor.color = Color.blue;
        selectedUnitCursor.transform.position = _point;
        selectedUnitCursor.transform.SetParent(BattleAction.ActiveUnit.transform);

        cameraCTRL.CentreOn (_point); 
    }

	void OnUnitEndTrun ()
	{
		unitDisplay.onChangeAction -= ActiveUnit_onChangeAction;
		Battle.TurnManager.CmdNextUnit ();
	}

	void ActiveUnit_onChangeAction ()
	{
		
	}

	public void StartBattle ()
	{

		Battle.TurnManager.CmdSortList ();


	
	}



	#endregion



	#region Highlighting

	void highlightEmptyCell (Vector3 _point)
	{
		gridCursor.color = Color.gray;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	

	}

	void highlightDeployment (Vector3 _point)
	{
		gridCursor.color = Color.yellow;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	

	}

	void highlightTarget (Vector3 _point)
	{
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
		gridCursor.color = Color.red;
	}

	void highlightEnemy (Vector3 _point)
	{
	    gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
		gridCursor.color = Color.magenta;
	}

	void highlightLegalMove (Vector3 _point)
	{	
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
		gridCursor.color = Color.green;
	}

    void highlightMovePath()
    {
        ClearPathSteps();

        if (BattleAction.ActiveUnit == null)
            return;

        var start = BattleAction.ActiveUnit.transform.position;
        var end = BattleAction.Destination;
        BattleAction.Path = BattleGrid.getGridPath(start, end);

        foreach(var step in BattleAction.Path)
        {
            var stepCursor = Instantiate(cellBorder) as SpriteRenderer;
            stepCursor.color = Color.green;
            stepCursor.transform.position = step;
            PathSteps.Add(stepCursor); 
        }
        

    }

    public void ClearPathSteps()
    {
        if (PathSteps.Count() <= 0)
            return;

        foreach(var step in PathSteps)
        {
            if(step != null)
                Destroy(step.gameObject); 
        }


    }


	#endregion


	#region Utility Functions

	public List<UnitModel> GetUnitsFromPositions (List<Vector3> _positions)
	{
		var result = new List<UnitModel> ();

		foreach (var _position in _positions)
		{
			var _cell = BattleGrid.GetCell (_position);

			if (_cell.unit != null)
			{
				result.Add (_cell.unit);
			}
		}

		return result;

	}

	public List<Vector3> GetUnitPositions (List<UnitModel> _Units)
	{
		var result = new List<Vector3> ();

		foreach (var _Unit in _Units)
		{
			result.Add (_Unit.transform.position);
		}

		return result;

	}


	#endregion


}

#region Enums

public enum BattleContext
{
	nothing_selected = 0,
	unit_default = 1,
	Deployment = 2,
	unit_Action = 3
}

public enum HightlightedContext
{
	nothing = 0,
	unit = 1,
	enemy = 2,
	target = 3,
	move = 4
}
#endregion
