﻿using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{

    #region Properties

    #region References
    [SerializeField]
    Register register;

    [SerializeField]
    NetManager networkManager;
    #endregion

    public static Game Manager = null;

	public static Register Register{ get; set; }

    public static NetManager NetworkManager { get; set; }

	public static string PlayerName{ get; set; }

    public static NetworkController NetworkController { get; set; }

	public static Battle BattleManager{ get; set; }

	public static List<Vector3> GridPoints{ get; set; }

    public List<Player> Players = new List<Player>();

    public static bool isServer { get; set; }

    public List<string> BasicFleet;

    #endregion

    void Awake ()
	{
		if (Manager == null)
		{
			Manager = this;
			Register = register;
            NetworkManager = networkManager;


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
