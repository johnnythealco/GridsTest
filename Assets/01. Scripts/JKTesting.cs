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

	void Start ()
	{
	
	}


	void Update ()
	{
	
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
		string type = "DeployUnit";
		var _param1 = JsonUtility.ToJson (_unit);
		var _param2 = JsonUtility.ToJson (_position);
		var LocalPlayer = GameObject.Find ("Local Player").GetComponent<ClientInput> ();
		LocalPlayer.CmdBattleCommand (type, _param1, _param2);
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


}
