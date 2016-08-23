using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class NetworkManagerUI :  MonoBehaviour
{

	NetManager NetMgr;

	void Awake ()
	{
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
	}


	public void StartHost ()
	{
		NetMgr.StartupHost ();
	}

	public void JoinGame ()
	{
		NetMgr.JoinGame ();
	}

	public void Disconnect ()
	{
		NetMgr.LocalPlayer.Disconnect ();
	}

	//	public void LoadScene2 ()
	//	{
	//		NetMgr.LoadScene ("Main2");
	//	}



}