using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;

public class BattleAction : MonoBehaviour
{
    #region Properties

    public static Unit ActiveUnit { get; set; }

    public static List<Vector3> LegalMoves{ get; set; }

	public static List<Vector3> LegalTargets{ get; set; }

	public static TargetType CurrentTargetType{ get; set; }

    public static Unit ActiveTarget { get; set; }

    public static Vector3 MoveDestination { get; set; }

    public static List<Vector3> MovePath { get; set; }

    public static Unit NextUnit;

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
		case "Move":
			return MoveUnit (_target);
		case "Attack":
			return BasicAttack ( _target, _param);
		}
		return false;
	}
    
    public static bool DeployUnit(UnitState _unit, Vector3 _position)
    {
        if (Game.BattleManager == null)
            return false;

        var battleManager = Game.BattleManager;
        var BattleGrid = battleManager.BattleGrid;

        var register = Game.Manager.register;
        var unitType = register.GetUnitType(_unit.UnitType);

        var unitModel = (Unit)Instantiate(unitType);
        unitModel.transform.position = _position;
        unitModel.setUnitState(_unit);
        unitModel.selectedWeapon = unitModel.Weapons.First();
        unitModel.selectedAction = unitModel.Actions.First();

        //Register on BattelGrid
        BattleGrid.RegisterUnit(_position, unitModel, CellContents.unit);


        JKLog.Log(_unit.Owner + " Deployed a " + _unit.UnitType + " to " + _position.ToString());

        return true;
    }
    
    #endregion

    #region Action Network Senders

    public static void Action_Click (string _action)
	{
		var _ActiveUnit = BattleAction.ActiveUnit;

		switch (_action)
		{
		case "Move":
                {
                    var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
                    var _destination = MoveDestination;
                    LocalPlayer.CmdBattleAction("Move", _destination, "none");
                }
                break;
		case "Attack":
			{
                    if (ActiveTarget == null)
                        return;

                    var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
                    var _Target = ActiveTarget.transform.position;
                    var _Weapon = ActiveUnit.selectedWeapon;
                    LocalPlayer.CmdBattleAction("Attack", _Target, _Weapon);
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

    public static void RandomDeploy(List<UnitState> _Units)
    {
        System.Random rnd = new System.Random();
        foreach (var _unit in _Units)
        {
            var i = rnd.Next(Game.GridPoints.Count());
            var _point = Game.GridPoints[i];

            while (Game.BattleManager.BattleGrid.GetCellAccessiblity(_point) == false)
            {
                i = rnd.Next(Game.GridPoints.Count());
                _point = Game.GridPoints[i];
            }

            var _unitJSON = JsonUtility.ToJson(_unit);
            var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
            LocalPlayer.CmdDeploy(_unitJSON, _point);            
        }

    }

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
        GetLegalTargets("Attack");

		return true;


	}

	static bool BasicAttack ( Vector3 _targetPosition, string _weapon)
	{
		if (Game.BattleManager == null)
			return false;

		var BattleGrid = Game.BattleManager.BattleGrid;
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


	#region Range and Target Methods

	public static void GetLegalMoves (Unit _unit)
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
    
	static List<Vector3> getEnemyTargetsInRange (Unit _unit, Weapon _weapon)
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

    
}
