using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientInput :  NetworkBehaviour
{
	[SyncVar]
	public string battlePhase = "Waiting to Start";


	NetManager NetMgr;

	#region Setup and Network Management

	void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
		Debug.Log ("ClientInput is Awake"); 

		this.netId
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


	[Command]
	public void CmdBattlePhase (string _phase)
	{
		this.battlePhase = _phase;
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
		}
		
	}


	#region Battle Actions

	[Command]
	public void CmdBattleAction (string _type, Vector3 _point1, Vector3 _point2)
	{
		RpcBattleAction (_type, _point1, _point2);
	}

	[ClientRpc]
	public void RpcBattleAction (string _type, Vector3 _point1, Vector3 _point2)
	{
		switch (_type)
		{

		case "MoveUnit":
			{
				BattleAction.Execute ("MoveUnit", _point1, _point2);
			}
			break;
		case "BasicAttack":
			{
				BattleAction.Execute ("BasicAttack", _point1, _point2);
			}
			break;


		}

	}

	#endregion

}
