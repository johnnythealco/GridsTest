using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TestDeployment : MonoBehaviour
{

    List<string> Fleet;
  
    void netWorkDeploy(UnitState _unit, Vector3 _position)
    {
        var _unitJSON = JsonUtility.ToJson(_unit);
        var LocalPlayer = GameObject.Find("Local Player").GetComponent<ClientInput>();
        LocalPlayer.CmdDeploy(_unitJSON, _position);
    }
    
    public void QuickDeploy(List<UnitState> _Units)
    {
        System.Random rnd = new System.Random();
        foreach (var _unit in _Units)
        {
            var i = rnd.Next(Game.GridPoints.Count());
            var _point = Game.GridPoints[i];
            while (Game.BattleManager.BattleGrid.GetCellAccessiblity(_point) == false)
            {
                i = rnd.Next(Game.GridPoints.Count());
                _point = Game.GridPoints[i];
            }

            netWorkDeploy(_unit, _point);
        }

    }

}
