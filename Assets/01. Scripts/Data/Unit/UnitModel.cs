using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitModel : MonoBehaviour
{
	Unit unit;
	public Sprite Icon;
	public string DsiplayName;
	public unitSize Size;
	public int Movement;
	public int Health;
	public List<string> Weapons;
	public string selectedWeapon;


	public string faction{ get { return unit.faction; } }

	public int currentMovement{ get { return unit.Movement; } }

	public int currentAttackRange {
		get { 
			var weapon = Game.Register.GetWeapon (selectedWeapon);		
			return weapon.range;
		}
	}

	public int currentHealth{ get { return unit.health; } }

	public int getDamage (unitSize _targetSize)
	{
		if (selectedWeapon == null || selectedWeapon == "")
			selectedWeapon = Weapons.First ();
		
		var weapon = Game.Register.GetWeapon (selectedWeapon);
		if (weapon != null)
			return weapon.getDamage (_targetSize);
		else
			return 0;


		
	}

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
