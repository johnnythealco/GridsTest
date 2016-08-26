using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
	public static Game Manager = null;
	public Register register;

	public static string PlayerName{ get; set; }

	public static Battle BattleManager{ get; set; }

	void Awake ()
	{
		if (Manager == null)
			Manager = this;
		else if (Manager != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}
}
