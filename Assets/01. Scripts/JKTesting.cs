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

	public string myFaction = "player1";
	public SelectionContext selectionContext;
	public HightlightedContext highlightedContext;

	public BattleState battleState;

	FlatHex BattleGrid;
	SpriteRenderer gridCursor;
	public Vector3 selectedPoint;
	public Vector3 highlightedPoint;
	UnitModel selectedUnit;
	UnitModel highlightedUnit;


	SpriteRenderer selectedUnitCursor;
	List<Vector3> highlightedMoves = new List<Vector3> ();

	void Start ()
	{
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

			var newUnit = new Unit (newUnitType, myFaction);
			DeployUnit (newUnit, highlightedPoint); 

		}


//
//		if (Input.GetKeyDown (KeyCode.Z))
//		{
//			SaveState ();
//		}
	}

	void BattleGrid_onRightClickCell (Vector3 _point, BattleCell _cell)
	{


	}

	void BattleGrid_onMouseOverCell (Vector3 _point, BattleCell _cell)
	{
		switch (_cell.context)
		{
		case CellContext.empty:
			highlightEmptyCell (_point, _cell);
			break;
		case CellContext.unit:			
			highlightUnit (_point, _cell);
			break;
		case CellContext.move:
			highlightedContext = HightlightedContext.move;
			break;
		}
	}

	void BattleGrid_onClickCell (Vector3 _point, BattleCell _cell)
	{
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
					highlightMove ();
					break;
				case HightlightedContext.nothing:
					deselectUnit ();
					break;
				}
			}
			break;
		case SelectionContext.unit:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.target:
					break;
				case HightlightedContext.unit:
					selectUnit (_point, _cell);
					highlightMove ();
					break;
				case HightlightedContext.nothing:
					deselectUnit ();
					unHighlightMoves (highlightedMoves);
					break;
				case HightlightedContext.move:
					moveUnit (_point, _cell);
					deselectUnit ();
					highlightUnit (_point, _cell);
					break;
				}
			}
			break;
		}
	}

	#region Cell Highlighting

	void highlightEmptyCell (Vector3 _point, BattleCell _cell)
	{
		gridCursor.color = Color.gray;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);
		highlightedPoint = _point;
		highlightedUnit = null;
		highlightedContext = HightlightedContext.nothing;
	}

	void highlightUnit (Vector3 _point, BattleCell _cell)
	{
		gridCursor.transform.position = _point;
		highlightedPoint = _point;
		highlightedUnit = _cell.unit;

		if (highlightedUnit.faction == myFaction)
		{
			highlightedContext = HightlightedContext.unit;
			gridCursor.color = Color.green;
		} else
		{
			highlightedContext = HightlightedContext.target;
			gridCursor.color = Color.red;
		}
	}

	void highlightMove ()
	{
		if (selectedUnit == null)
			return;

		var moves = BattleGrid.GetRange (selectedUnit.transform.position, selectedUnit.Movement);
	

		foreach (var point in moves)
		{
			var c = BattleGrid.GetCell (point);
			c.Color = Color.gray;
			c.context = CellContext.move;
		}
		highlightedMoves = moves;

	}

	void unHighlightMoves (List<Vector3> _points)
	{
		foreach (var point in _points)
		{
			var cell = BattleGrid.GetCell (point);
			cell.Color = BattleGrid.flatHexCell.Color;
			if (cell.context == CellContext.move)
				cell.context = CellContext.empty;
		}

	}

	#endregion

	#region Cell Selection

	void selectUnit (Vector3 _point, BattleCell _cell)
	{
		selectedUnit = _cell.unit;

		selectionContext = SelectionContext.unit;

		selectedUnitCursor.gameObject.SetActive (true);
		selectedUnitCursor.color = Color.blue;
		selectedUnitCursor.transform.position = _point;
	}

	void deselectUnit ()
	{
		selectedUnit = null;
		selectionContext = SelectionContext.nothing;
		selectedUnitCursor.gameObject.SetActive (false);
	}


	void moveUnit (Vector3 _point, BattleCell _cell)
	{
		BattleGrid.UnRegisterObject (selectedUnit.transform.position);
		BattleGrid.RegisterUnit (_point, selectedUnit, CellContext.unit);
		selectedUnit.transform.position = _point;
		unHighlightMoves (highlightedMoves);
		deselectUnit ();
		selectUnit (_point, _cell);
	}

	#endregion


	void DeployUnit (Unit _unit, Vector3 _position)
	{

		var register = Game.Manager.register;
		var unitType = register.GetUnitType (_unit.unitType);
		var unitModel = (UnitModel)Instantiate (unitType); 
		unitModel.transform.position = _position;
		unitModel.setUnitState (_unit);

		//Register on BattelGrid
		BattleGrid.RegisterUnit (_position, unitModel, CellContext.unit);		
	}


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
	target = 2
}

public enum HightlightedContext
{
	nothing = 0,
	unit = 1,
	target = 2,
	move = 3
}
