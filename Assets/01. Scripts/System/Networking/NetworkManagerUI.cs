using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class NetworkManagerUI :  MonoBehaviour
{

	public Button HostButton;
	public Button JoinButton;
	public InputField serverInput;
	public InputField nameInput;

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
		Game.NetworkManager.StartupHost ();
	}

	public void SetPlayerName ()
	{
		Game.PlayerName = nameInput.text;
	}

	public void JoinGame ()
	{
        Game.NetworkManager.JoinGame ();
	}

	public void Disconnect ()
	{
		
	}

}