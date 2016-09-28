using UnityEngine;
using System.Collections.Generic;
using System;
using JK.Grids;

[System.Serializable]
public class BattleState
{

    public List<BattleCellState> Units;
    public List<Vector3> TurnOrder;


    public BattleState(BattleGrid _Units, List<Vector3> _TurnOrder)
    {
        this.Units = new List<BattleCellState>();
        this.TurnOrder = _TurnOrder;
        var _cells = _Units.Grid.Values;

        foreach(var _cell in _cells)
        {
            if (_cell.isAccessible == false)
            {
                this.Units.Add(new BattleCellState(_cell));
            }
        }
    }

}


[System.Serializable]
public class BattleCellState
{
    public string name;
    public Vector3 position;
    public bool isAccessible = true;
    public bool isBlocking = false;
    public float Cost = 1;
    public CellContents context;
    public UnitState unit;

    public BattleCellState()
    {
    }

    public BattleCellState(BattleCell _cell)
    {
        this.name = _cell.name;
        this.position = _cell.gameObject.transform.position;
        this.isAccessible = _cell.isAccessible;
        this.isBlocking = _cell.isBlocking;
        this.Cost = _cell.Cost;
        this.context = _cell.contents;

        if (this.context == CellContents.unit)
        {
            this.unit = _cell.unit.state;
        }
           
    }
}