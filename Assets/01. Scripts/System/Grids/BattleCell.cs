﻿using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using JK.Grids;
using System.Collections.Generic;

namespace JK
{
	namespace Grids
	{
		public class BattleCell : SpriteCell
		{
			public bool isAccessible = true;
			public bool isBlocking = false;
			public float Cost = 1;
			public CellContents contents;
			public Unit unit;
		}
	}
}

[System.Serializable]
public class BattleCellState
{
	public string name;
	public Vector3 position;
	public bool isAccessible = true;
	public float Cost = 1;
	public CellContents context;

	public BattleCellState ()
	{
	}

	public BattleCellState (BattleCell _cell)
	{
		this.name = _cell.name;
		this.position = _cell.gameObject.transform.position;
		this.isAccessible = _cell.isAccessible;
		this.Cost = _cell.Cost;
		this.context = _cell.contents;

	}
}

[System.Serializable]
public class BattleState
{
	public List<BattleCellState> state;

	public BattleState (List<BattleCellState> _state)
	{
		this.state = _state;
	}

}



public enum CellContents
{
	empty = 0,
	unit = 2

}