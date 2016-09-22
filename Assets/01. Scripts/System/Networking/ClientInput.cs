using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{	
	NetManager NetMgr;
   

	#region Setup and Network Management

	void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
  
	}

    void Start()
    {
        Setup();
        //BattleAction.StartBattle ();

           
        }


    public void Setup ()
	{
		if (hasAuthority)
		{
			NetMgr.LocalPlayer = this;
			this.gameObject.name = "Local Player";

            var _Scene = SceneManager.GetActiveScene().name;

            if (_Scene == "Setup")
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

    }


    [Command]
    public void Cmd_PlayerChangeReadyStatus(uint _id, string _playerName, bool _readyStatus)
    {
        Rpc_PlayerChangeReadyStatus(_id, _playerName, _readyStatus);        
    }

    [ClientRpc]
    void Rpc_PlayerChangeReadyStatus(uint _id, string _playerName, bool _readyStatus)
    {
        var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
        battleSetup.OnPlayerChangedReadyStatus(_id, _playerName, _readyStatus);
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
        var _fleet = JsonUtility.FromJson<FleetState>(_fleet_JSON);
        var _player = JsonUtility.FromJson<Player>(_player_JSON);

        Rpc_UpdateFleet(_fleet_JSON, _player_JSON);
    }


    [ClientRpc]
    public void Rpc_UpdateFleet(string _fleet_JSON, string _player_JSON)
    {

        var _fleet = JsonUtility.FromJson<FleetState>(_fleet_JSON);
        var _player = JsonUtility.FromJson<Player>(_player_JSON);

        if (Game.Manager.Players.Contains(_player))
        {
            //Game.Manager.Players.
        }

        var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
        battleSetup.playerList.Prime(Game.Manager.Players);

    }


    #endregion

    #endregion



    #region Battle Actions

    #region Deploy
    [Command]
	public void CmdDeploy (string _Unit, Vector3 _position)
	{
		RpcDeploy (_Unit, _position);
	}

	[ClientRpc]
	public void RpcDeploy (string _param1, Vector3 _position)
	{	
				var _unit = JsonUtility.FromJson<UnitState> (_param1);
				BattleAction.DeployUnit ( _unit, _position);
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
