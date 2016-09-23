using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour {

    Player player;

    public Text playerName;
    public Toggle readyToggle;

    public void Prime(Player _Player)
    {
        player = _Player;

        if (playerName != null)
            playerName.text = player.Name;

        if (readyToggle != null)
        {
            if (readyToggle.isOn != player.ReadyStatus)
                readyToggle.isOn = player.ReadyStatus;
        }

        if (_Player.Name != Game.PlayerName)
            readyToggle.interactable = false;

    }

    public void ChangeReadyStatus()
    {
        
        
        var _netID = Game.NetworkController.netId.Value;

        if (player.ConnectionID != _netID)
            return;

        var _ReadyStatus = readyToggle.isOn;  

        Game.NetworkController.Cmd_PlayerChangeReadyStatus(_netID,  _ReadyStatus);
    } 

}
