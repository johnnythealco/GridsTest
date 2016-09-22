using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerListDisplay : MonoBehaviour {


    List<PlayerDisplay> Displays = new List<PlayerDisplay>();

    public PlayerDisplay playerDisplay;
   
    public Transform listTransform;

    

    public void Prime(List<Player> _Players)
    {
        clearDisplays();  

        foreach (var _player in _Players)
        {
            PlayerDisplay _display = (PlayerDisplay)Instantiate(playerDisplay, listTransform, false);
            _display.transform.SetParent(listTransform);
            _display.Prime(_player);
            Displays.Add(_display);
        }
    }

    public void UpdateReadyStatus(List<Player> _Players)
    {
        foreach (var _player in _Players)
        {
            foreach (var _display in Displays)
            {

                if (_display.playerName != null && _display.playerName.text == _player.Name)
                {
                    if (_display.readyToggle != null)
                    {
                            _display.readyToggle.isOn = _player.ReadyStatus;
                    }
                }
            }

        }
    }



    void clearDisplays()
    {
        for (int i = 0; i < listTransform.childCount; i++)
        {            
                Destroy(listTransform.GetChild(i).gameObject);            
        }
    }

}
