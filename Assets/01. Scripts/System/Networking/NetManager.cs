using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;


public class NetManager : NetworkManager
{

	public ClientInput LocalPlayer;

	public void StartupHost ()
	{
		SetPort ();
		SetIPAddress ();
		NetworkManager.singleton.StartHost ();
        Game.isServer = true;
	}

	public void JoinGame ()
	{
		SetPort ();
		SetIPAddress ();
		NetworkManager.singleton.StartClient ();
        Game.isServer = false;
    }

	void SetPort ()
	{
		NetworkManager.singleton.networkPort = 7777;
	}

	void SetIPAddress ()
	{
		string ipAddress = GameObject.Find ("InputFieldIPAddress").transform.FindChild ("Text").GetComponent<Text> ().text;
		NetworkManager.singleton.networkAddress = ipAddress;
	}

	public void LoadScene (string sceneName)
	{
		NetworkManager.singleton.ServerChangeScene (sceneName);
	}

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        Debug.Log("OnServer Read Called by " + conn.ToString());

        if (conn.playerControllers.Count() >= 1)
        {
            var playerController = conn.playerControllers[0];
            var _networkController = playerController.gameObject.GetComponent<ClientInput>();
            var _id = _networkController.netId.Value;
            Game.Manager.PlayerReady(_id);
        }         
         

    }




}