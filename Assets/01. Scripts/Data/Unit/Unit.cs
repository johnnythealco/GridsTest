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
	public int movement;
	public int armour;
	public ArmourType armourType;
	public int sheilds;
	public int evasion;


	public List<string> weapons;


	public Unit (UnitModel _type, string _faction)
	{
		this.faction = _faction;
		this.unitType = _type.DsiplayName;
		this.movement = _type.Movement;
		this.weapons = _type.Weapons;
		this.size = _type.Size;
		this.armour = _type.Armour;
	}


}

public enum unitSize
{
	tiny = 0,
	small = 1,
	medium = 2,
	large = 3,
	massive = 4
}

[System.Serializable]
public class Weapon
{
	public string name;
	public Sprite icon;
	public int range = 1;
	public int accuracy = 1;
	public int damage = 10;
	public DamageType damageType;



	public int getDamage (unitSize _targetSize)
	{

		return 0;
	}

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


