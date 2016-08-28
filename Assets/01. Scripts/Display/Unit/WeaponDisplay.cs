using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
	public Image Icon;
	public Text weaponName;

	Weapon weapon;

	public delegate void WeaponDisplayDelegate (Weapon _weapon);

	public event WeaponDisplayDelegate onClick;

	public void Prime (Weapon _weapon)
	{
		weapon = _weapon;

		Icon.sprite = weapon.icon;
		weaponName.text = weapon.name;
	}

	public void OnClick ()
	{
		if (onClick != null)
			onClick.Invoke (weapon);
	}

}
