using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;

public class Turn : MonoBehaviour
{
	public static List<UnitModel> Units;



}

public enum TurnPhase
{
	
}

public class JKSort
{


	static int Partition_Speed (List<UnitModel> list, int left, int right)
	{
		UnitModel pivot = list [left];

		while (true)
		{
			while (list [left].Speed > pivot.Speed)
				left++;

			while (list [right].Speed < pivot.Speed)
				right--;

			if (list [right].Speed == pivot.Speed && list [left].Speed == pivot.Speed)
				left++;

			if (left < right)
			{
				UnitModel temp = list [left];
				list [left] = list [right];
				list [right] = temp;
			} else
			{
				return right;
			}
		}
	}

	public static List<Vector3> SortUnits_Speed (List<UnitModel> list, int left, int right)
	{
		var result = new List<Vector3> ();


		if (left < right)
		{
			int pivotIdx = Partition_Speed (list, left, right);

			if (pivotIdx > 1)
				SortUnits_Speed (list, left, pivotIdx - 1);

			if (pivotIdx + 1 < right)
				SortUnits_Speed (list, pivotIdx + 1, right);
		}

		foreach (var _unit in list)
		{
			result.Add (_unit.transform.position);
		}

		return result;
	}


}