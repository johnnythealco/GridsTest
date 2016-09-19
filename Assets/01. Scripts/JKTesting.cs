using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class JKTesting : MonoBehaviour
{
	public List<string> JKFleet;
	public List<string> EnemyFleet;
	System.Random rnd = new System.Random ();


	void Update ()
	{


		if (Input.GetKeyDown (KeyCode.D))
		{
			var _SortedList = Battle.TurnManager.Units;

			DisplayList (_SortedList);
		}

		if (Input.GetKeyDown (KeyCode.Space))
		{
			Game.BattleManager.StartBattle ();
		}

        if(Input.GetKeyDown(KeyCode.L))
        {
            JKLog.Log(" Testing Log Entry !");
        }
	
	}

	List<Unit> CreateUnits (List<string> _UnitTypes, string _Owner)
	{
		var result = new List<Unit> ();
		foreach (var _unit in _UnitTypes)
		{
			var newUnitType = Game.Manager.register.GetUnitType (_unit);

			var newUnit = new Unit (newUnitType, _Owner);

			result.Add (newUnit);
		}
		return result;
	}


	void netWorkDeploy (Unit _unit, Vector3 _position)
	{
		var _unitJSON = JsonUtility.ToJson (_unit);		
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdDeploy (_unitJSON, _position);
	}


	public void QuickDeploy ()
	{
		
		var _JKFleet = CreateUnits (JKFleet, Game.PlayerName); 
		foreach (var _unit in _JKFleet)
		{
			var i = rnd.Next (Game.GridPoints.Count ());
			var _point = Game.GridPoints [i];
			while (Game.BattleManager.BattleGrid.GetCellAccessiblity (_point) == false)
			{
				i = rnd.Next (Game.GridPoints.Count ());
				_point = Game.GridPoints [i];
			}

			netWorkDeploy (_unit, _point);
		}

		var _EnemyFleet = CreateUnits (EnemyFleet, "Enemy"); 
		foreach (var _unit in _EnemyFleet)
		{
			var i = rnd.Next (Game.GridPoints.Count ());
			var _point = Game.GridPoints [i];
			while (Game.BattleManager.BattleGrid.GetCellAccessiblity (_point) == false)
			{
				i = rnd.Next (Game.GridPoints.Count ());
				_point = Game.GridPoints [i];
			}

			netWorkDeploy (_unit, _point);
		}
	}



	void DisplayList (List<UnitModel> _Units)
	{
		foreach (var _unit in _Units)
		{
			Debug.Log (_unit.DsiplayName + ":  Speed :" + _unit.Speed.ToString ());

		}

	}



}
