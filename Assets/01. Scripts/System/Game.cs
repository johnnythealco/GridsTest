using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
	public Register register;

	public static Game Manager = null;

	public static Register Register{ get; set; }

	public static string PlayerName{ get; set; }

	public static Battle BattleManager{ get; set; }

	void Awake ()
	{
		if (Manager == null)
		{
			Manager = this;
			Register = register;
		} else if (Manager != this)
			Destroy (gameObject);


		DontDestroyOnLoad (gameObject);
	}
}
