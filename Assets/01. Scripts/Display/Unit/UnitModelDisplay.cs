using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;


public class UnitModelDisplay : MonoBehaviour
{
    public Button ActionButton;
    public Button Next_ActionButton;
    public Button Prev_ActionButton;
    public Text UnitName;
	public Image ActionIcon;
	public Text ActionName;
    public Text currentAP;
    public Text APCost;

	public Text Armour;
	public Text Shields;
	public Transform weaponsPanel;
	public WeaponDisplay weaponDisplayPrefab;

	Unit unit;
	List<WeaponDisplay> weaponDisplays = new List<WeaponDisplay> ();

    int actionPoints;
    int actionPointCost;

	#region Delegates & Events

	public delegate void UnitModelDisplayDelegate ();

	public event UnitModelDisplayDelegate onChangeAction;

	#endregion


	public void Prime (Unit _unitModel)
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
		if (ActionName != null)
			ActionName.text = unit.selectedAction;
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;
        if (currentAP != null)
            currentAP.text = unit.state.AP.ToString();
        if (APCost != null)
            APCost.text = BattleAction.GetAPCost(unit.selectedAction).ToString();



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
        actionPoints = unit.state.AP;
        actionPointCost = BattleAction.GetAPCost(unit.selectedAction);
        checkAP();


        if (!Battle.LocalPlayerTurn)
        {
            ActionName.text = "Waiting";
            ActionButton.interactable = false;
            Next_ActionButton.interactable = false;
            Prev_ActionButton.interactable = false;
        }
        else
        {
            Next_ActionButton.interactable = true;
            Prev_ActionButton.interactable = true;

        }

    }

    public void ActionClick()
    {
        if (!Battle.LocalPlayerTurn)
            return;

        var _selectedAction = unit.selectedAction;

        if (_selectedAction != null || _selectedAction != "")
        {
            BattleAction.Action_Click(_selectedAction);
        }

        unit.state.AP = unit.state.AP - actionPointCost;
        checkAP();

        if (currentAP != null)
            currentAP.text = unit.state.AP.ToString();
        if (APCost != null)
            APCost.text = BattleAction.GetAPCost(unit.selectedAction).ToString();
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

		if (ActionName != null)
			ActionName.text = unit.selectedAction;
		
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;

        if (APCost != null)
            APCost.text = BattleAction.GetAPCost(unit.selectedAction).ToString();

        actionPointCost = BattleAction.GetAPCost(unit.selectedAction);

        if (onChangeAction != null)
			onChangeAction.Invoke ();

        checkAP();
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

		if (ActionName != null)
			ActionName.text = unit.selectedAction;
		if (ActionIcon != null)
			ActionIcon.sprite = _selectedActionIcon;

        if (APCost != null)
            APCost.text = BattleAction.GetAPCost(unit.selectedAction).ToString();

        actionPointCost = BattleAction.GetAPCost(unit.selectedAction);

        if (onChangeAction != null)
			onChangeAction.Invoke ();

        checkAP();
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
        BattleAction.GetLegalTargets("Attack");

        if (APCost != null)
            APCost.text = BattleAction.GetAPCost(unit.selectedAction).ToString();

        actionPointCost = BattleAction.GetAPCost(unit.selectedAction);

        checkAP();
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

    void checkAP()
    {
        actionPoints = unit.state.AP;
        actionPointCost = BattleAction.GetAPCost(unit.selectedAction);

        if (actionPointCost > actionPoints)
            ActionButton.interactable = false;
        else
            ActionButton.interactable = true;
    }



}
