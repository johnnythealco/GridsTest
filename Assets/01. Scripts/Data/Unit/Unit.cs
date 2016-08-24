using UnityEngine;
using System.Collections;

[System.Serializable]
public class Unit
{
	public string faction;
	public string unitType;
	public int Movement;
	public int attackRange;
	public int health;
	public int damage;

	public Unit (UnitModel _type, string _faction)
	{
		this.faction = _faction;
		this.unitType = _type.DsiplayName;
		this.Movement = _type.Movement;
		this.attackRange = _type.AttackRange;
		this.health = _type.Health;
		this.damage = _type.Damage;
	}


}
