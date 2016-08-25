using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.IO;

public class JKTesting : MonoBehaviour
{
	
	public FlatHex flatHex;
	public SpriteRenderer cellBorder;
	public Unit fighter;

	public string enemyFaction = "Enemy";
	public SelectionContext selectionContext;
	public HightlightedContext highlightedContext;

	public BattleState battleState;

	FlatHex BattleGrid;
	SpriteRenderer gridCursor;

	public Vector3 selectedPoint;
	BattleCell selectedCell;

	public Vector3 highlightedPoint;
	BattleCell highlightedCell;

	UnitModel selectedUnit;
	UnitModel highlightedUnit;


	SpriteRenderer selectedUnitCursor;
	List<Vector3> moves = new List<Vector3> ();
	List<Vector3> targets = new List<Vector3> ();

	void Start ()
	{
		Game.BattleManager = this;
		BattleGrid = Instantiate (flatHex) as FlatHex;
		BattleGrid.BuildGrid ();
		BattleGrid.onClickCell += BattleGrid_onClickCell;
		BattleGrid.onMouseOverCell += BattleGrid_onMouseOverCell;
		BattleGrid.onRightClickCell += BattleGrid_onRightClickCell;
		selectionContext = SelectionContext.nothing;

		if (gridCursor == null)
			gridCursor = Instantiate (cellBorder) as SpriteRenderer;

		gridCursor.gameObject.SetActive (false);

		if (selectedUnitCursor == null)
			selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;

		selectedUnitCursor.gameObject.SetActive (false);
	}

	void Update ()
	{
		
		if (Input.GetKeyDown (KeyCode.S) && BattleGrid.GetCellAccessiblity (highlightedPoint))
		{
			var newUnitType = Game.Manager.register.GetUnitType ("Fighter");

			var newUnit = new Unit (newUnitType, Game.PlayerName);
			netWorkDeploy (newUnit, highlightedPoint); 

		}

		if (Input.GetKeyDown (KeyCode.E) && BattleGrid.GetCellAccessiblity (highlightedPoint))
		{
			var newUnitType = Game.Manager.register.GetUnitType ("Fighter");

			var newUnit = new Unit (newUnitType, enemyFaction);
			DeployUnit (newUnit, highlightedPoint); 

		}

		if (Input.GetKeyDown (KeyCode.Space))
			TestCmd ();

	}

	#region Event handlers

	void BattleGrid_onRightClickCell (Vector3 _point, BattleCell _cell)
	{
		if (selectedUnit != null)
		{
			var path = BattleGrid.getPathToTarget (selectedUnit.transform.position, _point);

			foreach (var step in path)
			{
				var cell = BattleGrid.GetCell (step);
				cell.Color = Color.cyan;
			}

		}

	}

	void BattleGrid_onMouseOverCell (Vector3 _point, BattleCell _cell)
	{
		highlightCell (_point, _cell);
	}

	void BattleGrid_onClickCell (Vector3 _point, BattleCell _cell)
	{
		selectCell (_point, _cell);
	}

	#endregion

	#region Cell Highlighting

