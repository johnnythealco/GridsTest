using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
	public static Game Manager = null;
	public Register register;

	void Awake ()
	{
		if (Manager == null)
			Manager = this;
		else if (Manager != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}
}
