using UnityEngine;
using System.Collections;
using JK.Grids;

public class BattleAction : MonoBehaviour
{



	public static bool Execute (string _action, Vector3 _source, Vector3 _target, string _weapon)
	{
		switch (_action)
		{
		case "MoveUnit":
			return MoveUnit (_source, _target);
		case "BasicAttack":
			return BasicAttack (_source, _target, _weapon);
		}
		return false;
	}


	public static bool Execute (string _action, Unit _unit, Vector3 _target)
	{
		switch (_action)
		{
		case "DeployUnit":
			return DeployUnit (_unit, _target);
		}

		return false;
	}


	static bool MoveUnit (Vector3 _start, Vector3 _end)
	{
		if (Game.BattleManager == null)
			return false;
		
		var battleManager = Game.BattleManager;
		var BattleGrid = battleManager.BattleGrid;

		var _unit = BattleGrid.GetCell (_start).unit;
		var path = BattleGrid.getGridPath (_start, _end);

		foreach (var step in path)
		{
			BattleGrid.UnRegisterObject (_unit.transform.position);
			BattleGrid.RegisterUnit (step, _unit, CellContext.unit);
			_unit.transform.position = step;
			JKLog.Log (_unit.faction + " " + _unit.DsiplayName + " Move to " + step.ToString ());
		}

		return true;


	}


	static bool BasicAttack (Vector3 _attackerPostion, Vector3 _targetPosition, string _weapon)
	{
		if (Game.BattleManager == null)
			return false;

		var battleManager = Game.BattleManager;
		var BattleGrid = battleManager.BattleGrid;

//		var _unit = BattleGrid.GetCell (_attackerPostion).unit;
		var _target = BattleGrid.GetCell (_targetPosition).unit;


//		var damage = _unit.getDamage (_target.Size);

		var destroyed = _target.HitBy (_weapon);



		if (destroyed)
		{
			BattleGrid.UnRegisterObject (_target.transform.position);
			_target.DestroyUnit ();
			JKLog.Log (_target.faction + " " + _target.DsiplayName + " was destroyed! : ( ");

		}

		return true;
	}


	static bool DeployUnit (Unit _unit, Vector3 _position)
	{
		if (Game.BattleManager == null)
			return false;

		var battleManager = Game.BattleManager;
		var BattleGrid = battleManager.BattleGrid;

		var register = Game.Manager.register;
		var unitType = register.GetUnitType (_unit.unitType);

		var unitModel = (UnitModel)Instantiate (unitType); 
		unitModel.transform.position = _position;
		unitModel.setUnitState (_unit);

		//Register on BattelGrid
		BattleGrid.RegisterUnit (_position, unitModel, CellContext.unit);


		JKLog.Log (_unit.faction + " Deployed a " + _unit.unitType + " to " + _position.ToString ());

		return true;
	}

	static int GetActionRange (string _action, UnitModel _unit)
	{
		var _selectedWeapon = Game.Register.GetWeapon (_unit.selectedWeapon); 

		switch (_action)
		{
		case "Move":
			{
				return _unit.currentMovement;
			}
		case "Attack":
			{
				return _selectedWeapon.range;
			}
		case "Evade":
			{
				return 0;
			}
		}

		return 0;
	}

}
