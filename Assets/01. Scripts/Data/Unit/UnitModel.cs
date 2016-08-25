using UnityEngine;
using System.Collections;

[System.Serializable]
public class UnitModel : MonoBehaviour
{
	Unit unit;
	public Sprite Icon;
	public string DsiplayName;
	public int Movement;
	public int AttackRange;
	public int Health;
	public int Damage;

	public string faction{ get { return unit.faction; } }

	public int currentMovement{ get { return unit.Movement; } }

	public int currentAttackRange{ get { return unit.attackRange; } }

	public int currentHealth{ get { return unit.health; } }

	public int currentDamage{ get { return unit.damage; } }

	public void setUnitState (Unit _unit)
	{
		this.unit = _unit;
	}

	public bool TakeDirectDamage (int _damage)
	{
		unit.health = unit.health - _damage;

		if (unit.health <= 0)
			return true;
		else
			return false;

	}

	public void DestroyUnit ()
	{
		Destroy (gameObject);
	}

}
