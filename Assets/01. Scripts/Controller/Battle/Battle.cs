using UnityEngine;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;
using System;
using System.Collections;

public class Battle : MonoBehaviour
{
    #region Variables

    #region public
    public BattleGrid flatHexPrefab;
	public SpriteRenderer cellBorder;
	public UnitModelDisplay unitDisplay;
	public CameraCTRL cameraCTRL;
    public Turn turnManager;
    #endregion

    #region Properties

    public BattleGrid battleGrid{ get; set; }

	public static Turn TurnManager{ get; set; }

	public static Unit SelectedUnit{ get; set; }

	public static List<Unit> AllUnits{ get; set; }

    public static bool LocalPlayerTurn { get; set; }
    #endregion

    #region private

    SpriteRenderer gridCursor;
	SpriteRenderer selectedUnitCursor;
    SpriteRenderer selectedTargetCursor;
    List<SpriteRenderer> PathSteps = new List<SpriteRenderer>();
    #endregion

    #endregion

    #region Start & Update

    void Awake()
    {
        TurnManager = turnManager;


    }

	void Start ()
	{
        Game.BattleManager = this;
       
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
                Debug.Log("Deploying Fleet for " + _Player.Name);
                BattleAction.RandomDeploy(_Player.fleet.Units, _DeploymentArea);
            }
        }

        foreach (var _player in Game.Manager.Players)
        {
            foreach (var _unitState in _player.fleet.Units)
            {
                var _Unit = Game.CreateUnit(_unitState);
                _Unit.transform.position = _unitState.Position;
                battleGrid.RegisterUnit(_unitState.Position, _Unit, CellContents.unit);
            }
        }

        var _TurnOrder = TurnManager.TurnOrder;
        _TurnOrder.AddRange(AllUnits);
        TurnManager.SortUnits_Speed(_TurnOrder, 0, _TurnOrder.Count() - 1);

       Game.NetworkController.BattleReady = true;
       Game.NetworkController.DeploymentComplete = true;
       StartCoroutine(  WaitforCleints_LoadBattleState());
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

        while (!Game.NetworkManager.AllPlayersReady)
        {
            JKLog.Log("Waiting for Network Players!");
            yield return new WaitForSeconds(1);

        }
        JKLog.Log("All Players Ready!");

        SendBattleState();


    }

    IEnumerator WaitforCleints_StartBattle()
    {

        while (!Game.NetworkManager.DeploymentComplete)
        {
            JKLog.Log("Waiting for Network Players!");
            yield return new WaitForSeconds(1);

        }
        JKLog.Log("Deployment Complete");

        Game.NetworkController.Rpc_StartTurn();


    }


    void SendBattleState()
    {
        var _TurnOrder = TurnManager.TurnOrder;
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

            if (BattleAction.ActiveUnit.selectedAction == "Attack")
                BattleAction.ActiveUnit.selectedAction = ("Move");
                return;
        }


        if (BattleAction.LegalTargets.Contains(_point))
        {
            if(BattleAction.ActiveUnit.selectedAction == "Move")            
                BattleAction.ActiveUnit.selectedAction = ("Attack");
            
            unitDisplay.Prime(BattleAction.ActiveUnit);
            BattleAction.ActiveTarget = battleGrid.GetCell(_point).unit;
            selectedTargetCursor.transform.position = _point;
            selectedTargetCursor.gameObject.SetActive(true);
            selectedTargetCursor.color = Color.red;
        }

        if (!BattleAction.LegalMoves.Contains(_point))
        {
            BattleAction.MoveDestination = BattleAction.ActiveUnit.transform.position;
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
    	
	public void OnUnitStartTrun ()
	{
        if (LocalPlayerTurn)
        {

            var _point = BattleAction.ActiveUnit.transform.position;

            BattleAction.GetLegalMoves(BattleAction.ActiveUnit);
            BattleAction.GetLegalTargets("Attack");

            unitDisplay.Prime(BattleAction.ActiveUnit);
            unitDisplay.onChangeAction += ActiveUnit_onChangeAction;
            selectedUnitCursor.gameObject.SetActive(true);
            selectedUnitCursor.color = Color.blue;
            selectedUnitCursor.transform.position = _point;
            selectedUnitCursor.transform.SetParent(BattleAction.ActiveUnit.transform);
            cameraCTRL.CentreOn(_point);
        }
        else
        {
            var _point = BattleAction.NextUnit.transform.position;
            unitDisplay.Prime(BattleAction.NextUnit);
            cameraCTRL.CentreOn(_point);
        }

       
    }

	public void OnUnitEndTrun ()
	{
        Game.NetworkController.RpcNextUnit();
	}

	void ActiveUnit_onChangeAction ()
	{
		
	}
    
    public void Action_Click()
    {
        if (!LocalPlayerTurn)
            return;

        var _selectedAction = BattleAction.ActiveUnit.selectedAction;

        if (_selectedAction != null || _selectedAction != "")
        {
            BattleAction.Action_Click(_selectedAction);
        }


    }

    #endregion



    #region Highlighting

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
        gridCursor.color = Color.red;
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

        if (BattleAction.ActiveUnit == null)
            return;

        var start = BattleAction.ActiveUnit.transform.position;
        var end = BattleAction.MoveDestination;
        BattleAction.MovePath = battleGrid.getGridPath(start, end);

        foreach(var step in BattleAction.MovePath)
        {
            var stepCursor = Instantiate(cellBorder) as SpriteRenderer;
            stepCursor.color = Color.green;
            stepCursor.transform.position = step;
            PathSteps.Add(stepCursor); 
        }
        

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


	#endregion


}

#region Enums

public enum BattleContext
{
	nothing_selected = 0,
	unit_default = 1,
	Deployment = 2,
	unit_Action = 3
}

public enum HightlightedContext
{
	nothing = 0,
	unit = 1,
	enemy = 2,
	target = 3,
	move = 4
}
#endregion
