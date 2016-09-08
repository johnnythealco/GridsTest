using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BattleActionDisplay : MonoBehaviour
{

	public Image Icon;
	public Text actionName;

	string action;

	public delegate void BattleActionDisplayDelegate (string _actionName);

	public event BattleActionDisplayDelegate onClick;

	public void Prime (string _actionName)
	{
		action = _actionName;
		var actionIcon = Game.Register.GetActionIcon (action);

		if (Icon != null)
			Icon.sprite = actionIcon;

		if (actionName != null)
			actionName.text = action;
	}

	public void OnClick ()
	{
		if (onClick != null)
			onClick.Invoke (action);
	}
}
