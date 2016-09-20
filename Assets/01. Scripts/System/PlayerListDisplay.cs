using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerListDisplay : MonoBehaviour {

    List<ClientInput> Players;
    List<PlayerDisplay> Displays;

    public PlayerDisplay playerDisplay;
    public Transform listTransform;

    

    public void Prime(List<ClientInput> _Players)
    {
        foreach(var _player in _Players)
        {
            var _display = (PlayerDisplay)Instantiate(playerDisplay);
            _display.transform.SetParent(listTransform);
            _display.Prime(_player);
            _display.onReadyStatusChanged += onReadyStatusChanged;
            Displays.Add(_display);

        }
    }

    private void onReadyStatusChanged(bool _ReadyStatus)
    {
       
    }
}
