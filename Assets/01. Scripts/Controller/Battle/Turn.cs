using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class Turn : NetworkBehaviour
{
	public  List<UnitModel> Units;
	public UnitModel activeUnit;
    public UnitModel activeTarget; 

    public delegate void TurnDelegate ();

	public event TurnDelegate onUnitStartTrun;
	public event TurnDelegate onUnitEndTrun;


	void Awake ()
	{
		Battle.TurnManager = this;
		Units = new List<UnitModel> ();
	}

	public void StartTurn ()
	{
		activeUnit = Units [0];
		StartUnitTurn ();
	}

	public void StartUnitTurn ()
	{
		if (onUnitStartTrun != null)
			onUnitStartTrun.Invoke ();
	}


	public void EndUnitTurn ()
	{
		if (onUnitEndTrun != null)
			onUnitEndTrun.Invoke ();
	}

	public void NextUnit ()
	{
		var i = Units.IndexOf (activeUnit);

		if (i < Units.Count () - 1)
		{
			activeUnit = Units [i + 1];
		} else
		{
			activeUnit = Units [0];
		}

		StartUnitTurn ();


	}

	#region Sorting

	[Command]
	public  void CmdSortList ()
	{
		Units.Clear ();
		Units.AddRange (Battle.AllUnits);
		SortUnits_Speed (Units, 0, Units.Count () - 1);  

		var turnList = new TurnList ();
		turnList.positions = Game.BattleManager.GetUnitPositions (Units); 
		var JSON = JsonUtility.ToJson (turnList);

		RpcUpdateTurnOrder (JSON);
	}

	[ClientRpc]
	public void RpcUpdateTurnOrder (string _UnitPostions)
	{
		var turnlist = (TurnList)JsonUtility.FromJson<TurnList> (_UnitPostions);


		Units = Game.BattleManager.GetUnitsFromPositions (turnlist.positions);
		Battle.TurnManager.StartTurn ();


	}


	int Partition_Speed (List<UnitModel> list, int left, int right)
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

	void SortUnits_Speed (List<UnitModel> list, int left, int right)
	{
		if (left < right)
		{
			int pivotIdx = Partition_Speed (list, left, right);

			if (pivotIdx > 1)
				SortUnits_Speed (list, left, pivotIdx - 1);

			if (pivotIdx + 1 < right)
				SortUnits_Speed (list, pivotIdx + 1, right);
		}

	}

	#endregion
}

public enum TurnPhase
{
	
}

[System.Serializable]
public class TurnList
{
	public List<Vector3> positions;

}

