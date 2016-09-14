using UnityEngine;
using System.Collections.Generic;
using JK.Grids;

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
    BattleContext battleContext;
	HightlightedContext highlightedContext;
	Vector3 highlightedPoint;
	UnitModel selectedUnit;
	UnitModel highlightedUnit;


	public List<Vector3> moves = new List<Vector3> ();
	public List<Vector3> targets = new List<Vector3> ();
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

		battleContext = BattleContext.nothing_selected;
		gameLog.gameObject.SetActive (true);


		gridCursor = Instantiate (cellBorder) as SpriteRenderer;
	    selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
        selectedTargetCursor = Instantiate(cellBorder) as SpriteRenderer;
        selectedUnitCursor.gameObject.SetActive (false);
        selectedTargetCursor.gameObject.SetActive(false);
        gridCursor.gameObject.SetActive(false);

    }




	void Update ()
	{
		


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
            highlightMove(_point);
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
		
	}

	void BattleGrid_onNoCellSelected ()
	{
		gridCursor.gameObject.SetActive (false);
		highlightedContext = HightlightedContext.nothing;
	}

	public void Unit_Action_Click ()
	{
		if (selectedUnit == null)
			return;


	}

	void OnUnitStartTrun ()
	{
		var _point = Battle.TurnManager.activeUnit.transform.position;
		var _unitName = Battle.TurnManager.activeUnit.DsiplayName;
		var _owner = Battle.TurnManager.activeUnit.unit.Owner; 
		cameraCTRL.CentreOn (_point);
		JKLog.Log ("Starting Turn for " + _unitName + " Controlled by " + _owner);
		unitDisplay.onChangeAction += ActiveUnit_onChangeAction;

		selectUnit (_point);
	
	}

	void OnUnitEndTrun ()
	{
		unitDisplay.onChangeAction -= ActiveUnit_onChangeAction;
		Battle.TurnManager.NextUnit ();
	}

	void ActiveUnit_onChangeAction ()
	{
		
	}

	public void StartBattle ()
	{

		Battle.TurnManager.CmdSortList ();


	
	}



	#endregion

	#region Set Context

	void highlightCell (Vector3 _point)
	{
		highlightedPoint = _point;

		getCellContext (_point);

		switch (battleContext)
		{
		case BattleContext.nothing_selected:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightEmptyCell (_point);
					break;
				case HightlightedContext.unit:
					highlightUnit (_point);
					break;			
				case HightlightedContext.enemy:
					highlightEnemy (_point);
					break;
				}

			}
			break;
		case BattleContext.unit_default:	
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightEmptyCell (_point);
					break;
				case HightlightedContext.unit:
					highlightUnit (_point);
					break;
				case HightlightedContext.target:
					highlightTarget (_point);
					break;
				case HightlightedContext.move:
					highlightMove (_point);
					break;
				case HightlightedContext.enemy:
					highlightEnemy (_point);
					break;
				}
			}
			break;
		case BattleContext.Deployment:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightDeployment (_point);
					break;
				case HightlightedContext.unit:					
					break;				
				case HightlightedContext.enemy:
					break;
				}

			}
			break;
		}
	}

	void getCellContext (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point);

		if (selectedUnit != null)
		{
			highlightedContext = BattleAction.GetActionContext (selectedUnit.selectedAction, _point);
		} else
		{

			switch (_cell.contents)
			{
			case CellContents.empty:
				{
					if (moves.Contains (_point))
						highlightedContext = HightlightedContext.move;
					else
						highlightedContext = HightlightedContext.nothing;
				}
				break;

			case CellContents.unit:
				{
					highlightedUnit = _cell.unit;

					if (highlightedUnit.faction == Game.PlayerName)
					{
						highlightedContext = HightlightedContext.unit;
					} else
					{
						getValidTargets ();

						if (targets.Contains (_point))
						{
							highlightedContext = HightlightedContext.target;
						} else
						{
							highlightedContext = HightlightedContext.enemy;
						}


					}
				}
				break;

			}		
		}
	}

	#endregion

	#region Highlighting

	void highlightEmptyCell (Vector3 _point)
	{
		gridCursor.color = Color.gray;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	
		highlightedUnit = null;
	}

    void highlightActiveUnit(Vector3 _point)
    {
        selectedUnitCursor.gameObject.SetActive(true);
        selectedUnitCursor.color = Color.blue;
        selectedUnitCursor.transform.position = _point;
    }

	void highlightDeployment (Vector3 _point)
	{
		gridCursor.color = Color.yellow;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	
		highlightedUnit = null;
	}

	void highlightUnit (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point); 
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
        highlightedUnit = _cell.unit;
		gridCursor.color = Color.green;
	}

	void highlightTarget (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point); 
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
        highlightedUnit = _cell.unit;
		gridCursor.color = Color.red;
	}

	void highlightEnemy (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point); 
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
        highlightedUnit = _cell.unit;
		gridCursor.color = Color.magenta;
	}

	void highlightMove (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point); 
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
        highlightedUnit = _cell.unit;
		gridCursor.color = Color.green;
	}

	void showLegalMoves ()
	{
		if (selectedUnit == null)
			return;

		foreach (var point in BattleAction.LegalMoves)
		{
			var c = BattleGrid.GetCell (point);
			c.Color = Color.gray;
		}
	}

	void getValidTargets ()
	{
		targets.Clear ();

		if (selectedUnit == null)
			return;

		var occupiedCells = BattleGrid.occupiedCells;
	

		foreach (var cell in occupiedCells)
		{
			if (cell.contents == CellContents.unit)
			{
				if (cell.unit.faction != Game.PlayerName)
				{
					targets.Add (cell.transform.position);
				}
			}
		}
	}

	void clearMoves ()
	{
		foreach (var point in moves)
		{
			var cell = BattleGrid.GetCell (point);
			cell.Color = BattleGrid.flatHexCell.Color;	
		}

		moves.Clear ();

	}

	#endregion

	#region Cell Selection

	void selectCell (Vector3 _point)
	{

		switch (battleContext)
		{
		case BattleContext.nothing_selected:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.target:
					break;
				case HightlightedContext.unit:
					selectUnit (_point);
					showLegalMoves ();
					break;
				case HightlightedContext.nothing:					
					break;
				}
			}
			break;
		case BattleContext.unit_default:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					clearSelection ();
					clearMoves ();
					break;
				case HightlightedContext.unit:
					selectUnit (_point);
					clearMoves ();
					showLegalMoves ();
					break;
				case HightlightedContext.move:
					netWorkMove (selectedUnit.transform.position, highlightedPoint);
					clearMoves ();
					clearSelection ();
					highlightCell (_point);
					break;
				case HightlightedContext.target:
					netWorkBasicAttack (selectedUnit.transform.position, highlightedPoint);
					clearMoves ();
					clearSelection ();
					highlightCell (highlightedPoint);
					break;
				case HightlightedContext.enemy:
					break;
				}
			}
			break;
		}
	}

	void selectUnit (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point);
        var _unit = _cell.unit;

        TurnManager.activeUnit = _unit;
		BattleAction.GetLegalMoves (_unit);
		unitDisplay.Prime (_unit);
        highlightActiveUnit(_point);
    }

    void selectTarget(Vector3 _point)
    {
        var _cell = BattleGrid.GetCell(_point);
        highlightActiveUnit(_point);
    }

    void clearSelection ()
	{
		selectedUnit = null;
		battleContext = BattleContext.nothing_selected;
		selectedUnitCursor.gameObject.SetActive (false);
	}


	#endregion

	#region Actions

	void netWorkDeploy (Unit _unit, Vector3 _position)
	{
		string type = "DeployUnit";
		var _param1 = JsonUtility.ToJson (_unit);
		var _param2 = JsonUtility.ToJson (_position);
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleCommand (type, _param1, _param2);
	}

	void netWorkBasicAttack (Vector3 _attackerPostion, Vector3 _targetPosition)
	{
		var weapon = selectedUnit.selectedWeapon;
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleAction ("BasicAttack", _attackerPostion, _targetPosition, weapon);
	}

	void netWorkMove (Vector3 _position, Vector3 _destination)
	{
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleAction ("MoveUnit", _position, _destination, "none");
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
