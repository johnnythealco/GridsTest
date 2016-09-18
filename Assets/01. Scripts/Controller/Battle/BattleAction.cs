using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class BattleAction : MonoBehaviour
{
    #region Properties

    public static UnitModel ActiveUnit { get; set; }

    public static List<Vector3> LegalMoves{ get; set; }

	public static List<Vector3> LegalTargets{ get; set; }

	public static TargetType CurrentTargetType{ get; set; }

    public static UnitModel ActiveTarget { get; set; }

    public static Vector3 Destination { get; set; }

    public static List<Vector3> Path { get; set; }

	#endregion

	#region Action Execution Network Recievers

	public static void StartBattle ()
	{ 
		var jktesting = GameObject.Find ("! JKTESTING !").GetComponent<JKTesting> (); 
		jktesting.QuickDeploy ();
	}

	public static bool Execute (string _action, Vector3 _target, string _param)
	{
		switch (_action)
		{
		case "MoveUnit":
			return MoveUnit (_target);
		case "BasicAttack":
			return BasicAttack ( _target, _param);
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
		var _ActiveUnit = BattleAction.ActiveUnit;

		switch (_action)
		{
		case "Move":
                {
                    var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
                    var _destination = Destination;
                    LocalPlayer.CmdBattleAction("MoveUnit", _destination, "none");
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

	static bool MoveUnit (Vector3 _end)
	{
		if (Game.BattleManager == null)
			return false;
		
		var battleManager = Game.BattleManager;
		var BattleGrid = battleManager.BattleGrid;

	
        var _start = ActiveUnit.transform.position;
		var path = BattleGrid.getGridPath (_start, _end);

		foreach (var step in path)
		{
			BattleGrid.UnRegisterObject (ActiveUnit.transform.position);
			BattleGrid.RegisterUnit (step, ActiveUnit, CellContents.unit);
            ActiveUnit.transform.position = step;
			JKLog.Log (ActiveUnit.faction + " " + ActiveUnit.DsiplayName + " Move to " + step.ToString ());
		}

        battleManager.ClearPathSteps();
        GetLegalMoves(ActiveUnit);

		return true;


	}

	static bool BasicAttack ( Vector3 _targetPosition, string _weapon)
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
		BattleGrid.RegisterUnit (_position, unitModel, CellContents.unit);


		JKLog.Log (_unit.Owner + " Deployed a " + _unit.UnitType + " to " + _position.ToString ());

		return true;
	}

	#endregion


	#region Range and Target Methods

	public static void GetLegalMoves (UnitModel _unit)
	{
		BattleAction.LegalMoves = Game.BattleManager.BattleGrid.GetRange (_unit.transform.position, _unit.Engines); 

	}

    public static void GetLegalTargets(string _Action)
    {

        var _TargetType = Game.Register.GetActionTargetType(_Action);
        var _source = ActiveUnit.transform.position; 

        if (LegalTargets == null)
            LegalTargets = new List<Vector3>();

        LegalTargets.Clear();

        if (_TargetType == TargetType.self)
        {
            LegalTargets.Add(ActiveUnit.transform.position);
            return;
        }


        int _range;

        if (_Action == "Attack")
        {
            var _selectedWeapon = ActiveUnit.selectedWeapon;
            _range = Game.Register.GetWeapon(_selectedWeapon).range;
        }
        else
        {
            _range = Game.Register.GetActionRange(_Action);
        }

        switch (_TargetType)
        {
            case TargetType.enemy:
                {
                   LegalTargets =  Game.BattleManager.BattleGrid.GetTargets(_source, _range, TargetType.enemy); 
                }
                break;
            case TargetType.ally:
                {
                    LegalTargets = Game.BattleManager.BattleGrid.GetTargets(_source, _range, TargetType.ally);
                }
                break;
            case TargetType.empty:
                {

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
			if (_cell.contents == CellContents.unit && _cell.unit != null)
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

		switch (_cell.contents)
		{
		case CellContents.empty:
			{
				if (BattleAction.LegalMoves.Contains (_point))
					return HightlightedContext.move;
				else
					return HightlightedContext.nothing;
			}
		case CellContents.unit:
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
			switch (_cell.contents)
			{
			case CellContents.empty:
				{				
					return HightlightedContext.nothing;
				}
			case CellContents.unit:
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

		switch (_cell.contents)
		{
		case CellContents.empty:
			{
				return HightlightedContext.nothing;
			}
		case CellContents.unit:
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

		switch (_cell.contents)
		{
		case CellContents.empty:
			{
				return HightlightedContext.nothing;
			}
		case CellContents.unit:
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
