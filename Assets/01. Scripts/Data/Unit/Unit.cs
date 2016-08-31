using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Unit
{
	public string faction;
	public string unitType;
	public unitSize size;

	public int AP;
	public int speed;

	public int armour;
	public ArmourType armourType;

	public int sheilds;
	public int engines;
	public int evasion;


	public List<string> weapons;
	public List<string> actions;


	public Unit (UnitModel _type, string _faction)
	{
		this.faction = _faction;
		this.unitType = _type.DsiplayName;
		this.engines = _type.Engines;
		this.weapons = _type.Weapons;
		this.actions = _type.Actions;
		this.size = _type.Size;
		this.armour = _type.Armour;
	}


}

#region Weapon
[System.Serializable]
public class Weapon
{
	public string name;
	public Sprite icon;
	public int range = 1;
	public int APcost;
	public int accuracy = 1;
	public int damage = 10;
	public DamageType damageType;

}
#endregion

#region Action
[System.Serializable]
public class Icon
{
	public string name;
	public Sprite icon;

}
#endregion


#region Enums
public enum unitSize
{
	tiny = 0,
	small = 1,
	medium = 2,
	large = 3,
	massive = 4
}



public enum DamageType
{
	kinetic = 0,
	laser = 1,
	plasma = 2
}

public enum ArmourType
{
	light = 0,
	medium = 1,
	heavy = 2
}

[System.Serializable]
public class UnitCost
{
	public string unitType;
	public int cost;
}

#endregion

