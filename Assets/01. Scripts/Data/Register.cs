using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Register : ScriptableObject
{
	#region Unit

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



}
