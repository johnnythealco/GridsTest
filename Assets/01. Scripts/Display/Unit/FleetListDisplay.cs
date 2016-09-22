using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FleetListDisplay : MonoBehaviour
{
    List<FleetDisplay> Displays = new List<FleetDisplay>();

    public FleetDisplay fleetDisplay;
    public Transform listTransform;


    public void Prime(List<FleetState> _Fleets)
    {
        clearDisplays();

        foreach (var _fleet in _Fleets)
        {
            FleetDisplay _display = (FleetDisplay)Instantiate(fleetDisplay, listTransform, false);
            _display.transform.SetParent(listTransform);
            _display.Prime(_fleet);
            Displays.Add(_display);
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
