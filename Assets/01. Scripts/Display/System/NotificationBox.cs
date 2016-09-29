using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotificationBox : MonoBehaviour
{
    public Text NotificationText;
    

    public delegate void NotificationBoxDelegate();
    public event NotificationBoxDelegate onClick_Ok;

    public void Prime(string _Notification)
    {
        if (NotificationText != null)
            NotificationText.text = _Notification;
    }

    public void Click_Ok()
    {
        if(onClick_Ok != null)
        {
            onClick_Ok.Invoke();
        }
    }


}
