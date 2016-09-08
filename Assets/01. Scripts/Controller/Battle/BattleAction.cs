using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class BattleAction : MonoBehaviour
{
	#region Properties

	public static List<Vector3> LegalMoves{ get; set; }

	public static List<Vector3> LegalTargets{ get; set; }

	public static TargetType currentTarget{ get; set; }

	#endregion

	#region Action Execution Network Recievers

	public static void StartBattle ()
	{ 
		var jktesting = GameObject.Find ("! JKTESTING !").GetComponent<JKTesting> (); 
		jktesting.QuickDeploy ();
	}

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


	#endregion

	#region Action Local Revievers

	public static void Action_Click (string _action)
	{
		var _ActiveUnit = Battle.TurnManager.activeUnit;

		switch (_action)
		{
		case "Move":
			{
				Debug.Log (_ActiveUnit.DsiplayName + " " + _ActiveUnit.selectedAction);
			}
			break;
		case "Attack":
			{
				Debug.Log (_ActiveUnit.DsiplayName + " " + _ActiveUnit.selectedAction);
			}
			break;
		case "Evade":
			{
				Debug.Log (_ActiveUnit.DsiplayName + " " + _ActiveUnit.selectedAction); 
			}
			break;
		case "End Turn":
			{ 
				Debug.Log (_ActiveUnit.DsiplayName + " " + _ActiveUnit.selectedAction);
				Battle.TurnManager.EndUnitTurn ();
			}
			break;
		}
	}

	#endregion

	#region Action Ececution Methods

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


		var _target = BattleGrid.GetCell (_targetPosition).unit;
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
		var unitType = register.GetUnitType (_unit.UnitType);

		var unitModel = (UnitModel)Instantiate (unitType); 
		unitModel.transform.position = _position;
		unitModel.setUnitState (_unit);

		//Register on BattelGrid
		BattleGrid.RegisterUnit (_position, unitModel, CellContext.unit);


		JKLog.Log (_unit.Owner + " Deployed a " + _unit.UnitType + " to " + _position.ToString ());

		return true;
	}

	#endregion


	#region Range and Target Methods

	public static void GetLegalMoves (UnitModel _unit)
	{
		BattleAction.LegalMoves = Game.BattleManager.BattleGrid.GetRange (_unit.transform.position, _unit.Engines); 

	}

	public static void GetLegalTargets (UnitModel _unit)
	{
		if (LegalTargets == null)
			LegalTargets = new List<Vector3> ();
		
		LegalTargets.Clear (); 
		switch (_unit.selectedAction)
		{
		case "Move":
			{
				return;
			}
		case "Attack":
			{
				var _weapon = Game.Register.GetWeapon (_unit.selectedWeapon);
				if (_weapon != null)
				{
					LegalTargets.AddRange (getEnemyTargetsInRange (_unit, _weapon));
				}
			}
			break;
		case "Evade":
			{
				LegalTargets.Add (_unit.transform.position);
			}
			break;
		case "End Turn":
			{
				LegalTargets.Add (_unit.transform.position);
			}
			break;
		}
	}

	static List<Vector3> getEnemyTargetsInRange (UnitModel _unit, Weapon _weapon)
	{
		List<Vector3> result = new List<Vector3> ();

		var _grid = Game.BattleManager.BattleGrid;
		var occupiedCells = _grid.occupiedCells;


		foreach (var _cell in occupiedCells)
		{
			if (_cell.context == CellContext.unit && _cell.unit != null)
			{
				if (_cell.unit.faction != _unit.faction)
				{
					var _targetPostion = _cell.transform.position;
					var _attackerPosition = _unit.transform.position;  
					var distance = _grid.getPathToTarget (_attackerPosition, _targetPostion).Count (); 
					if (distance <= _weapon.range)
					{
						result.Add (_targetPostion);
					}
				}	
			}
		}
		return result;

	}

	#endregion


	#region Action Context Getters

	public static HightlightedContext GetActionContext (string _action, Vector3 _point)
	{


		switch (_action)
		{
		case "Move":
			{
				return GetContextForMove (_point);
			}
		case "Attack":
			{
				return GetContextForAttack (_point);
			}
		case "Evade":
			{
				return GetContextForEvade (_point);
			}
		case "End Turn":
			{
				return GetContextForEndTurn (_point);
			}
		}

		return 0;
	}

	static HightlightedContext GetContextForMove (Vector3 _point)
	{
		
		var _cell = Game.BattleManager.BattleGrid.GetCell (_point);

		switch (_cell.context)
		{
		case CellContext.empty:
			{
				if (BattleAction.LegalMoves.Contains (_point))
					return HightlightedContext.move;
				else
					return HightlightedContext.nothing;
			}
		case CellContext.unit:
			{
				var _highlightedUnit = _cell.unit;

				if (_highlightedUnit.faction == Game.PlayerName)
				{
					return HightlightedContext.unit;
				} else
				{
					return HightlightedContext.enemy;
				}
			}
		}

		return HightlightedContext.nothing;
	}

	static HightlightedContext GetContextForAttack (Vector3 _point)
	{
		if (LegalTargets.Contains (_point))
		{
			return HightlightedContext.target;
		} else
		{		
			var _cell = Game.BattleManager.BattleGrid.GetCell (_point);
			switch (_cell.context)
			{
			case CellContext.empty:
				{				
					return HightlightedContext.nothing;
				}
			case CellContext.unit:
				{				
					var _highlightedUnit = _cell.unit;
					if (_highlightedUnit.faction == Game.PlayerName)
					{
						return HightlightedContext.unit;
					} else
					{
						return HightlightedContext.enemy;
					}
				}
			}
		}
	

		return HightlightedContext.nothing;
		
	}

	static HightlightedContext GetContextForEvade (Vector3 _point)
	{

		var _cell = Game.BattleManager.BattleGrid.GetCell (_point);

		switch (_cell.context)
		{
		case CellContext.empty:
			{
				return HightlightedContext.nothing;
			}
		case CellContext.unit:
			{
				var _highlightedUnit = _cell.unit;

				if (_highlightedUnit.faction == Game.PlayerName)
				{
					return HightlightedContext.unit;
				} else
				{
					return HightlightedContext.enemy;
				}
			}
		}

		return HightlightedContext.nothing;
	}

	static HightlightedContext GetContextForEndTurn (Vector3 _point)
	{

		var _cell = Game.BattleManager.BattleGrid.GetCell (_point);

		switch (_cell.context)
		{
		case CellContext.empty:
			{
				return HightlightedContext.nothing;
			}
		case CellContext.unit:
			{
				var _highlightedUnit = _cell.unit;

				if (_highlightedUnit.faction == Game.PlayerName)
				{
					return HightlightedContext.unit;
				} else
				{
					return HightlightedContext.enemy;
				}
			}
		}

		return HightlightedContext.nothing;
	}

	#endregion
}
