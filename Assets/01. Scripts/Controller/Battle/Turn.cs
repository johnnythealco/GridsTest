using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class Turn : NetworkBehaviour
{
    public List<Unit> TurnOrder = new List<Unit>();   

    public delegate void TurnDelegate ();

	public event TurnDelegate onUnitStartTrun;
	public event TurnDelegate onUnitEndTrun;


	public void StartTurn ()
	{
        BattleAction.ActiveUnit = TurnOrder [0];

        if (BattleAction.ActiveUnit.state.Owner != Game.PlayerName)
        {
            GetNextUnitforLocalPlayer();
            Battle.LocalPlayerTurn = false;
        }
        else
        {
            BattleAction.NextUnit = null;
            Battle.LocalPlayerTurn = true;
        }

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
        var i = TurnOrder.IndexOf(BattleAction.ActiveUnit);

        if (i < TurnOrder.Count() - 1)
        {
            BattleAction.ActiveUnit = TurnOrder[i + 1];

            if (BattleAction.ActiveUnit.state.Owner != Game.PlayerName)
            {
                GetNextUnitforLocalPlayer();
                Battle.LocalPlayerTurn = false;
            }
            else
            {
                BattleAction.NextUnit = null;
                Battle.LocalPlayerTurn = true;
            }

            StartUnitTurn();
        }
        else
        {
            StartTurn();
        }

        
    }

    void GetNextUnitforLocalPlayer()
    {
        var indexofActiveUnit = TurnOrder.IndexOf(BattleAction.ActiveUnit);

        int unitsRemaining = indexofActiveUnit - TurnOrder.Count() + 1;

        for (int i = 1;i < unitsRemaining; i++ )
        {
            int index = indexofActiveUnit + i;
            var _unit = TurnOrder[index];

            if (_unit.state.Owner == Game.PlayerName)
            {
                BattleAction.NextUnit = _unit;
                return;
            }
        }

        for (int i = 0; i < indexofActiveUnit; i++)
        {
            int index = i;
            var _unit = TurnOrder[index];

            if (_unit.state.Owner == Game.PlayerName)
            {
                BattleAction.NextUnit = _unit;
                return;
            }
        }

    }

	#region Sorting

	[Command]
	public  void CmdSortList ()
	{
		TurnOrder.Clear ();
		TurnOrder.AddRange (Battle.AllUnits);
		SortUnits_Speed (TurnOrder, 0, TurnOrder.Count () - 1);  

		var turnList = new TurnList ();
		turnList.positions = Game.BattleManager.GetUnitPositions (TurnOrder); 
		var JSON = JsonUtility.ToJson (turnList);

		RpcUpdateTurnOrder (JSON);
	}

	[ClientRpc]
	public void RpcUpdateTurnOrder (string _UnitPostions)
	{
		var turnlist = (TurnList)JsonUtility.FromJson<TurnList> (_UnitPostions);


		TurnOrder = Game.BattleManager.GetUnitsFromPositions (turnlist.positions);
		Battle.TurnManager.StartTurn ();


	}


	int Partition_Speed (List<Unit> list, int left, int right)
	{
		Unit pivot = list [left];

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
				Unit temp = list [left];
				list [left] = list [right];
				list [right] = temp;
			} else
			{
				return right;
			}
		}
	}

	void SortUnits_Speed (List<Unit> list, int left, int right)
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

