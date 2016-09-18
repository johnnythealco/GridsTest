using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitModel : MonoBehaviour
{
	#region Variables

	public Unit unit;
	public Sprite Icon;
	public string DsiplayName;

	public unitSize Size;

	public int AP;
	public int Speed;

	public int Armour;
	public ArmourType ArmourType;

	public int Sheilds;
	public int Engines;
	public int Evasion;

	public List<string> Weapons;
	public List<string> Actions;

	public string selectedWeapon;
	public string selectedAction;

  

	#endregion



	#region Getters & Setters

	public string faction{ get { return unit.Owner; } }

	public int currentMovement{ get { return unit.engines; } }

	public int currentAttackRange {
		get { 
			var weapon = Game.Register.GetWeapon (selectedWeapon);		
			return weapon.range;
		}
	}

	public int currentArmour{ get { return unit.armour; }}


    public TargetType targetType
    {
        get
        {
            if (this == BattleAction.ActiveUnit && this.unit.Owner == Game.PlayerName)
                return TargetType.self;

            if (this.unit.Owner == Game.PlayerName)
            {
                return TargetType.ally;
            }
            else
            {
                return TargetType.enemy;
            }
        }
    }


    public void setUnitState (Unit _unit)
	{
		this.unit = _unit;
	}



	#endregion

	#region Combat

	public bool AttackWith (string _weapon, string _subSystem)
	{
		var weapon = Game.Register.GetWeapon (_weapon);
		if (weapon.accuracy >= this.unit.evasion)
		{
			return true;
		}

		return false;

	}

	public bool HitBy (string _weapon)
	{
		var weapon = Game.Register.GetWeapon (_weapon);
		var weaponDamage = weapon.damage;




		switch (weapon.damageType)
		{
		case DamageType.laser:
			{
				takeLaserDamage (weaponDamage);
				Debug.Log (" Laser damage :" + weaponDamage.ToString ());
			}
			break;
		case DamageType.kinetic:
			{
				takeKineticDamage (weaponDamage);
				Debug.Log (" Kinetic damage :" + weaponDamage.ToString ());
			}
			break;
		case DamageType.plasma:
			{
				takePlasmaDamage (weaponDamage);
				Debug.Log (" Plasma damage :" + weaponDamage.ToString ());
			}
			break;
		}

		if (unit.armour <= 0)
		{
			return true;
		}

		return false;



	}

	#region Damage Types

	void takeLaserDamage (int _damage)
	{

		var sheilds = unit.sheilds;

		var armourType = unit.armourType; 
		
		if (sheilds > 0)
		{
			{
				sheilds = sheilds - _damage;
				Debug.Log ("");

			}
			if (sheilds < 0)
			{
				var extraDamage = -sheilds;
				switch (armourType)
				{
				case ArmourType.light:
					{
						if ((extraDamage - 1) > 0)
							unit.armour = unit.armour - (extraDamage - 1);
					}
					break;
				case ArmourType.medium:
					{
						if ((extraDamage - 10) > 0)
							unit.armour = unit.armour - (extraDamage - 10);
					}
					break;
				case ArmourType.heavy:
					{
						if ((extraDamage - 100) > 0)
							unit.armour = unit.armour - (extraDamage - 100);
					}
					break;

				}
			}

		} else
		{

			switch (armourType)
			{
			case ArmourType.light:
				{
					if ((_damage - 1) > 0)
						unit.armour = unit.armour - (_damage - 1);
				}
				break;
			case ArmourType.medium:
				{
					if ((_damage - 10) > 0)
						unit.armour = unit.armour - (_damage - 10);
				}
				break;
			case ArmourType.heavy:
				{
					if ((_damage - 100) > 0)
						unit.armour = unit.armour - (_damage - 100);
				}
				break;

			}
		}
	}

	void takeKineticDamage (int _damage)
	{

		var sheilds = unit.sheilds;

		var armourType = unit.armourType; 

		if (sheilds > 0)
		{
			{
				sheilds = sheilds - (_damage / 2);
	
			}
		} else
		{
			
			switch (armourType)
			{
			case ArmourType.light:
				{
					if ((_damage - 1) > 0)
						unit.armour = unit.armour - (_damage - 1);
				}
				break;
			case ArmourType.medium:
				{
					if ((_damage - 10) > 0)
						unit.armour = unit.armour - (_damage - 10);
				}
				break;
			case ArmourType.heavy:
				{
					if ((_damage - 100) > 0)
						unit.armour = unit.armour - (_damage - 100);
				}
				break;

			}
		}

	}

	void takePlasmaDamage (int _damage)
	{

		var sheilds = unit.sheilds;

		var armourType = unit.armourType; 

		if (sheilds > 0)
		{
			{
				sheilds = sheilds - (_damage / 2);

			}
		} else
		{

			switch (armourType)
			{
			case ArmourType.light:
				{
					if ((_damage - 1) > 0)
						unit.armour = unit.armour - (_damage - 1);
				}
				break;
			case ArmourType.medium:
				{
					if ((_damage - 10) > 0)
						unit.armour = unit.armour - (_damage - 10);
				}
				break;
			case ArmourType.heavy:
				{
					if ((_damage - 100) > 0)
						unit.armour = unit.armour - (_damage - 100);
				}
				break;

			}
		}

	}

	#endregion

	public void DestroyUnit ()
	{
		Destroy (gameObject);
	}

	#endregion

}
