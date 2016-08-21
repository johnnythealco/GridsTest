using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;

namespace JK
{
	namespace Grids
	{
		public class Cell : SpriteCell
		{

			public bool isAccessible = true;
			public float Cost = 1;
			public CellContents contents;
			public GameObject obj;


		}
	}
}


public enum CellContents
{
	empty = 0,
	move = 1,
	unit = 2

}