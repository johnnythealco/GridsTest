using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class NetworkController :  NetworkBehaviour
{
    [SyncVar]
    public bool BattleReady;

    [SyncVar]
    public bool DeploymentComplete;


    #region Setup and Network Management

    void Awake ()
	{
        DontDestroyOnLoad (this.gameObject);
        Game.NetworkManager.NetworkPlayers.Add(this);
     }

    void Start()
    {
        Setup();
   }


    public void Setup ()
	{
        if (hasAuthority)
		{
            Game.NetworkController = this;

            var _Scene = SceneManager.GetActiveScene().name;

            if (_Scene == "Lobby")
            {
                var _player = new Player(Game.PlayerName, netId.Value, true);
               AddPlayer(_player);
            }             


        }

		if (!hasAuthority)
		{
			this.enabled = false;
		}



    }

	public void Disconnect ()
	{

		if (isServer)
		{
			NetworkManager.singleton.StopHost ();
			NetworkServer.Reset ();
		}

		if (isClient)
		{
			NetworkManager.singleton.StopClient ();
		}
	}

    [Command]
    public void Cmd_SetBattleReady(uint _id, bool _readyStatus)
    {
        if (this.netId.Value == _id)
            this.BattleReady = _readyStatus;
    }

  
    [Command]
    public void Cmd_SetDeploymentComplete(uint _id, bool _value)
    {
        if (this.netId.Value == _id)
            this.DeploymentComplete = _value;
    }

    #endregion

    #region battle Setup Actions

    #region Player
    public void AddPlayer(Player _Player)
    {
        var JSON = JsonUtility.ToJson(_Player);
        Cmd_AddPlayer(JSON);
    }

    [Command]
    public void Cmd_AddPlayer(string _Player_JSON)
    {
        var _Player = JsonUtility.FromJson<Player>(_Player_JSON);
        Game.Manager.Players.Add(_Player);

        foreach (var player in Game.Manager.Players)
        {
            var JSON = JsonUtility.ToJson(player);
            Rpc_AddPlayer_UpdatePlayerList(JSON);
        }
    }


    [ClientRpc]
    public void Rpc_AddPlayer_UpdatePlayerList(string _Player_JSON)
    {

        var _Player = JsonUtility.FromJson<Player>(_Player_JSON);

        if (!Game.Manager.Players.Contains(_Player))
        {
            Game.Manager.Players.Add(_Player);
        }

        var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
        battleSetup.playerList.Prime(Game.Manager.Players);
        battleSetup.UpdateFleetList();

    }

    public void RemovePlayer(Player _Player)
    {
        var JSON = JsonUtility.ToJson(_Player);
        Cmd_RemovePlayer(JSON);
    }

    [Command]
    public void Cmd_RemovePlayer(string _Player_JSON)
    {
        Rpc_RemovePlayer_UpdatePlayerList(_Player_JSON);
    }

    [ClientRpc]
    public void Rpc_RemovePlayer_UpdatePlayerList(string _Player_JSON)
    {

        var _Player = JsonUtility.FromJson<Player>(_Player_JSON);

        if (Game.Manager.Players.Contains(_Player))
        {
            Game.Manager.Players.Remove(_Player);
        }

        var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
        battleSetup.playerList.Prime(Game.Manager.Players);
        battleSetup.UpdateFleetList();

    }

    #endregion

    #region Ready Status


    [Command]
    public void Cmd_PlayerChangeReadyStatus(uint _id, bool _readyStatus)
    {
        Rpc_PlayerChangeReadyStatus(_id, _readyStatus);        
    }

    [ClientRpc]
    void Rpc_PlayerChangeReadyStatus(uint _id,bool _readyStatus)
    {
        foreach (var Player in Game.Manager.Players)
        {
            if (Player.human && Player.ConnectionID == _id)
            {
                Player.ReadyStatus = _readyStatus;
            }
        }
                   

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
            battleSetup.OnPlayerChangedReadyStatus();
        }

        
    }

    [Command]
    public void Cmd_SetAllPlayersNotReady()
    {
        foreach (var _player in Game.Manager.Players)
        {
            if (_player.human)
            {
                _player.ReadyStatus = false;
            }
        }

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
            battleSetup.playerList.UpdateReadyStatus(Game.Manager.Players);
        }
        Rpc_UpdateAllPlayersReadyDisplay();
    }

    [ClientRpc]
    void Rpc_UpdateAllPlayersReadyDisplay()
    {    
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
            battleSetup.playerList.UpdateReadyStatus(Game.Manager.Players);
        }
    }

    void AllPlayersNotReady()
    {
        if (!isServer)
            return;

        foreach (var _player in Game.Manager.Players)
        {
            if (_player.human)
            {
                _player.ReadyStatus = false;
            }
        }
    }

    
    void PlayerReady()
    {
        var neitID = netId.Value;
        Cmd_PlayerChangeReadyStatus(neitID, true);

    }


    #endregion

    #region Fleet

    public void UpdateFleet(FleetState _fleet, Player _player)
    {
        var _fleet_JSON = JsonUtility.ToJson(_fleet);
        var _player_JSON = JsonUtility.ToJson(_player);
        Cmd_AddFleet(_fleet_JSON, _player_JSON);
    }

    [Command]
    public void Cmd_AddFleet(string _fleet_JSON, string _player_JSON)
    {      
        Rpc_UpdateFleet(_fleet_JSON, _player_JSON);
    }


    [ClientRpc]
    public void Rpc_UpdateFleet(string _fleet_JSON, string _player_JSON)
    {

        var _fleet = JsonUtility.FromJson<FleetState>(_fleet_JSON);
        var _player = JsonUtility.FromJson<Player>(_player_JSON);

        if (Game.Manager.Players.Contains(_player))
        {
            var i = Game.Manager.Players.IndexOf(_player);
            Game.Manager.Players[i].fleet = _fleet;
        }

        var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
        battleSetup.UpdateFleetList();

    }


    #endregion

    #endregion

    #region Battle Turn Management

    [Command]
    public void Cmd_StartTurn()
    {
        Rpc_StartTurn();
    }

    [ClientRpc]
    public void Rpc_StartTurn()
    {
        var TurnOrder = Battle.TurnManager.TurnOrder;

        BattleAction.ActiveUnit = TurnOrder[0];

        if (BattleAction.ActiveUnit.state.Owner != Game.PlayerName)
        {
            GetNextUnitforLocalPlayer();
            Battle.LocalPlayerTurn = false;
        }
        else
        {
            BattleAction.NextUnit = null;
            Battle.LocalPlayerTurn = true;
        }

        StartUnitTurn();
    }

    public void StartUnitTurn()
    {
        Game.BattleManager.OnUnitStartTrun();
    }

    [Command]
    public void CmdNextUnit()
    {
        Game.BattleManager.OnUnitEndTrun();
    }

    [ClientRpc]
    public void RpcNextUnit()
    {
        var TurnOrder = Battle.TurnManager.TurnOrder;

        var i = TurnOrder.IndexOf(BattleAction.ActiveUnit);

        if (i < TurnOrder.Count() - 1)
        {
            BattleAction.ActiveUnit = TurnOrder[i + 1];

            if (BattleAction.ActiveUnit.state.Owner != Game.PlayerName)
            {
                GetNextUnitforLocalPlayer();
                Battle.LocalPlayerTurn = false;
            }
            else
            {
                BattleAction.NextUnit = null;
                Battle.LocalPlayerTurn = true;
            }

            StartUnitTurn();
        }
        else
        {
            Cmd_StartTurn();
        }


    }


    void GetNextUnitforLocalPlayer()
    {
        var TurnOrder = Battle.TurnManager.TurnOrder;
        var indexofActiveUnit = TurnOrder.IndexOf(BattleAction.ActiveUnit);

        int unitsRemaining = TurnOrder.Count() + 1 - indexofActiveUnit;

        for (int i = 1; i < unitsRemaining; i++)
        {
            int index = indexofActiveUnit + i;
            var _unit = TurnOrder[index];

            if (_unit.state.Owner == Game.PlayerName)
            {
                BattleAction.NextUnit = _unit;
                return;
            }
        }

        for (int i = 0; i < indexofActiveUnit; i++)
        {
            int index = i;
            var _unit = TurnOrder[index];

            if (_unit.state.Owner == Game.PlayerName)
            {
                BattleAction.NextUnit = _unit;
                return;
            }
        }

    }


    #endregion


    #region Battle Actions

    #region Deploy 

    [ClientRpc]
    public void Rpc_SetBattleState(string _battleStateJSON)
    {
        if (Game.isServer)
            return;

        var _battleState = JsonUtility.FromJson<BattleState>(_battleStateJSON);
        Debug.Log("Loading Battle State");
        Game.BattleManager.battleGrid.LoadState(_battleState);
        Game.BattleManager.RecieveBattleState();



        int i = 1;
        Debug.Log("--- Turn Order ----");

        foreach(var _unit in Battle.TurnManager.TurnOrder)
        {
            Debug.Log(_unit.DsiplayName + " " + "Position " + i.ToString());
            i++;
        }

    }


    [Command]
	public void CmdDeploy (string _Player)
	{

		RpcDeploy (_Player);
    }

	[ClientRpc]
	public void RpcDeploy (string _Player)
	{	
		var _player = JsonUtility.FromJson<Player> (_Player);
        BattleAction.DeployPlayerFleet(_player);
	}
    #endregion


    [Command]
	public void CmdBattleAction (string _type, Vector3 _target, string _param)
	{
		RpcBattleAction (_type, _target, _param);
	}

	[ClientRpc]
	public void RpcBattleAction (string _type, Vector3 _target, string _param)
	{
        BattleAction.Execute(_type, _target, _param);        

	}

	#endregion

}
