using UnityEngine;
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

            public void SetState(BattleCellState _state)
            {
                this.isAccessible = _state.isAccessible;
                this.isBlocking = _state.isBlocking;
                this.Cost = _state.Cost;
                this.contents = _state.context;
                if(_state.unit != null)
                {
                    this.unit = Game.BattleManager.CreateUnit(_state.unit);
                    this.unit.transform.position = this.transform.position;
                }
            }
        }


	}
}




public enum CellContents
{
	empty = 0,
	unit = 2

}