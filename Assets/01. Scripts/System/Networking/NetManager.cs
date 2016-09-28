using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;


public class NetManager : NetworkManager
{
    public List<NetworkController> NetworkPlayers = new List<NetworkController>();

    public bool AllPlayersReady { get
        {
            foreach (var client in NetworkPlayers)
            {
                if (!client.BattleReady)
                {
                    return false;
                }
            }

            return true;
                
                    } }

    public bool DeploymentComplete
    {
        get
        {
            foreach (var client in NetworkPlayers)
            {
                if (!client.BattleReady || !client.DeploymentComplete)
                {
                    return false;
                }
            }

            return true;

        }
    }

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

    



}