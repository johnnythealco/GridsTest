using UnityEngine;
using System.Collections;

public class JKQuickStart : MonoBehaviour
{
	public NetworkManagerUI networkManagerUI;

	// Use this for initialization
	void Start ()
	{
		networkManagerUI.nameInput.text = "JK";
		networkManagerUI.SetPlayerName ();
		networkManagerUI.EnableButtons ();
		networkManagerUI.StartHost ();
		
	
	}
	

}
