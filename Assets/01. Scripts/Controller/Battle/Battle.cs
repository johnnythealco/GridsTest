using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.IO;

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

	BattleContext battleContext;
	HightlightedContext highlightedContext;

	Vector3 highlightedPoint;
	BattleCell highlightedCell;

	UnitModel selectedUnit;
	UnitModel highlightedUnit;

	string selectedUntyDeploy;



	public List<Vector3> moves = new List<Vector3> ();
	public List<Vector3> targets = new List<Vector3> ();

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
		gridCursor.gameObject.SetActive (false);


		selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
		selectedUnitCursor.gameObject.SetActive (false);

		deploymentManager.onSelectUnit += DeploymentManager_onSelectUnit;
		deploymentManager.onEndDeployment += DeploymentManager_onEndDeployment;

	}




	void Update ()
	{
		


	}

	#endregion

	#region Event handlers

	void BattleGrid_onRightClickCell (Vector3 _point, BattleCell _cell)
	{
		cameraCTRL.CentreOn (_point);

	}

	void BattleGrid_onMouseOverCell (Vector3 _point, BattleCell _cell)
	{
//		if (Battle.TurnManager.activeUnit == null) 
//			return;
//		
//		var _cell = BattleGrid.GetCell(_point);
//		var _cellContents = _cell.context;
//
//		switch(_cellContents)
//		{
//		case CellContext.empty:
//			BattleAction.currentTarget = TargetType.empty;
//			break;
//		case CellContext.unit:
//			{
//				if(_cell.unit.unit.Owner == Game.PlayerName)
//					BattleAction.currentTarget = TargetType.ally;
//				else
//					BattleAction.currentTarget = TargetType.enemy;
//			}
//			break;
//		}
////
//		switch(BattleAction.currentTarget)
//		{
//		case TargetType.empty:
//			
//		}


	}

	void BattleGrid_onClickCell (Vector3 _point, BattleCell _cell)
	{
		selectCell (_point, _cell);
	}

	void BattleGrid_onNoCellSelected ()
	{
		gridCursor.gameObject.SetActive (false);
		highlightedContext = HightlightedContext.nothing;
	}

	void DeploymentManager_onSelectUnit (string _unit)
	{
		battleContext = BattleContext.Deployment;
		selectedUntyDeploy = _unit;
	}


	void DeploymentManager_onEndDeployment ()
	{
		battleContext = BattleContext.nothing_selected;
		deploymentManager.gameObject.SetActive (false);

		deploymentManager.onSelectUnit -= DeploymentManager_onSelectUnit;
		deploymentManager.onEndDeployment -= DeploymentManager_onEndDeployment;

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
		selectUnit (_point);
	
	}


	void OnUnitEndTrun ()
	{
		Battle.TurnManager.NextUnit ();
	}

	public void StartBattle ()
	{

		Battle.TurnManager.CmdSortList ();


	
	}



	#endregion

	#region Set Context

	void highlightCell (Vector3 _point, BattleCell _cell)
	{
		highlightedPoint = _point;
		highlightedCell = _cell;
		getCellContext (_point, _cell);

		switch (battleContext)
		{
		case BattleContext.nothing_selected:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightEmptyCell (_point, _cell);
					break;
				case HightlightedContext.unit:
					highlightUnit (_point, _cell);
					break;			
				case HightlightedContext.enemy:
					highlightEnemy (_point, _cell);
					break;
				}

			}
			break;
		case BattleContext.unit_default:	
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightEmptyCell (_point, _cell);
					break;
				case HightlightedContext.unit:
					highlightUnit (_point, _cell);
					break;
				case HightlightedContext.target:
					highlightTarget (_point, _cell);
					break;
				case HightlightedContext.move:
					highlightMove (_point, _cell);
					break;
				case HightlightedContext.enemy:
					highlightEnemy (_point, _cell);
					break;
				}
			}
			break;
		case BattleContext.Deployment:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					highlightDeployment (_point, _cell);
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

	void getCellContext (Vector3 _point, BattleCell _cell)
	{
		if (selectedUnit != null)
		{
			highlightedContext = BattleAction.GetActionContext (selectedUnit.selectedAction, _point);
		} else
		{

			switch (_cell.context)
			{
			case CellContext.empty:
				{
					if (moves.Contains (_point))
						highlightedContext = HightlightedContext.move;
					else
						highlightedContext = HightlightedContext.nothing;
				}
				break;

			case CellContext.unit:
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

	void highlightEmptyCell (Vector3 _point, BattleCell _cell)
	{
		gridCursor.color = Color.gray;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	
		highlightedUnit = null;
	}

	void highlightDeployment (Vector3 _point, BattleCell _cell)
	{
		gridCursor.color = Color.yellow;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	
		highlightedUnit = null;
	}

	void highlightUnit (Vector3 _point, BattleCell _cell)
	{
		gridCursor.transform.position = _point;
		highlightedUnit = _cell.unit;
		gridCursor.color = Color.green;
	}

	void highlightTarget (Vector3 _point, BattleCell _cell)
	{
		gridCursor.transform.position = _point;
		highlightedUnit = _cell.unit;
		gridCursor.color = Color.red;
	}

	void highlightEnemy (Vector3 _point, BattleCell _cell)
	{
		gridCursor.transform.position = _point;
		highlightedUnit = _cell.unit;
		gridCursor.color = Color.magenta;
	}

	void highlightMove (Vector3 _point, BattleCell _cell)
	{
		gridCursor.transform.position = _point;
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
			if (cell.context == CellContext.unit)
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

	void selectCell (Vector3 _point, BattleCell _cell)
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
					highlightCell (_point, _cell);
					break;
				case HightlightedContext.target:
					netWorkBasicAttack (selectedUnit.transform.position, highlightedPoint);
					clearMoves ();
					clearSelection ();
					highlightCell (highlightedPoint, highlightedCell);
					break;
				case HightlightedContext.enemy:
					break;
				}
			}
			break;
		case BattleContext.Deployment:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					deployUnit (_point, _cell);
					break;
				case HightlightedContext.unit:
					break;
				}
			}
			break;
		}
	}

	void selectUnit (Vector3 _point)
	{
		var _cell = BattleGrid.GetCell (_point);
		selectedUnit = _cell.unit;
		BattleAction.GetLegalMoves (selectedUnit);
		BattleAction.GetLegalTargets (selectedUnit);
		unitDisplay.Prime (selectedUnit);

		unitDisplay.gameObject.SetActive (true);

		battleContext = BattleContext.unit_default;

		selectedUnitCursor.gameObject.SetActive (true);
		selectedUnitCursor.color = Color.blue;
		selectedUnitCursor.transform.position = _point;
	}

	void deployUnit (Vector3 _point, BattleCell _cell)
	{
		if (deploymentManager.BuildUnit ())
		{
			var newUnitType = Game.Manager.register.GetUnitType (selectedUntyDeploy);

			var newUnit = new Unit (newUnitType, Game.PlayerName);
			netWorkDeploy (newUnit, highlightedPoint);
			highlightCell (highlightedPoint, highlightedCell);
		}
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
