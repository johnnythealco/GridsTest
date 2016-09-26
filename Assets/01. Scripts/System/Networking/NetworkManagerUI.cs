using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class NetworkManagerUI :  MonoBehaviour
{

	public Button HostButton;
	public Button JoinButton;
	public InputField serverInput;
	public InputField nameInput;

	NetManager NetMgr;

	void Awake ()
	{
		NetMgr = GameObject.Find ("! NetworkManager !").GetComponent<NetManager> ();
	}

	void Start ()
	{
		if (nameInput.text == "" || nameInput.text == null)
		{
			HostButton.interactable = false;
			JoinButton.interactable = false;
		}
	}

	public void EnableButtons ()
	{
		if (nameInput.text != "" && nameInput.text != null)
		{
			HostButton.interactable = true;
			JoinButton.interactable = true;
		}
	}


	public void StartHost ()
	{
		NetMgr.StartupHost ();
	}

	public void SetPlayerName ()
	{
		Game.PlayerName = nameInput.text;

		Debug.Log ("Local Player's name is " + Game.PlayerName);
	}

	public void JoinGame ()
	{
		NetMgr.JoinGame ();
	}

	public void Disconnect ()
	{
		
	}

	//	public void LoadScene2 ()
	//	{
	//		NetMgr.LoadScene ("Main2");
	//	}



}