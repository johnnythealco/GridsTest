﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class BattleAction : MonoBehaviour
{
    #region Properties

    public static List<Vector3> LegalMoves{ get; set; }

	public static List<Vector3> LegalTargets{ get; set; }

	public static TargetType CurrentTargetType{ get; set; }

    public static Unit ActiveTarget { get; set; }

    public static Vector3 MoveDestination { get; set; }

    public static List<Vector3> MovePath { get; set; }

    #endregion

    #region Action Execution Network Recievers

 	public static bool Execute (string _action, Vector3 _target, string _param)
	{
		switch (_action)
		{
		case "Move":
			return MoveUnit (_target);
		case "Attack":
			return BasicAttack ( _target, _param);
		}
		return false;
	}
           
    public static void DeployUnit(UnitState _unit, Vector3 _position)
    {  
        var battleManager = Game.BattleManager;
        var BattleGrid = battleManager.battleGrid;

        var register = Game.Register;
        var unitType = register.GetUnitType(_unit.UnitType);

        var unitModel = (Unit)Instantiate(unitType);
        unitModel.transform.position = _position;
        unitModel.setUnitState(_unit);
        unitModel.selectedWeapon = unitModel.Weapons.First();
        unitModel.selectedAction = unitModel.Actions.First();

        //Register on BattelGrid
        BattleGrid.RegisterUnit(_position, unitModel, CellContents.unit);

    }

    public static void DeployPlayerFleet(Player _player)
    {
    
        var BattleGrid = Game.BattleManager.battleGrid;

        foreach (var _unit in _player.fleet.Units)
        {
            var unitType = Game.Register.GetUnitType(_unit.UnitType);
            var _position = _unit.Position;

            var unitModel = (Unit)Instantiate(unitType);
            unitModel.transform.position = _position;
            unitModel.setUnitState(_unit);
            unitModel.selectedWeapon = unitModel.Weapons.First();
            unitModel.selectedAction = unitModel.Actions.First();

            BattleGrid.RegisterUnit(_position, unitModel, CellContents.unit);
            JKLog.Log(_unit.Owner + " Deployed a " + _unit.UnitType + " to " + _position.ToString());

        }
    }


    #endregion

    #region Action Network Senders

    public static void Action_Click (string _action)
	{
		switch (_action)
		{
		case "Move":
                {
                    var _destination = MoveDestination;
                    Game.NetworkController.CmdBattleAction("Move", _destination, "none");
                }
                break;
		case "Attack":
			{
                    if (ActiveTarget == null)
                        return;

                    var _Target = ActiveTarget.transform.position;
                    var _Weapon = Game.BattleManager.ActiveUnit.selectedWeapon;
                    Game.NetworkController.CmdBattleAction("Attack", _Target, _Weapon);
                }
			break;
		case "Evade":
			{
				JKLog.Log (Game.BattleManager.ActiveUnit.DsiplayName + " " + Game.BattleManager.ActiveUnit.selectedAction); 
			}
			break;
		case "End Turn":
			{ 
				JKLog.Log (Game.BattleManager.ActiveUnit.DsiplayName + " " + Game.BattleManager.ActiveUnit.selectedAction);
                Game.NetworkController.Cmd_EndTurn();
			}
			break;
		}
	}
    

    #endregion

    #region Action Ececution Methods

    public static void RandomDeploy(List<UnitState> _Units, List<Vector3> _DeploymentArea)
    {
        System.Random rnd = new System.Random();

        foreach (var _unit in _Units)
        {
            var i = rnd.Next(_DeploymentArea.Count());
            var _point = _DeploymentArea[i];

            while (Game.BattleManager.battleGrid.GetCellAccessiblity(_point) == false)
            {
                i = rnd.Next(_DeploymentArea.Count());
                _point = _DeploymentArea[i];
            }
            _unit.Position = _point;        
        }

    }

    static bool MoveUnit (Vector3 _end)
	{
		if (Game.BattleManager == null)
			return false;
		
		var battleManager = Game.BattleManager;
		var BattleGrid = battleManager.battleGrid;

	
        var _start = Game.BattleManager.ActiveUnit.transform.position;
		var path = BattleGrid.getGridPath (_start, _end);

		foreach (var step in path)
		{
			BattleGrid.UnRegisterObject (Game.BattleManager.ActiveUnit.transform.position);
			BattleGrid.RegisterUnit (step, Game.BattleManager.ActiveUnit, CellContents.unit);
            Game.BattleManager.ActiveUnit.transform.position = step;
			JKLog.Log (Game.BattleManager.ActiveUnit.faction + " " + Game.BattleManager.ActiveUnit.DsiplayName + " Move to " + step.ToString ());
		}

        battleManager.ClearPathSteps();
        GetLegalMoves(Game.BattleManager.ActiveUnit);
        GetLegalTargets("Attack");

		return true;


	}

	static bool BasicAttack ( Vector3 _targetPosition, string _weapon)
	{
		if (Game.BattleManager == null)
			return false;

		var BattleGrid = Game.BattleManager.battleGrid;
        var _target = BattleGrid.GetCell (_targetPosition).unit;

        var destroyed = _target.HitBy (_weapon);



		if (destroyed)
		{
			BattleGrid.UnRegisterObject (_target.transform.position);
			_target.DestroyUnit ();
			JKLog.Log (_target.faction + " " + _target.DsiplayName + " was destroyed! : ( ");

		}

        Game.BattleManager.ClearTarget();

		return true;
	}



    #endregion

    #region Action Information
    public static int GetAPCost(string _action)
    {
        switch (_action)
        {
            case "Attack":
                {
                    return Game.Register.GetWeaponAPcost(Game.BattleManager.ActiveUnit.selectedWeapon);
                }                
            default:
                {
                    return Game.Register.GetActionAPCost(_action);
                }              
        }
    }

    #endregion

    #region Range and Target Methods

    public static void GetLegalMoves (Unit _unit)
	{
		BattleAction.LegalMoves = Game.BattleManager.battleGrid.GetRange (_unit.transform.position, _unit.Engines); 

	}

    public static void GetLegalTargets(string _Action)
    {

        var _TargetType = Game.Register.GetActionTargetType(_Action);
        var _source = Game.BattleManager.ActiveUnit.transform.position; 

        if (LegalTargets == null)
            LegalTargets = new List<Vector3>();

        LegalTargets.Clear();

        if (_TargetType == TargetType.self)
        {
            LegalTargets.Add(Game.BattleManager.ActiveUnit.transform.position);
            return;
        }


        int _range;

        if (_Action == "Attack")
        {
            var _selectedWeapon = Game.BattleManager.ActiveUnit.selectedWeapon;
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
                   LegalTargets =  Game.BattleManager.battleGrid.GetTargets(_source, _range, TargetType.enemy); 
                }
                break;
            case TargetType.ally:
                {
                    LegalTargets = Game.BattleManager.battleGrid.GetTargets(_source, _range, TargetType.ally);
                }
                break;
            case TargetType.empty:
                {

                }
                break;
        }
    }
    
	static List<Vector3> getEnemyTargetsInRange (Unit _unit, Weapon _weapon)
	{
		List<Vector3> result = new List<Vector3> ();

		var _grid = Game.BattleManager.battleGrid;
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

    
}
