using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FleetState
{
    public string Owner;
    public string Name;
    public List<UnitState> Units;


    public FleetState(string _owner, string _name)
    {
        Owner = _owner;
        Name = _name;
        Units = new List<UnitState>();
    }

    public FleetState(string _owner, string _name, List<UnitState> _Units)
    {
        Owner = _owner;
        Name = _name;
        Units = _Units;
    }
}
