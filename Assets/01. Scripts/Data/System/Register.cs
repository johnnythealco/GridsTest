using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Register : ScriptableObject
{
	#region Units

	public List<Unit> unitTypes;

	public Unit GetUnitType (string _UnitType)
	{
		Unit result = null;

		foreach (var unitType in unitTypes)
		{
			if (unitType.DsiplayName == _UnitType)
				result = unitType;
		}

		return result;

	}

	public List<Unit> GetUnitTypes (List<string> _UnitTypes)
	{
		var result = new List<Unit> ();

		foreach (var name in _UnitTypes)
		{
			result.Add (GetUnitType (name));
		}

		return result;
	}

	#endregion

	#region Weapons

	public List<Weapon> Weapons;

	public Weapon GetWeapon (string _name)
	{
		Weapon result = null;

		foreach (var weapon in Weapons)
		{
			if (weapon.name == _name)
				result = weapon;
		

		}
		return result;
	}

    public int GetWeaponAPcost(string _name)
    {
        int result = 0;

        foreach (var weapon in Weapons)
        {
            if (weapon.name == _name)
                result = weapon.APcost;
        }
        return result;
    }

    #endregion

    #region Actions

    public List<UnitAction> Actions;

	public Sprite GetActionIcon (string _name)
	{
		Sprite result = null;

		foreach (var action in Actions)
		{
			if (action.name == _name)
				result = action.icon;
		}
		return result;

	}

	public TargetType GetActionTargetType (string _name)
	{
		

		foreach (var action in Actions)
		{
			if (action.name == _name)
				return action.targetType;
		}
		return TargetType.empty;

	}

    public int GetActionRange(string _name)
    {
        int result = 0;

        foreach (var action in Actions)
        {
            if (action.name == _name)
                result = action.range; 
        }
        return result; 

    }

    public int GetActionAPCost(string _name)
    {
        int result = 0;

        foreach (var action in Actions)
        {
            if (action.name == _name)
                result = action.APCost;
        }
        return result;

    }

    #endregion



}
