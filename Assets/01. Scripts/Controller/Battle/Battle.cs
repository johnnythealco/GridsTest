﻿using UnityEngine;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;
using System;
using System.Collections;

public class Battle : MonoBehaviour
{
    #region Variables

    #region public
    public Unit ActiveUnit;
    public BattleGrid flatHexPrefab;
	public SpriteRenderer cellBorder;
	public UnitModelDisplay unitDisplay;
	public Camera TopDownCam;
    public Camera FreeCam;

    #endregion

    #region Properties

    public BattleGrid battleGrid{ get; set; }

	public static List<Unit> AllUnits{ get; set; }
    public static List<Unit> AllEnemies { get; set;}
    public static bool LocalPlayerTurn { get; set; }
    public static Camera ActiveBattleCam { get; set; }

        #endregion

    #region private

    SpriteRenderer gridCursor;
	SpriteRenderer selectedUnitCursor;
    SpriteRenderer selectedTargetCursor;
    List<SpriteRenderer> PathSteps = new List<SpriteRenderer>();
    #endregion

    #endregion

    #region Start & Update

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Game.NetworkController.Cmd_EndTurn();

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if(TopDownCam.enabled)
            {
                TopDownCam.enabled = false;
                FreeCam.enabled = true;
                ActiveBattleCam = FreeCam;
                ActiveBattleCam.GetComponent<CameraController>().ResetOn(ActiveUnit.transform.position);
            }
            else
            {
                TopDownCam.enabled = true;
                FreeCam.enabled = false;
                ActiveBattleCam = TopDownCam;
                 
            }
        }
    }

   
	void Start ()
	{
        Game.BattleManager = this;

        AllEnemies = new List<Unit>();

        TopDownCam.enabled = true;
        ActiveBattleCam = TopDownCam;
        FreeCam.enabled = false;

        battleGrid = Instantiate (flatHexPrefab) as BattleGrid;
		battleGrid.Setup();
		battleGrid.onClickCell += BattleGrid_onClickCell;
		battleGrid.onMouseOverCell += BattleGrid_onMouseOverCell;
		battleGrid.onRightClickCell += BattleGrid_onRightClickCell;
		battleGrid.onNoCellSelected += BattleGrid_onNoCellSelected;		
        
		gridCursor = Instantiate (cellBorder) as SpriteRenderer;
	    selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
        selectedTargetCursor = Instantiate(cellBorder) as SpriteRenderer;
        selectedUnitCursor.gameObject.SetActive (false);
        selectedTargetCursor.gameObject.SetActive(false);
        gridCursor.gameObject.SetActive(false);

        if (Game.isServer)
        {
            Server_SetupBattleState();
        }
        else
        {
            Client_SetupBattleState();
        }
    }


    public void Server_SetupBattleState()
    {
        if (!Game.isServer)
            return;

        for (int i = 0; i < Game.Manager.Players.Count(); i++)
        {
            var _Player = Game.Manager.Players[i];
            var _DeploymentArea = BattleGrid.DeploymentAreas[i];
            if (Game.isServer)
            {
                BattleAction.RandomDeploy(_Player.fleet.Units, _DeploymentArea);
            }
        }

        foreach (var _player in Game.Manager.Players)
        {
            foreach (var _unitState in _player.fleet.Units)
            {
                var _Unit = CreateUnit(_unitState);
                _Unit.transform.position = _unitState.Position;
                battleGrid.RegisterUnit(_unitState.Position, _Unit, CellContents.unit);
            }
        }

       SortUnits_Speed(AllUnits, 0, AllUnits.Count() - 1);



       StartCoroutine(WaitforCleints_LoadBattleState());
    
    }
    
    public void Client_SetupBattleState()
    {
        if (Game.isServer)
            return;

        var _id = Game.NetworkController.netId.Value;

        Game.NetworkController.Cmd_SetBattleReady(_id, true);

    }

    IEnumerator WaitforCleints_LoadBattleState()
    {
        int i = 1;
        while (i<2)
        {
            yield return new WaitForSeconds(1);
            i++;
        }
        Game.NetworkController.BattleReady = true;
        Game.NetworkController.DeploymentComplete = true;

        while (!Game.NetworkManager.AllPlayersReady)
        {
            BattleLog.Log("Waiting for Network Players!");
            yield return new WaitForSeconds(1);

        }

        BattleLog.Log("All Players Ready!");
        if (Game.isMultiplayer)
        {
            SendBattleState();
        }
        else
        {
            StartTurn();
        }


    }

    IEnumerator WaitforCleints_StartBattle()
    {

        while (!Game.NetworkManager.DeploymentComplete)
        {
            BattleLog.Log("Waiting for Network Players!");
            yield return new WaitForSeconds(1);

        }
        BattleLog.Log("Deployment Complete");
        StartTurn();
    }


    void SendBattleState()
    {
        var _TurnOrder = AllUnits;
        var turnList = new TurnList();
        turnList.positions = Game.BattleManager.GetUnitPositions(_TurnOrder);

        var _BattleState = new BattleState(Game.BattleManager.battleGrid, turnList.positions);
        var JSON = JsonUtility.ToJson(_BattleState);
        Game.NetworkController.Rpc_SetBattleState(JSON);
        StartCoroutine(WaitforCleints_StartBattle());
        
    }

    public void RecieveBattleState()
    {
        var _id = Game.NetworkController.netId.Value;
        Game.NetworkController.Cmd_SetDeploymentComplete(_id, true);
    }


    #endregion

    #region Event handlers 

    #region Mouse Input
    void BattleGrid_onRightClickCell (Vector3 _point)
	{


	}

	void BattleGrid_onMouseOverCell (Vector3 _point)
	{
        if (BattleAction.LegalMoves == null)
            return;

        if (BattleAction.LegalMoves.Contains(_point))
        {
            highlightLegalMove(_point);
            return;
        }

        if (BattleAction.LegalTargets != null && BattleAction.LegalTargets.Contains(_point))
        {
            highlightTarget(_point);
            return;
        }

        highlightEmptyCell (_point);
	}

	void BattleGrid_onClickCell (Vector3 _point)
	{
        if (!LocalPlayerTurn)
            return;

        if (BattleAction.LegalMoves == null)
            return;

        if (BattleAction.LegalMoves.Contains(_point))
        {
            BattleAction.MoveDestination = _point;
            highlightMovePath();

            if (Game.BattleManager.ActiveUnit.selectedAction == "Attack")
                Game.BattleManager.ActiveUnit.selectedAction = ("Move");
                return;
        }


        if (BattleAction.LegalTargets.Contains(_point))
        {
            if(Game.BattleManager.ActiveUnit.selectedAction == "Move")            
                Game.BattleManager.ActiveUnit.selectedAction = ("Attack");
            
            unitDisplay.Prime(Game.BattleManager.ActiveUnit);
            BattleAction.ActiveTarget = battleGrid.GetCell(_point).unit;
            selectedTargetCursor.transform.position = _point;
            selectedTargetCursor.gameObject.SetActive(true);
            selectedTargetCursor.color = Color.red;
        }

        if (!BattleAction.LegalMoves.Contains(_point))
        {
            BattleAction.MoveDestination = Game.BattleManager.ActiveUnit.transform.position;
            if (PathSteps.Count() > 0)
                ClearPathSteps();         
        }

        if(BattleAction.ActiveTarget != null && _point != BattleAction.ActiveTarget.transform.position)
        {
            ClearTarget();
        }



       




    }

	void BattleGrid_onNoCellSelected ()
	{
		gridCursor.gameObject.SetActive (false);

	}
    #endregion

    void StartTurn()
    {       
        ActiveUnit = AllUnits[0];
        var _unitPosition = ActiveUnit.transform.position;
        Game.NetworkController.Rpc_StartUnitTurn(_unitPosition);
    }

    public void OnUnitStartTrun (Vector3 _unitPosition)
	{
        ClearActiveUnit();
        ClearPathSteps();        
        FaceClosestEnemy_AllUnits();
        var _unit = GetUnit(_unitPosition);
        _unit.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
        ActiveUnit = _unit;     
        LocalPlayerTurn = _unit.state.Owner == Game.PlayerName;

        if(_unit.state.AP < _unit.state.Max_AP)
        {
            _unit.state.AP = _unit.state.AP + _unit.AP;
            if(_unit.state.AP > _unit.state.Max_AP)
            {
                _unit.state.AP = _unit.state.Max_AP;
            }
        }


        if (LocalPlayerTurn)
        {

            var _point = Game.BattleManager.ActiveUnit.transform.position;

            BattleAction.GetLegalMoves(ActiveUnit);
            BattleAction.GetLegalTargets("Attack");
            unitDisplay.Prime(ActiveUnit);
            unitDisplay.onChangeAction += ActiveUnit_onChangeAction;
            highlightActiveUnit(_point);
            TopDownCam.gameObject.GetComponent<CameraController>().CentreOn(_point);
            FreeCam.GetComponent<CameraController>().CentreOn(_point);   
        }
        else
        {
            var _nextUnit = GetNextUnitforLocalPlayer();
            unitDisplay.Prime(_nextUnit);
            TopDownCam.GetComponent<CameraController>().CentreOn(_nextUnit.transform.position);
            FreeCam.GetComponent<CameraController>().CentreOn(_nextUnit.transform.position);
        }       
    }

	public void OnUnitEndTrun ()
	{
        ActiveUnit.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
        var i = AllUnits.IndexOf(ActiveUnit);

        if (i < AllUnits.Count()-1)
        {
            ActiveUnit = AllUnits[i + 1];
            var _unitPosition = ActiveUnit.transform.position;
            Game.NetworkController.Rpc_StartUnitTurn(_unitPosition);
        }
        else
        {
            StartTurn();
        }
    }

    void ActiveUnit_onChangeAction ()
	{
		
	}
    
    public void Action_Click()
    {
        if (!LocalPlayerTurn)
            return;

        var _selectedAction = Game.BattleManager.ActiveUnit.selectedAction;

        if (_selectedAction != null || _selectedAction != "")
        {
            BattleAction.Action_Click(_selectedAction);
        }


    }

    #endregion
    
    #region Highlighting

    void highlightActiveUnit(Vector3 _point)
    {
        selectedUnitCursor.gameObject.SetActive(true);
        selectedUnitCursor.color = Color.blue;
        selectedUnitCursor.transform.position = _point;
        selectedUnitCursor.transform.SetParent(GetUnit(_point).transform);
    }

    void highlightEmptyCell (Vector3 _point)
	{
		gridCursor.color = Color.gray;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	

	}

	void highlightDeployment (Vector3 _point)
	{
		gridCursor.color = Color.yellow;
		gridCursor.transform.position = _point;
		gridCursor.gameObject.SetActive (true);	

	}

	void highlightTarget (Vector3 _point)
	{
        gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
        gridCursor.color = Color.yellow;
	}

	void highlightEnemy (Vector3 _point)
	{
	    gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
		gridCursor.color = Color.magenta;
	}

	void highlightLegalMove (Vector3 _point)
	{	
		gridCursor.transform.position = _point;
        gridCursor.gameObject.SetActive(true);
		gridCursor.color = Color.green;
	}

    void highlightMovePath()
    {
        ClearPathSteps();

        if (Game.BattleManager.ActiveUnit == null)
            return;


        var start = Game.BattleManager.ActiveUnit.transform.position;
        var end = BattleAction.MoveDestination;

  
            if (Game.BattleManager.ActiveUnit.Size == unitSize.large)
                battleGrid.SetLargeUnitAccessiblty(start, true);       



        BattleAction.MovePath = battleGrid.getGridPath(start, end);

        foreach(var step in BattleAction.MovePath)
        {
            var stepCursor = Instantiate(cellBorder) as SpriteRenderer;
            stepCursor.color = Color.green;
            stepCursor.transform.position = step;
            PathSteps.Add(stepCursor); 
        }

        if (Game.BattleManager.ActiveUnit.Size == unitSize.large)
            battleGrid.SetLargeUnitAccessiblty(start, false);


    }

    void ClearActiveUnit()
    {
        selectedUnitCursor.gameObject.SetActive(false);
    }

    public void ClearPathSteps()
    {
        if (PathSteps.Count() <= 0)
            return;

        foreach(var step in PathSteps)
        {
            if(step != null)
                Destroy(step.gameObject); 
        }


    }

    public void ClearTarget()
    {
        selectedTargetCursor.gameObject.SetActive(false);
        BattleAction.ActiveTarget = null;
    }




    #endregion


    #region Utility Functions

    
    public Unit CreateUnit(UnitState _state)
    {

        var _Template = Game.Register.GetUnitType(_state.UnitType);

        var _unit = (Unit)Instantiate(_Template);
        _unit.setUnitState(_state);
        _unit.selectedWeapon = _unit.Weapons.First();
        _unit.selectedAction = _unit.Actions.First();

        var unitBorder = Instantiate(cellBorder) as SpriteRenderer;
        unitBorder.gameObject.SetActive(true);
        unitBorder.transform.position = _unit.transform.position;
        unitBorder.transform.SetParent(_unit.transform);

        if (_unit.state.Owner == Game.PlayerName)
            unitBorder.color = Color.grey;
        else
            unitBorder.color = Color.red;


        return _unit;
    }

    public List<Unit> GetUnitsFromPositions (List<Vector3> _positions)
	{
		var result = new List<Unit> ();

		foreach (var _position in _positions)
		{
			var _cell = battleGrid.GetCell (_position);

			if (_cell.unit != null)
			{
				result.Add (_cell.unit);
			}
		}

		return result;

	}

	public List<Vector3> GetUnitPositions (List<Unit> _Units)
	{
		var result = new List<Vector3> ();

		foreach (var _Unit in _Units)
		{
			result.Add (_Unit.transform.position);
		}

		return result;

	}

    int Partition_Speed(List<Unit> list, int left, int right)
    {
        Unit pivot = list[left];

        while (true)
        {
            while (list[left].Speed > pivot.Speed)
                left++;

            while (list[right].Speed < pivot.Speed)
                right--;

            if (list[right].Speed == pivot.Speed && list[left].Speed == pivot.Speed)
                left++;

            if (left < right)
            {
                Unit temp = list[left];
                list[left] = list[right];
                list[right] = temp;
            }
            else
            {
                return right;
            }
        }
    }

    void SortUnits_Speed(List<Unit> list, int left, int right)
    {
        if (left < right)
        {
            int pivotIdx = Partition_Speed(list, left, right);

            if (pivotIdx > 1)
                SortUnits_Speed(list, left, pivotIdx - 1);

            if (pivotIdx + 1 < right)
                SortUnits_Speed(list, pivotIdx + 1, right);
        }

    }

    Unit GetNextUnitforLocalPlayer()
    {
        foreach (var _unit in AllUnits)
        {
            if (_unit.state.Owner == Game.PlayerName)
                return _unit;    
        }

        return null;

    }

    Unit GetUnit(Vector3 _position)
    {
        var _cell = battleGrid.GetCell(_position);

        if (_cell.contents != CellContents.unit)
            return null;

        return _cell.unit;
    }

    public Unit FindClosestEnemy(Unit _Unit)
    {
        int _closestDistance = 5000000;
        Unit _closestEnemy = null;

        foreach (var _target in AllUnits)
        {
            if (_target.state.Owner != _Unit.state.Owner)
            { 

            var _distance = battleGrid.GetDistance(_Unit.transform.position, _target.transform.position);

            if (_distance < _closestDistance)
                _closestEnemy = _target;
            }
        }

        return _closestEnemy; 
    
    }


    
    void FaceClosestEnemy_AllUnits()
    {
        foreach(var _unit in AllUnits)
        {
            var _enemy = FindClosestEnemy(_unit);
            if(_enemy != null)
                _unit.transform.forward = _enemy.transform.position;
        }
    }

    #endregion


}

[System.Serializable]
public class TurnList
{
    public List<Vector3> positions;

}
