using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Unit
{
	public string faction;
	public string unitType;
	public unitSize size;
	public int Movement;
	public int health;

	public List<string> weapons;


	public Unit (UnitModel _type, string _faction)
	{
		this.faction = _faction;
		this.unitType = _type.DsiplayName;
		this.Movement = _type.Movement;
		this.weapons = _type.Weapons;
		this.size = _type.Size;
		this.health = _type.Health;

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
	public int damageTiny = 10;
	public int damageSmall = 10;
	public int damageMedium = 10;
	public int damageLarge = 10;
	public int damageMassive = 10;

	public int getDamage (unitSize _targetSize)
	{
		switch (_targetSize)
		{
		case unitSize.tiny:
			return damageTiny;
		case unitSize.small:
			return damageSmall;
		case unitSize.medium:
			return damageMedium;
		case unitSize.large:
			return damageLarge;
		case unitSize.massive:
			return damageMassive;
		}

		return 0;
	}

}

[System.Serializable]
public class UnitCost
{
	public string unitType;
	public int cost;
}


