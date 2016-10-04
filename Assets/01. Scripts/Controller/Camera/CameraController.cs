using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    [SerializeField]
    float ScrollSpeed = 15f;
    [SerializeField]
    float ScrollEdge = 0.01f;
 
 
    public float zoomSpeed = 10f;
    public float minZoomFOV = 2;
    public float maxZoomFOV = 90;


    public float dragSpeed = 0.25f;        
    private Vector3 dragOrigin;

    private Camera thisCamera;
    public static CameraController camController;
    public Vector3 Focus { get; set; }

   Vector3 offSet;
   Quaternion InitRotation;

    void Awake()
    {
        thisCamera = this.GetComponent<Camera>();
        offSet = transform.position;
        InitRotation = transform.rotation;


        if (camController == null)
        {
            camController = this;
        }

        Focus = new Vector3(0f,0f,0f);
    }


    void Update()
    {

        #region Movement
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width * (1 - ScrollEdge))
        {
            transform.Translate(Vector3.right * Time.deltaTime * ScrollSpeed, Space.World);
        }
        else if (Input.GetKey("a") || Input.mousePosition.x <= Screen.width * ScrollEdge)
        {
            transform.Translate(Vector3.right * Time.deltaTime * -ScrollSpeed, Space.World);
        }

        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height * (1 - ScrollEdge))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * ScrollSpeed, Space.World);
        }
        else if (Input.GetKey("s") || Input.mousePosition.y <= Screen.height * ScrollEdge)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * -ScrollSpeed, Space.World);
        }
        #endregion

        #region Roation
        if (Input.GetKey("q"))
        {
            transform.RotateAround(Focus, -transform.forward, dragSpeed + 20f * Time.deltaTime);
        }

        if (Input.GetKey("e"))
        {
            transform.RotateAround(Focus, transform.forward, dragSpeed + 20f * Time.deltaTime);
        }


        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if(Input.GetKey("mouse 1"))
        {
            Vector3 pos = Input.mousePosition - dragOrigin;

            var verticalRotation = pos.y * dragSpeed;


            transform.RotateAround(Focus, -transform.right, verticalRotation * Time.deltaTime);



            var HorizontalRotation = pos.x * dragSpeed;


            transform.RotateAround(Focus, transform.up, HorizontalRotation * Time.deltaTime);

        }
        #endregion

        #region Zoom
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            ZoomIn();
        }
        else if (scroll < 0f)
        {
            ZoomOut();
        }

        #endregion

        #region focus

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ResetOn(Focus);
        }

        #endregion



    }

    public void ZoomIn()
    {
        thisCamera.fieldOfView -= zoomSpeed;
        if (thisCamera.fieldOfView < minZoomFOV)
        {
            thisCamera.fieldOfView = minZoomFOV;
        }
    }

    public void ZoomOut()
    {
        thisCamera.fieldOfView += zoomSpeed;
        if (thisCamera.fieldOfView > maxZoomFOV)
        {
            thisCamera.fieldOfView = maxZoomFOV;
        }
    }


    public void CentreOn(Vector3 point)
    {
        var currentoffSet = transform.position - Focus;
        transform.position = point + currentoffSet;
        Focus = point;
    }

    public void ResetOn(Vector3 point)
    {
        this.transform.position = point + offSet;

        transform.rotation = InitRotation;
        thisCamera.fieldOfView = 50.0f;
        Focus = point;
    }
}
