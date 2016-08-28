using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;


public class UnitModelDisplay : MonoBehaviour
{
	public Text unitName;
	public Image Icon;
	public Text unitHealth;
	public Transform weaponsPanel;
	public WeaponDisplay weaponDisplayPrefab;

	UnitModel unit;
	List<WeaponDisplay> weaponDisplays = new List<WeaponDisplay> ();

	public void Prime (UnitModel _unitModel)
	{
		unit = _unitModel;
		clearWeaponDisplays ();

		unitName.text = unit.DsiplayName;
		Icon.sprite = unit.Icon;
		unitHealth.text = unit.Health.ToString ();

		if (unit.selectedWeapon == null || unit.selectedWeapon == "")
			unit.selectedWeapon = unit.Weapons.First ();


		foreach (var item in unit.Weapons)
		{
			var weapon = Game.Register.GetWeapon (item);
			var weaponDsiplay = (WeaponDisplay)Instantiate (weaponDisplayPrefab);
			weaponDsiplay.transform.SetParent (weaponsPanel);
			weaponDsiplay.Prime (weapon);
			weaponDsiplay.onClick += WeaponDsiplay_onClick;
			weaponDisplays.Add (weaponDsiplay);
		}

		highlightSelectedWeapon ();

	}

	void highlightSelectedWeapon ()
	{
		foreach (var item in weaponDisplays)
		{
			if (item.weaponName.text == unit.selectedWeapon)
				item.weaponName.fontStyle = FontStyle.Bold;
			else
				item.weaponName.fontStyle = FontStyle.Normal;
		}


	}

	void WeaponDsiplay_onClick (Weapon _weapon)
	{
		unit.selectedWeapon = _weapon.name;
		highlightSelectedWeapon ();
	}

	void onDestroy ()
	{
		clearWeaponDisplays ();
	}

	void clearWeaponDisplays ()
	{
		foreach (var weaponDsiplay in weaponDisplays)
		{
			weaponDsiplay.onClick -= WeaponDsiplay_onClick;
		}

		for (int i = 0; i < weaponsPanel.childCount; i++)
		{
			Destroy (weaponsPanel.GetChild (i).gameObject);
		}

	}


}
