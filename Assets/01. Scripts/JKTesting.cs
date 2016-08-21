using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;

public class JKTesting : MonoBehaviour
{
	public FlatHex flatHex;
	public SpriteRenderer cellBorder;
	public Unit scout;
	public Unit Destroyer;
	public Faction myFaction;
	public Faction player1;
	public Faction player2;
	public SelectionContext selectionContext;
	public HightlightedContext highlightedContext;

	FlatHex BattleGrid;
	SpriteRenderer gridCursor;
	Vector3 selectedgridPoint;
	Unit selectedUnit;
	Unit highlightedUnit;
	SpriteRenderer selectedUnitCursor;
	List<Vector3> highlightedCells = new List<Vector3> ();

	void Start ()
	{
		BattleGrid = Instantiate (flatHex) as FlatHex;
		BattleGrid.BuildGrid ();
		BattleGrid.onClickCell += BattleGrid_onClickCell;
		BattleGrid.onMouseOverCell += BattleGrid_onMouseOverCell;
		BattleGrid.onRightClickCell += BattleGrid_onRightClickCell;
		selectionContext = SelectionContext.nothing;
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.S) && BattleGrid.GetCellAccessiblity (selectedgridPoint))
		{
			var unit = Instantiate (scout);
			unit.transform.position = selectedgridPoint;
			BattleGrid.RegisterObject (selectedgridPoint, unit.gameObject, CellContents.unit);
			unit.faction = player1;
		}

		if (Input.GetKeyDown (KeyCode.E) && BattleGrid.GetCellAccessiblity (selectedgridPoint))
		{
			var unit = Instantiate (scout);
			unit.transform.position = selectedgridPoint;
			BattleGrid.RegisterObject (selectedgridPoint, unit.gameObject, CellContents.unit);
			unit.faction = player2;
		}
	}


	void BattleGrid_onRightClickCell (Vector3 _point, Cell _cell)
	{


	}

	void BattleGrid_onMouseOverCell (Vector3 _point, Cell _cell)
	{
		if (gridCursor == null)
			gridCursor = Instantiate (cellBorder) as SpriteRenderer;

		var context = _cell.contents; 

		switch (context)
		{
		case CellContents.empty:
			gridCursor.color = Color.gray;
			gridCursor.transform.position = _point;
			selectedgridPoint = _point;
			highlightedUnit = null;
			highlightedContext = HightlightedContext.nothing;
			break;
		case CellContents.unit:			
			highlightUnit (_point, _cell);
			break;
		case CellContents.move:
			highlightedContext = HightlightedContext.move;
			break;
		}



	}

	void BattleGrid_onClickCell (Vector3 _point, Cell _cell)
	{
		switch (selectionContext)
		{
		case SelectionContext.nothing:
			{
				switch (highlightedContext)
				{
				case HightlightedContext.enemy:
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
				case HightlightedContext.enemy:
					break;
				case HightlightedContext.unit:
					selectUnit (_point, _cell);
					break;
				case HightlightedContext.nothing:
					deselectUnit ();
					break;
				case HightlightedContext.move:
					moveUnit (_point, _cell);
					break;

				}
			}
			break;
		
		}



	}

	public enum SelectionContext
	{
		nothing = 0,
		unit = 1,
		enemy = 2
	}

	public enum HightlightedContext
	{
		nothing = 0,
		unit = 1,
		enemy = 2,
		move = 3
	}


	void highlightUnit (Vector3 _point, Cell _cell)
	{
		gridCursor.transform.position = _point;
		selectedgridPoint = _point;
		highlightedUnit = _cell.obj.GetComponent<Unit> ();

		if (highlightedUnit.faction == myFaction)
		{
			highlightedContext = HightlightedContext.unit;
			gridCursor.color = Color.green;
		} else
		{
			highlightedContext = HightlightedContext.enemy;
			gridCursor.color = Color.red;
		}
	}

	void selectUnit (Vector3 _point, Cell _cell)
	{
		selectedUnit = _cell.obj.GetComponent<Unit> ();
		selectionContext = SelectionContext.unit;
		if (selectedUnitCursor == null)
			selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
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

	void highlightMove ()
	{
		if (selectedUnit == null)
			return;
	
		var moves = BattleGrid.GetRange (selectedUnit.transform.position, selectedUnit.Movement);

		foreach (var point in moves)
		{
			var c = BattleGrid.GetCell (point);
			c.Color = Color.gray;
			c.contents = CellContents.move;
		}
		highlightedCells = moves;
	
	}

	void moveUnit (Vector3 _point, Cell _cell)
	{
		BattleGrid.UnRegisterObject (selectedUnit.transform.position);
		BattleGrid.RegisterObject (_point, selectedUnit.gameObject, CellContents.unit);
		selectedUnit.transform.position = _point;
		unHighlightCells (highlightedCells);
		deselectUnit ();
		selectUnit (_point, _cell);


	}

	void unHighlightCells (List<Vector3> _points)
	{
		foreach (var point in _points)
		{
			var cell = BattleGrid.GetCell (point);
			cell.Color = BattleGrid.flatHexCell.Color;
		}
	}



}
