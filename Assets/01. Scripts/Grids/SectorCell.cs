using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;


public class SectorCell : SpriteCell
{

	public bool isAccessible = true;
	public float Cost = 1;
	public Contents contents;







	public enum Contents
	{
		empty,
		unit

	}
}

