using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class JKLog : MonoBehaviour
{
	public Transform log;
	public Text logEnrty;
	public Scrollbar scrollbar;
	public static JKLog GameLog;

	// Use this for initialization
	void Awake ()
	{
		if (GameLog == null)
			GameLog = this;
		else if (GameLog != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	
	}

	public static void Log (string _text)
	{
		var entry = Instantiate (JKLog.GameLog.logEnrty);
		entry.text = _text;


		entry.transform.SetParent (JKLog.GameLog.log);

		JKLog.GameLog.scrollbar.value = 0;
	}
	

}
