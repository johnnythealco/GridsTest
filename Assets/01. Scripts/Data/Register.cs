﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Register : ScriptableObject
{
	#region Units

	public List<UnitModel> unitTypes;

	public UnitModel GetUnitType (string _dsiplayname)
	{
		UnitModel result = null;

		foreach (var unitType in unitTypes)
		{
			if (unitType.DsiplayName == _dsiplayname)
				result = unitType;
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

	#endregion

	#region Icons

	public List<Icon> Icons;

	public Sprite Geticon (string _name)
	{
		Sprite result = null;

		foreach (var icon in Icons)
		{
			if (icon.name == _name)
				result = icon.icon;
		}
		return result;

	}

	#endregion



}
