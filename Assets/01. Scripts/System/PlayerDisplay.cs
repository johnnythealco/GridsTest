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

    }

    public void ChangeReadyStatus()
    {
        
        var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
        var _netID = LocalPlayer.netId.Value;

        if (player.ConnectionID != _netID)
            return;

        var _ReadyStatus = readyToggle.isOn;
        var _PlayerName = Game.PlayerName;

        LocalPlayer.Cmd_PlayerChangeReadyStatus(_netID, _PlayerName, _ReadyStatus);
        Debug.Log("Changed Ready Status Called");
    }

    public void Click()
    {
        Debug.Log("Clicked");
        readyToggle.isOn = !readyToggle.isOn;


    }

}
