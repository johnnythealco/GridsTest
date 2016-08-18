using UnityEngine;
using System.Collections;

public class CellInput : MonoBehaviour
{

	void OnMouseDown ()
	{
		Debug.Log ("Transform position: " + this.transform.position.ToString ());
	}

}
