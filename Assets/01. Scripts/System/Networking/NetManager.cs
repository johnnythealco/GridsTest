using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;


public class NetManager : NetworkManager
{

	public ClientInput LocalPlayer;

	public void StartupHost ()
	{
		SetPort ();
		SetIPAddress ();
		NetworkManager.singleton.StartHost ();
	}

	public void JoinGame ()
	{
		SetPort ();
		SetIPAddress ();
		NetworkManager.singleton.StartClient ();	
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