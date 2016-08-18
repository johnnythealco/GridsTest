using UnityEngine;
using System.Collections;

public class CameraCTRL : MonoBehaviour
{
	public float zoomSpeed = 10f;
	public float minZoomFOV = 2;
	public float maxZoomFOV = 90;

	public float dragSpeed = 0.25f;

	public static CameraCTRL camController;

	private Vector3 dragOrigin;

	private Camera thisCamera;
	public Vector3 offSet;

	void Awake ()
	{
		thisCamera = this.GetComponent<Camera> ();
		if (camController == null)
		{
			camController = this;
		}
	}


	void LateUpdate ()
	{
		
		var scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll > 0f)
		{
			ZoomIn ();
		} else if (scroll < 0f)
		{
			ZoomOut ();
		}


		if (Input.GetMouseButtonDown (1))
		{
			dragOrigin = Input.mousePosition;
			return;
		}

		if (!Input.GetMouseButton (1))
			return;
		{
			Vector3 pos = Camera.main.ScreenToViewportPoint (Input.mousePosition - dragOrigin);

			Vector3 move = new Vector3 (pos.x * dragSpeed, 0, pos.y * dragSpeed);


			transform.Translate (move, Space.World);
		}



	}

	public void ZoomIn ()
	{
		thisCamera.fieldOfView -= zoomSpeed;
		if (thisCamera.fieldOfView < minZoomFOV)
		{
			thisCamera.fieldOfView = minZoomFOV;
		}
	}

	public void ZoomOut ()
	{
		thisCamera.fieldOfView += zoomSpeed;
		if (thisCamera.fieldOfView > maxZoomFOV)
		{
			thisCamera.fieldOfView = maxZoomFOV;
		}
	}

	public void CentreOn (Vector3 point)
	{
		this.transform.position = point + offSet;
	}

}
