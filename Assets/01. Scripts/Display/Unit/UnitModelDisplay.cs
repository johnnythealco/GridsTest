using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;


public class UnitModelDisplay : MonoBehaviour
{
	
	public Text UnitName;
	public Image ActionIcon;

	public Text Action;

	public Text Armour;
	public Text Shields;
	public Transform weaponsPanel;
	public WeaponDisplay weaponDisplayPrefab;

	UnitModel unit;
	List<WeaponDisplay> weaponDisplays = new List<WeaponDisplay> ();

	#region Delegates & Events

	public delegate void UnitModelDisplayDelegate ();

	public event UnitModelDisplayDelegate onChangeAction;

	#endregion


	public void Prime (UnitModel _unitModel)
	{
		unit = _unitModel;
		clearWeaponDisplays ();

		if (unit.selectedWeapon == null || unit.selectedWeapon == "")
			unit.selectedWeapon = unit.Weapons.First ();

		if (unit.selectedAction == null || unit.selectedAction == "")
			unit.selectedAction = unit.Actions.First ();

		var _selectedActionIcon = Game.Register.GetActionIcon (_unitModel.selectedAction);

		if (UnitName != null)
			UnitName.text = unit.DsiplayName;
		if (Action != null)
			Action.text = unit.selectedAction;
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;
		if (Armour != null)
			Armour.text = unit.Armour.ToString ();
		if (Shields != null)
			Shields.text = unit.Sheilds.ToString ();





		if (weaponsPanel != null)
		{
			foreach (var item in unit.Weapons)
			{
				var weapon = Game.Register.GetWeapon (item);
				var weaponDsiplay = (WeaponDisplay)Instantiate (weaponDisplayPrefab);
				weaponDsiplay.transform.SetParent (weaponsPanel, false);
				weaponDsiplay.Prime (weapon);
				weaponDsiplay.onClick += WeaponDsiplay_onClick;
				weaponDisplays.Add (weaponDsiplay);
			}

			highlightSelectedWeapon ();
		}

	}

	public void NextAction ()
	{
		var _actionList = unit.Actions;
		var i = _actionList.IndexOf (unit.selectedAction);

		if (i < _actionList.Count () - 1)
		{
			unit.selectedAction = _actionList [i + 1];
		} else
		{
			unit.selectedAction = _actionList [0];
		}

		var _selectedActionIcon = Game.Register.GetActionIcon (unit.selectedAction);

		if (Action != null)
			Action.text = unit.selectedAction;
		
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;

		if (onChangeAction != null)
			onChangeAction.Invoke ();
	}

	public void PrevAction ()
	{
		var _actionList = unit.Actions;
		var i = _actionList.IndexOf (unit.selectedAction);

		if (i > 0)
		{
			unit.selectedAction = _actionList [i - 1];
		} else
		{
			unit.selectedAction = _actionList [_actionList.Count () - 1];
		}

		var _selectedActionIcon = Game.Register.GetActionIcon (unit.selectedAction);

		if (Action != null)
			Action.text = unit.selectedAction;
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;

		if (onChangeAction != null)
			onChangeAction.Invoke ();
	}

	public void SetAction (TargetType _targetType)
	{

		foreach (var _action in unit.Actions)
		{
			if (Game.Register.GetActionTargetType (_action) == _targetType)
			{
				unit.selectedAction = _action;
				var _selectedActionIcon = Game.Register.GetActionIcon (unit.selectedAction);

				if (Action != null)
					Action.text = unit.selectedAction;
				if (ActionIcon != null)
					ActionIcon.sprite = _selectedActionIcon;
				if (onChangeAction != null)
					onChangeAction.Invoke ();
				return;
			}
				
		}
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
        BattleAction.GetLegalTargets(_weapon.name);
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

		weaponDisplays.Clear ();

	}

	public void HighlightAction ()
	{
		if (Action != null)
			Action.fontStyle = FontStyle.Bold;
	}

	public void UnHighlightAction ()
	{
		if (Action != null)
			Action.fontStyle = FontStyle.Normal;
	}

	public void Action_Click ()
	{
		var _selectedAction = unit.selectedAction;

		if (_selectedAction != null || _selectedAction != "")
		{
			BattleAction.Action_Click (_selectedAction);
		}


	}



}
