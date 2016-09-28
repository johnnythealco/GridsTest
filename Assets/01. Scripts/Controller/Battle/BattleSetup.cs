using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class BattleSetup : MonoBehaviour {

    public PlayerListDisplay playerList;
    public FleetListDisplay fleetList;
    

    public Button Start_Battle_Btn;
    public Button Cancel_Btn;
    public Button Add_AI_Player_Btn;
    public Button Remove_AI_Btn;

    void Awake()
    {
        Start_Battle_Btn.interactable = false;
        Cancel_Btn.interactable = true;
        Add_AI_Player_Btn.interactable = false;
        Remove_AI_Btn.interactable = false;

    }
    
    void Start ()
    {
        if (Game.Manager.Players == null)
            Game.Manager.Players = new List<Player>();

        if (Game.isServer)
        { 
            Add_AI_Player_Btn.interactable = true;
            Remove_AI_Btn.interactable = true;
        }
    }

    public void OnPlayerChangedReadyStatus()
    { 
        playerList.UpdateReadyStatus(Game.Manager.Players);

        if(AllplayersReady())
        {
            Start_Battle_Btn.interactable = true;
        }
        else
        {
            Start_Battle_Btn.interactable = false;
        }
    }
    
    public void Add_AI_Player()
    {
        var AI_Name = "Enemy AI :" + Game.Manager.Players.Count();
        
        var _netID = Game.NetworkController.netId.Value;
        var _player = new Player(AI_Name, _netID, false);

        Game.NetworkController.AddPlayer(_player);

    }

    public void Remove_AI_Player()
    {
        var playerCount = Game.Manager.Players.Count();

        for(int i = playerCount -1; i >= 0; i--)
        {
            var _Player = Game.Manager.Players[i];
            if(!_Player.human)
            {

                Game.NetworkController.RemovePlayer(_Player);
                return;
            }
        }
    }

    public void Start_Battle()
    {
        if (!Game.isServer)
            return;

        foreach(var client in Game.NetworkManager.NetworkPlayers)
        {
            client.BattleReady = false;
            client.DeploymentComplete = false;
        }
        

        Game.NetworkManager.LoadScene("Battle");


    }

    public void Cancel()
    {

    }

    public void UpdateFleetList()
    {
        var _fleets = new List<FleetState>();

        foreach(var _Player in Game.Manager.Players)
        {
            _fleets.Add(_Player.fleet);
        }

        fleetList.Prime(_fleets);
    }

    public bool AllplayersReady()
    {
        foreach (var Player in Game.Manager.Players)
        {
            if (Player.ReadyStatus == false)
            {
                return false;
            }
        }
        return true;
    }
    
    public static List<UnitState> CreateUnits(List<string> _UnitTypes, string _Owner)
    {
        var result = new List<UnitState>();

        foreach (var _unit in _UnitTypes)
        {
            var newUnitType = Game.Register.GetUnitType(_unit);

            var newUnit = new UnitState(newUnitType, _Owner);

            result.Add(newUnit);
        }
        return result;
    }

}
