using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{
	[SyncVar]
	public string battlePhase = "Waiting to Start";


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
//				Debug.Log (Game.PlayerName + " is connected on Network ID " + this.netId);
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


	[Command]
	public void CmdBattlePhase (string _phase)
	{
		this.battlePhase = _phase;
	}



	//	[Command]
	//	public void CmdUpdateState (string _param1, string _param2)
	//	{
	//		var _netID = this.netId;
	//		RpcBattleRpc (_netID, _param1, _param2);
	//	}
	//
	//	[ClientRpc]
	//	public void RpcUpdateState (int _netID, string _param1, string _param2)
	//	{
	//
	//
	//
	//	}
	//

	#region Battle Actions

	[Command]
	public void CmdBattleCommand (string _type, string _param1, string _param2)
	{
		RpcBattleRpc (_type, _param1, _param2);
	}

	[ClientRpc]
	public void RpcBattleRpc (string _type, string _param1, string _param2)
	{
		switch (_type)
		{
		case "DeployUnit":
			{
				var _unit = JsonUtility.FromJson<Unit> (_param1);
				var _position = JsonUtility.FromJson<Vector3> (_param2); 
				BattleAction.Execute ("DeployUnit", _unit, _position);
			}
			break;
		}

	}



	[Command]
	public void CmdBattleAction (string _type, Vector3 _target, string _param)
	{
		RpcBattleAction (_type, _target, _param);
	}

	[ClientRpc]
	public void RpcBattleAction (string _type, Vector3 _target, string _param)
	{
		switch (_type)
		{

		case "MoveUnit":
			{
				BattleAction.Execute ("MoveUnit", _target, _param);
			}
			break;
		case "BasicAttack":
			{
				BattleAction.Execute ("BasicAttack", _target, _param);
			}
			break;


		}

	}

	#endregion

}
