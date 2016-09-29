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

    public void QuickBattle()
    {
        Game.NetworkManager.onlineScene = "Battle";
        Game.PlayerName = "Player";
        var _player = new Player("Player", 0u, true);
        Game.Manager.Players.Add(_player);
        var _enemy = new Player("Enemy", 0u, true);
        Game.Manager.Players.Add(_enemy);

        foreach(var player in Game.Manager.Players)
        {
            var _units = BattleSetup.CreateUnits(Game.Manager.BasicFleet, player.Name);
            var _fleet = new FleetState(player.Name, "Basic Fleet", _units);
            player.fleet = _fleet;
        }
        Game.NetworkManager.StartupHost();

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