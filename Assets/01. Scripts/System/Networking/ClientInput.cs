using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{	
	NetManager NetMgr;
    

    [SyncVar]
    public string PlayerName;

    public bool ready;

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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            {
            var _connections = NetworkServer.connections;

            foreach (var _connection in _connections)
            {
                var _Player = _connection.playerControllers[0];
                Debug.Log(_Player.gameObject.name);
            }

        }


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

                var _player = new Player(Game.PlayerName, netId.Value);
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
            Rpc_UpdatePlayerList(JSON);
        }
    }

    [ClientRpc]
    public void Rpc_UpdatePlayerList(string _Player_JSON)
    {

        var _Player = JsonUtility.FromJson<Player>(_Player_JSON);

        if (!Game.Manager.Players.Contains(_Player))
        {
            Game.Manager.Players.Add(_Player);
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
