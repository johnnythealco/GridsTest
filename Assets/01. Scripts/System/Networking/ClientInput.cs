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

	void Start ()
	{
		Setup ();
        //BattleAction.StartBattle ();
       
	}

	void Setup ()
	{
		if (hasAuthority)
		{
			NetMgr.LocalPlayer = this;
			this.gameObject.name = "Local Player";
            var _name = Game.PlayerName;
            var _netId = (int)this.netId.Value;
            CmdJoinBattle(_netId, _name);
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
    void CmdJoinBattle(int _id, string _name)
    {      
        RpcJoinBattle(_id, _name);
    }

    [ClientRpc]
    void RpcJoinBattle(int i, string _name)
    {

        if (this.netId.Value == (uint)i)
        {
            if(this.gameObject.name != "Local Player")
            {
                this.gameObject.name = "Player " + this.netId.Value.ToString();
            }

            this.PlayerName = _name;

            var _Scene = SceneManager.GetActiveScene().name;

            if (_Scene == "Setup")
            {
                var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();
                battleSetup.AddPlayer(this);
            }

        }




    }

    [Command]
    public void CmdChangeReadyStatus(int _id, bool _readyStatus)
    {
        RpcReadyStatusChanged(_id, _readyStatus);
    }

    [ClientRpc]
    void RpcReadyStatusChanged(int _id, bool _readyStatus)
    {
        if (this.netId.Value == (uint)_id)
        {
            this.ready = _readyStatus;
        }

        if (this.netId.Value != (uint)_id)
        {
            var _Scene = SceneManager.GetActiveScene().name;

            if (_Scene == "Setup")
            {
                var battleSetup = GameObject.Find("BattleSetup").GetComponent<BattleSetup>();

                battleSetup.playerList.Prime(Game.Manager.Players);
            }

        }
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
