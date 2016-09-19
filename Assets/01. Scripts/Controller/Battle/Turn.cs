using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class Turn : NetworkBehaviour
{
    public List<UnitModel> Units;
    public UnitModel PlayerNextUnit;

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
        BattleAction.ActiveUnit = Units [0];
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

    [Command]
    public void CmdNextUnit ()
	{
        RpcNextUnit();
    }

    [ClientRpc]
    public void RpcNextUnit()
    {
        var i = Units.IndexOf(BattleAction.ActiveUnit);

        if (i < Units.Count() - 1)
        {
            BattleAction.ActiveUnit = Units[i + 1];

            if (BattleAction.ActiveUnit.unit.Owner != Game.PlayerName)
            {
                GetNextUnitforLocalPlayer();
            }
            else
                PlayerNextUnit = null;

            StartUnitTurn();
        }
        else
        {
            StartTurn();
        }

        
    }

    void GetNextUnitforLocalPlayer()
    {
        var indexofActiveUnit = Units.IndexOf(BattleAction.ActiveUnit);

        int unitsRemaining = indexofActiveUnit - Units.Count() + 1;

        for (int i = 1;i < unitsRemaining; i++ )
        {
            int index = indexofActiveUnit + i;
            var _unit = Units[index];

            if (_unit.unit.Owner == Game.PlayerName)
            {
                PlayerNextUnit = _unit;
                return;
            }
        }

        for (int i = 0; i < indexofActiveUnit; i++)
        {
            int index = i;
            var _unit = Units[index];

            if (_unit.unit.Owner == Game.PlayerName)
            {
                PlayerNextUnit = _unit;
                return;
            }
        }

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

