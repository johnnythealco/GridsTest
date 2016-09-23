using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq; 

public class FleetDisplay : MonoBehaviour {

    FleetState fleet;

    public Text ownerName;
    public Text fleetName;
    public Text unitCount;

    public void Prime(FleetState _fleet)
    {
        fleet = _fleet;

        if (ownerName != null)
            ownerName.text = fleet.Owner;

        if (fleetName != null)
            fleetName.text = fleet.Name;

        if (unitCount != null)
            unitCount.text = fleet.Units.Count().ToString();
    }


}
