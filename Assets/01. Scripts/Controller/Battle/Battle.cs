using UnityEngine;
using System.Collections.Generic;
using JK.Grids;
using System.Linq;


public class Battle : MonoBehaviour
{
    #region Variables

    #region public
    public FlatHex flatHexPrefab;
	public SpriteRenderer cellBorder;
	public UnitModelDisplay unitDisplay;
	public CameraCTRL cameraCTRL;
    public Turn turnManager;
    #endregion

    #region Properties

    public FlatHex BattleGrid{ get; set; }

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
       
        BattleGrid = Instantiate (flatHexPrefab) as FlatHex;
		BattleGrid.BuildGrid ();
		BattleGrid.onClickCell += BattleGrid_onClickCell;
		BattleGrid.onMouseOverCell += BattleGrid_onMouseOverCell;
		BattleGrid.onRightClickCell += BattleGrid_onRightClickCell;
		BattleGrid.onNoCellSelected += BattleGrid_onNoCellSelected;

		Battle.TurnManager.onUnitStartTrun += OnUnitStartTrun;
		Battle.TurnManager.onUnitEndTrun += OnUnitEndTrun;
        
		gridCursor = Instantiate (cellBorder) as SpriteRenderer;
	    selectedUnitCursor = Instantiate (cellBorder) as SpriteRenderer;
        selectedTargetCursor = Instantiate(cellBorder) as SpriteRenderer;
        selectedUnitCursor.gameObject.SetActive (false);
        selectedTargetCursor.gameObject.SetActive(false);
        gridCursor.gameObject.SetActive(false);

        Game.NetworkManager.onAllPlayersReady += onAllPlayersReady;

    }

    private void onAllPlayersReady()
    {
        Debug.Log("All Players Ready");
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
            BattleAction.ActiveTarget = BattleGrid.GetCell(_point).unit;
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
    	
	void OnUnitStartTrun ()
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

	void OnUnitEndTrun ()
	{
		unitDisplay.onChangeAction -= ActiveUnit_onChangeAction;
		Battle.TurnManager.CmdNextUnit ();
	}

	void ActiveUnit_onChangeAction ()
	{
		
	}

	public void StartBattle ()
	{
         Debug.Log("All Player Ready Starting Battle");
		//Battle.TurnManager.CmdSortList ();	
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
        BattleAction.MovePath = BattleGrid.getGridPath(start, end);

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
			var _cell = BattleGrid.GetCell (_position);

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
