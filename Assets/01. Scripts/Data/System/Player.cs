using UnityEngine;
using System.Collections;

[System.Serializable]
public class Player
{
    public string Name;
    public uint ConnectionID;
    public bool connected;
    public bool ReadyStatus;
    public bool human;
    public FleetState fleet;



    public Player(string _name, uint _id, bool _human)
    {
        this.Name = _name;
        this.ConnectionID = _id;
        this.connected = true;

        if(_human)
            this.ReadyStatus = false;
        else
            this.ReadyStatus = true;

        this.human = _human;

        fleet = new FleetState(this.Name, "Basic Fleet");
        var _units = BattleSetup.CreateUnits(Game.Manager.BasicFleet, this.Name);
        fleet.Units = _units;



    }

    public Player(string _name, uint _id, bool _human, FleetState _fleet)
    {
        this.Name = _name;
        this.ConnectionID = _id;
        this.connected = true;

        if (_human)
            this.ReadyStatus = false;
        else
            this.ReadyStatus = true;

        this.human = _human;

        fleet = _fleet;


    }

    #region Overides
    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        Player p = obj as Player;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (Name == p.Name) && (ConnectionID == p.ConnectionID);
    }

    public bool Equals(Player p)
    {
        // If parameter is null return false:
        if ((object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (Name == p.Name) && (ConnectionID == p.ConnectionID);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode() ^ (int)ConnectionID;
    }
    #endregion


}
