using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{	
	NetManager NetMgr;
	JKTesting jktesting;

	#region Setup and Network Management

	void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
	}

	void Start ()
	{
		Setup ();
		BattleAction.StartBattle ();

	}

	void Setup ()
	{
		if (hasAuthority)
		{
			NetMgr.LocalPlayer = this;
			this.gameObject.name = "Local Player";
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
				var _unit = JsonUtility.FromJson<Unit> (_param1);
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
