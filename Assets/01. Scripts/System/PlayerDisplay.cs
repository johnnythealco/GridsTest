using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour {

    ClientInput player;

    public Text playerName;
    public Toggle readyToggle;

    public delegate void PlayerDisplayDelegate_bool(bool _ReadyStatus);
    public event PlayerDisplayDelegate_bool onReadyStatusChanged;
   


    public void Prime(ClientInput _Player)
    {
        player = _Player;

        if (playerName != null)
            playerName.text = player.PlayerName;

        if (readyToggle != null)
            readyToggle.isOn = player.ready;
    }

    public void ChangeReadyStatus()
    {

        if (onReadyStatusChanged != null)
        {
            onReadyStatusChanged.Invoke(readyToggle.isOn);
        }

    }

}
