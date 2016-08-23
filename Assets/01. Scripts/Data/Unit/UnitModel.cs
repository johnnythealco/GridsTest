using UnityEngine;
using System.Collections;

public class UnitModel : MonoBehaviour
{
	Unit unit;
	public Sprite Icon;
	public string DsiplayName;
	public int Movement;
	public int AttackRange;
	public int Health;

	public string faction{ get { return unit.faction; } }

	public int currentMovement{ get { return unit.Movement; } }

	public int currentAttackRange{ get { return unit.attackRange; } }

	public int currentHealth{ get { return unit.attackRange; } }

	public void setUnitState (Unit _unit)
	{
		this.unit = _unit;
	}

}
