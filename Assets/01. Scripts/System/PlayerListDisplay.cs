using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerListDisplay : MonoBehaviour {


    List<PlayerDisplay> Displays;

    public PlayerDisplay playerDisplay;
    public Transform listTransform;

    

    public void Prime(List<ClientInput> _Players)
    {
        clearDisplays();  

        foreach (var _player in _Players)
        {
            var _display = (PlayerDisplay)Instantiate(playerDisplay);
            _display.transform.SetParent(listTransform);
            _display.Prime(_player);
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
