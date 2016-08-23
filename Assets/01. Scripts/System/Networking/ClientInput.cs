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
//		if (Input.GetKeyDown (KeyCode.Space))
			

	}


	//	[Command]
	//	void CmdAddPlayer (string JSON_Player)
	//	{
	//		var player = JsonUtility.FromJson<Player> (JSON_Player);
	//		Game.Manager.state.Players.Add (player);
	//		RpcAddPlayer (JSON_Player);
	//	}
	//
	//	[ClientRpc]
	//	void RpcAddPlayer (string JSON_Player)
	//	{
	//
	//		var player = JsonUtility.FromJson<Player> (JSON_Player);
	//		if (!this.isServer)
	//			Game.Manager.state.Players.Add (player);
	//
	//
	//	}
	//
	//	void CreatePlayer ()
	//	{
	//		var PlayerID = (short)this.netId.Value;
	//		var PlayerName = "Player " + PlayerID.ToString ();
	//		var newPlayerObject = new Player (PlayerName, PlayerID);
	//		var JSON_Player = JsonUtility.ToJson (newPlayerObject);
	//
	//		CmdAddPlayer (JSON_Player);
	//	}


}
