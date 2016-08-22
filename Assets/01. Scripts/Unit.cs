using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
	public Faction faction;
	public Sprite Icon;
	public string DsiplayName;
	public int Movement;
	public int attackRange;
	public int health;

	public void setState (UnitState _state)
	{
		this.DsiplayName = _state.DsiplayName;
		this.Movement = _state.Movement;
		this.attackRange = _state.attackRange;
		this.health = _state.health;
	}

	public UnitState getState (GameObject _prefab)
	{
		var _state = new UnitState (this);
		_state.prefab = _prefab;

		return _state;
	}


}

public class UnitState
{
	public GameObject prefab;
	public string factionName;
	public string DsiplayName;
	public int Movement;
	public int attackRange;
	public int health;

	public UnitState (Unit _unit)
	{
		this.DsiplayName = _unit.DsiplayName;
		this.Movement = _unit.Movement;
		this.attackRange = _unit.attackRange;
		this.health = _unit.health;

	}

}
