using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
	public Register register;

	public static Game Manager = null;

	public static Register Register{ get; set; }

	public static string PlayerName{ get; set; }

	public static Battle BattleManager{ get; set; }

	public static List<Vector3> GridPoints{ get; set; }

    public List<ClientInput> Players = new List<ClientInput>();

	void Awake ()
	{
		if (Manager == null)
		{
			Manager = this;
			Register = register;

		} else if (Manager != this)
			Destroy (gameObject);


		DontDestroyOnLoad (gameObject);

		Debug.Log ("Game Manager is Awake"); 
	}
}
