using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{

    #region Properties
    public Register register;

    public static Game Manager = null;

	public static Register Register{ get; set; }

	public static string PlayerName{ get; set; }

    public static ClientInput NetworkController { get; set; }

	public static Battle BattleManager{ get; set; }

	public static List<Vector3> GridPoints{ get; set; }

    public List<Player> Players = new List<Player>();

    public static bool isServer { get; set; }

    public List<string> BasicFleet;

    #endregion

    public delegate void GameDelegate_netID(uint _id);
    public event GameDelegate_netID onPlayerReady;
    
    void Awake ()
	{
		if (Manager == null)
		{
			Manager = this;
			Register = register;

		} else if (Manager != this)
			Destroy (gameObject);


		DontDestroyOnLoad (gameObject);

		Debug.Log ("Game Manager is Awake"); 
	}
    
    public static Player GetPlayer(string _playerName)
    {
        foreach(var _player in Game.Manager.Players)
        {
            if (_player.Name == _playerName)
                return _player;
        }
        return null;

    
    }

    public  void PlayerReady(uint _id)
    {
        foreach (var Player in Game.Manager.Players)
        {
            if (Player.ConnectionID == _id)
            {
                Player.ReadyStatus = true;
            }
        }
        
        if(AllPlayersReady())
        {
            Game.NetworkController.Cmd_SetAllPlayersReady();
        }    
    }

    public bool AllPlayersReady()
    {
        foreach(var _Player in Players)
        {
            if (_Player.ReadyStatus != true)
                return false;
        }
        return true;
    }

    #region Testing
    public static void AddBasicFleet(string _player)
    {
        
        var _units = BattleSetup.CreateUnits(Game.Manager.BasicFleet, _player);
        var _Player = Game.GetPlayer(_player);
        var _fleet = new FleetState(_player, "Basic Fleet", _units);

        Game.NetworkController.UpdateFleet(_fleet, _Player);
    }
    #endregion

}
