using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Unit : MonoBehaviour
{
	#region Variables

	public UnitState state;
	public Sprite Icon;
	public string DsiplayName;

	public unitSize Size;

	public int AP;
	public int Speed;

	public int Armour;
	public ArmourType ArmourType;

	public int Shields;
	public int Engines;
	public int Evasion;

	public List<string> Weapons;
	public List<string> Actions;

	public string selectedWeapon;
	public string selectedAction;

  

	#endregion



	#region Getters & Setters

	public string faction{ get { return state.Owner; } }

	public int currentMovement{ get { return state.engines; } }

	public int currentAttackRange {
		get { 
			var weapon = Game.Register.GetWeapon (selectedWeapon);		
			return weapon.range;
		}
	}

	public int currentArmour{ get { return state.armour; }}


    public TargetType targetType
    {
        get
        {
            if (this == Game.BattleManager.ActiveUnit && this.state.Owner == Game.PlayerName)
                return TargetType.self;

            if (this.state.Owner == Game.PlayerName)
            {
                return TargetType.ally;
            }
            else
            {
                return TargetType.enemy;
            }
        }
    }


    public void setUnitState (UnitState _unit)
	{
		this.state = _unit;
	}



	#endregion

	#region Combat

	public bool AttackWith (string _weapon, string _subSystem)
	{
		var weapon = Game.Register.GetWeapon (_weapon);
		if (weapon.accuracy >= this.state.evasion)
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
				
			}
			break;
		case DamageType.kinetic:
			{
				takeKineticDamage (weaponDamage);                    
			}
			break;
		case DamageType.plasma:
			{
				takePlasmaDamage (weaponDamage);
                    
			}
			break;
		}

		if (state.armour <= 0)
		{
			return true;
		}

		return false;



	}

	#region Damage Types

	void takeLaserDamage (int _damage)
	{

		var Shields = state.shields;
		var armourType = state.armourType;
        float shieldDefense = 1f;
        float lightArmourDefense = 1f;
        float mediumArmourDefense = 0.8f;
        float heavyArmourDefense = 0.5f;

        int shieldDamage = 0;
        int armourDamage = 0;

        #region Shields Remaininag
        if (Shields > 0)
        {               
            shieldDamage = (int)(_damage * shieldDefense);
            this.Shields = this.Shields - shieldDamage;
                
            if (Shields < 0)
            {
                var extraDamage = -Shields;
                switch (armourType)
                {
                    case ArmourType.light:
                        {
                            armourDamage = (int)(extraDamage * lightArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                    case ArmourType.medium:
                        {
                            armourDamage = (int)(extraDamage * mediumArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                    case ArmourType.heavy:
                        {
                            armourDamage = (int)(extraDamage * heavyArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                }
            }            
        }
        #endregion

        #region No Shields Remaining
        else
        {
            switch (armourType)
            {
                case ArmourType.light:
                    {
                        armourDamage = (int)(_damage * lightArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.medium:
                    {
                        armourDamage = (int)(_damage * mediumArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.heavy:
                    {
                        armourDamage = (int)(_damage * heavyArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
            }
        }
        #endregion

        BattleLog.Damage(this, "Laser", _damage, shieldDamage, armourDamage);

    }

    void takeKineticDamage (int _damage)
	{
        var Shields = state.shields;
        var armourType = state.armourType;
        float shieldDefense = 0.5f;
        float lightArmourDefense = 1f;
        float mediumArmourDefense = 0.9f;
        float heavyArmourDefense = 0.6f;

        int shieldDamage = 0;
        int armourDamage = 0;

        #region Shields Remaininag
        if (Shields > 0)
        {

        shieldDamage = (int)(_damage * shieldDefense);
        this.Shields = this.Shields - shieldDamage;

        }
        #endregion

        #region No Shields Remaining
        else
        {
            switch (armourType)
            {
                case ArmourType.light:
                    {
                        armourDamage = (int)(_damage * lightArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.medium:
                    {
                        armourDamage = (int)(_damage * mediumArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.heavy:
                    {
                        armourDamage = (int)(_damage * heavyArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
            }
        }
        #endregion

        BattleLog.Damage(this, "Kinetic", _damage, shieldDamage, armourDamage);

    }

	void takePlasmaDamage (int _damage)
	{
        var Shields = state.shields;
        var armourType = state.armourType;

        float shieldDefense = 0.8f;
        float lightArmourDefense = 1f;
        float mediumArmourDefense = 1f;
        float heavyArmourDefense = 0.8f;

        int shieldDamage = 0;
        int armourDamage = 0;

        #region Shields Remaininag
        if (Shields > 0)
        {
           
            shieldDamage = (int)(_damage * shieldDefense);
            this.Shields = this.Shields - shieldDamage;
                
            if (Shields < 0)
            {
                var extraDamage = -Shields;
                switch (armourType)
                {
                    case ArmourType.light:
                        {
                            armourDamage = (int)(extraDamage * lightArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                    case ArmourType.medium:
                        {
                            armourDamage = (int)(extraDamage * mediumArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                    case ArmourType.heavy:
                        {
                            armourDamage = (int)(extraDamage * heavyArmourDefense);
                            if (armourDamage > 0)
                                state.armour = state.armour - armourDamage;
                        }
                        break;
                }
            }
            
        }
        #endregion

        #region No Shields Remaining
        else
        {
            switch (armourType)
            {
                case ArmourType.light:
                    {
                        armourDamage = (int)(_damage * lightArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.medium:
                    {
                        armourDamage = (int)(_damage * mediumArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
                case ArmourType.heavy:
                    {
                        armourDamage = (int)(_damage * heavyArmourDefense);
                        if (armourDamage > 0)
                            state.armour = state.armour - armourDamage;
                    }
                    break;
            }
        }
        #endregion

        BattleLog.Damage(this, "Plasma", _damage, shieldDamage, armourDamage);


    }

    #endregion

    public void DestroyUnit ()
	{
        if (Battle.AllUnits.Contains(this))
            Battle.AllUnits.Remove(this);

        if (Battle.AllEnemies.Contains(this))
            Battle.AllEnemies.Remove(this);

        Destroy (gameObject);
	}

	#endregion

}