	void highlightCell (Vector3 _point, BattleCell _cell)
	{
		highlightedPoint = _point;
		highlightedCell = _cell;
		getCellContext (_point, _cell);

		switch (selectionContext)
		{
		case SelectionContext.nothing:
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
				case HightlightedContext.enemy:
					highlightEnemy (_point, _cell);
					break;
				}

			}
			break;
		case SelectionContext.unit:	
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
		}
	}

	void highlightEmptyCell (Vector3 _point, BattleCell _cell)
	{
		gridCursor.color = Color.gray;
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

	void getCellContext (Vector3 _point, BattleCell _cell)
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

	void showMoves ()
	{
		if (selectedUnit == null)
			return;

		moves = BattleGrid.GetRange (selectedUnit.transform.position, selectedUnit.Movement);

		foreach (var point in moves)
		{
			var c = BattleGrid.GetCell (point);
			c.Color = Color.gray;
		}
	}


	void getValidTargets ()
	{
		if (selectedUnit == null)
			return;

		var occupiedCells = BattleGrid.occupiedCells;
		targets.Clear ();

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
			if (cell.context == CellContext.move)
				cell.context = CellContext.empty;
		}

		moves.Clear ();

	}

	#endregion

	#region Cell Selection

	void selectCell (Vector3 _point, BattleCell _cell)
	{
		selectedPoint = _point;
		selectedCell = _cell;
		switch (selectionContext)
		{
		case SelectionContext.nothing:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.target:
					break;
				case HightlightedContext.unit:
					selectUnit (_point, _cell);
					showMoves ();
					break;
				case HightlightedContext.nothing:
					clearSelection ();
					break;
				}
			}
			break;
		case SelectionContext.unit:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.nothing:
					clearSelection ();
					clearMoves ();
					break;
				case HightlightedContext.unit:
					selectUnit (_point, _cell);
					clearMoves ();
					showMoves ();
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
		}
	}

	void selectUnit (Vector3 _point, BattleCell _cell)
	{
		selectedUnit = _cell.unit;

		selectionContext = SelectionContext.unit;

		selectedUnitCursor.gameObject.SetActive (true);
		selectedUnitCursor.color = Color.blue;
		selectedUnitCursor.transform.position = _point;
	}

	void clearSelection ()
	{
		selectedUnit = null;
		selectionContext = SelectionContext.nothing;
		selectedUnitCursor.gameObject.SetActive (false);
	}


	#endregion

	#region Actions

	void netWorkDeploy (Unit _unit, Vector3 _position)
	{
		string type = "Deploy";
		var _param1 = JsonUtility.ToJson (_unit);
		var _param2 = JsonUtility.ToJson (_position);
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleCommand (type, _param1, _param2);
	}

	void netWorkBasicAttack (Vector3 _attackerPostion, Vector3 _targetPosition)
	{
		
		string type = "BasicAttack";
		var _param1 = JsonUtility.ToJson (_attackerPostion);
		var _param2 = JsonUtility.ToJson (_targetPosition);
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleCommand (type, _param1, _param2);
	}

	void netWorkMove (Vector3 _position, Vector3 _destination)
	{
		string type = "Move";
		var _param1 = JsonUtility.ToJson (_position);
		var _param2 = JsonUtility.ToJson (_destination);
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleCommand (type, _param1, _param2);
	}

	public void moveUnit (Vector3 _position, Vector3 _destination)
	{
		var _unit = BattleGrid.GetCell (_position).unit;
		var path = BattleGrid.getGridPath (_position, _destination);

		foreach (var step in path)
		{
			BattleGrid.UnRegisterObject (_unit.transform.position);
			BattleGrid.RegisterUnit (step, _unit, CellContext.unit);
			_unit.transform.position = step;
			JKLog.Log (_unit.faction + " " + _unit.DsiplayName + " Move to " + step.ToString ());
		}

	}

	public void BasicAttack (Vector3 _attackerPostion, Vector3 _targetPosition)
	{
		var _unit = BattleGrid.GetCell (_attackerPostion).unit;
		var _target = BattleGrid.GetCell (_targetPosition).unit;
		
		var destroyed = _target.TakeDirectDamage (_unit.Damage);

		JKLog.Log (_unit.faction + " " + _unit.DsiplayName + " : Attacks " + _target.faction + " " + _target.DsiplayName);
		JKLog.Log ("Dealing " + _unit.Damage + " Damage");

		if (destroyed)
		{
			BattleGrid.UnRegisterObject (_target.transform.position);
			_target.DestroyUnit ();
			JKLog.Log (_target.faction + " " + _target.DsiplayName + " was destroyed! : ( ");

		}
	}

	public void DeployUnit (Unit _unit, Vector3 _position)
	{

		var register = Game.Manager.register;
		var unitType = register.GetUnitType (_unit.unitType);
		var unitModel = (UnitModel)Instantiate (unitType); 
		unitModel.transform.position = _position;
		unitModel.setUnitState (_unit);

		//Register on BattelGrid
		BattleGrid.RegisterUnit (_position, unitModel, CellContext.unit);
		highlightCell (highlightedPoint, highlightedCell);

		JKLog.Log (_unit.faction + " Deployed a " + _unit.unitType + " to " + _position.ToString ());
	}

	void TestCmd ()
	{
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdTest (Game.PlayerName);
	}

	#endregion


	public void SaveState ()
	{
		
		var state = new BattleState (BattleGrid.State);
		var JSON = JsonUtility.ToJson (state, true);

		File.WriteAllText (Application.dataPath + "state.json", JSON);

		Debug.Log (state);
	}
}

public enum SelectionContext
{
	nothing = 0,
	unit = 1,
	enemy = 2,
	target = 3,
	move = 4
}

public enum HightlightedContext
{
	nothing = 0,
	unit = 1,
	enemy = 2,
	target = 3,
	move = 4
}
