using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour {

    ClientInput player;

    public Text playerName;
    public Toggle readyToggle;

    public void Prime(ClientInput _Player)
    {
        player = _Player;

        if (playerName != null)
            playerName.text = player.PlayerName;

        if (readyToggle != null)
        {
            if(readyToggle.isOn != player.ready)
                readyToggle.isOn = player.ready;
        }
            
    }

    public void ChangeReadyStatus()
    {

        var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
        var _netID = (int)LocalPlayer.netId.Value;
        var _ReadyStatus = readyToggle.isOn;

        LocalPlayer.CmdChangeReadyStatus(_netID, _ReadyStatus);

        Debug.Log("Changed Ready Status Called");

    }

}
