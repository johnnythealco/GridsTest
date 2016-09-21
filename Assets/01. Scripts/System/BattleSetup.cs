using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BattleSetup : MonoBehaviour {

    public PlayerListDisplay playerList;

    void Start ()
    {
        if (Game.Manager.Players == null)
            Game.Manager.Players = new List<Player>();

    }

    public void OnPlayerChangedReadyStatus(uint _id, string _playerName, bool _readyStatus)
    {
        foreach(var Player in Game.Manager.Players)
        {
            if(Player.ConnectionID == _id && Player.Name == _playerName)
            {
                Player.ReadyStatus = _readyStatus;
            }
        }


        playerList.UpdateReadyStatus(Game.Manager.Players);
    }
}
