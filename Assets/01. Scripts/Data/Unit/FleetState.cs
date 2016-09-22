using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FleetState
{
    public string owner;
    public string Name;
    public List<UnitState> Units;


    public FleetState(string _owner, string _name)
    {
        Units = new List<UnitState>();
    }
}
