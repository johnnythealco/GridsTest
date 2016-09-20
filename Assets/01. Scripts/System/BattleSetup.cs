using UnityEngine;
using System.Collections.Generic;

public class BattleSetup : MonoBehaviour {

    public PlayerListDisplay playerList;

   
    void Start ()
    {
        if (Game.Manager.Players == null)
            Game.Manager.Players = new List<ClientInput>();



	
	}

    public void AddPlayer(ClientInput _Player)
    {
        Game.Manager.Players.Add(_Player);
        playerList.Prime(Game.Manager.Players);

    }



}
