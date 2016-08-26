using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{

	public bool isPlayer;


	NetManager NetMgr;

	#region Setup and Network Management

	void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
	}

	void Start ()
	{
		Setup ();
	}

	void Setup ()
	{
		if (hasAuthority)
		{
			NetMgr.LocalPlayer = this;
			this.gameObject.name = "Local Player";
			this.isPlayer = this.isLocalPlayer;
			Debug.Log (Game.PlayerName + " is connected on Network ID " + this.netId);
		}

		if (!hasAuthority)
		{
			this.enabled = false;
		}

	}

	/// <summary>
	/// Disconnect this Player
	/// </summary>
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

	void Update ()
	{

			

	}


	[Command]
	public void CmdTest (string _msg)
	{		
		
		RpcTest (_msg);
	}

	[ClientRpc]
	void RpcTest (string _msg)
	{
		string message = _msg + " connected on Network ID " + this.netId + " sent a command";
		Debug.Log (message);	
	
	}

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
		case "MoveUnit":
			{
				var _position = JsonUtility.FromJson<Vector3> (_param1);
				var _destination = JsonUtility.FromJson<Vector3> (_param2);
				BattleAction.Execute ("MoveUnit", _position, _destination);
			}
			break;
		case "BasicAttack":
			{
				var _attacker = JsonUtility.FromJson<Vector3> (_param1);
				var _target = JsonUtility.FromJson<Vector3> (_param2);
				BattleAction.Execute ("BasicAttack", _attacker, _target);
			}
			break;


		}
		
	}


}
