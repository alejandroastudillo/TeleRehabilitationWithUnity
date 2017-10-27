using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO.Ports;
using System;
using System.Text.RegularExpressions;

public class SessionSelect : MonoBehaviour {

	public GameObject PanelSelectPatient;
	public GameObject PanelSelectSession;

	private string ServerAddress = Register.ServerAddress;
	private string id_selected = "";
    public static string SessionDate, SessionUserID;

	//UsersList.PatientSessionInfo patientsessioninf;

	// Use this for initialization
	void Start () {
		//FillSessionDropdown ();
	}
	
	public void FillSessionDropdown()
	{
		//StartCoroutine(GetSessionsCoroutine());
	}

	public void BackSESSIONBT()
	{
		PanelSelectSession.SetActive (false);
		PanelSelectPatient.SetActive (true);
	}

	public void ContinueSESSIONBT()
	{
		Dropdown sessionDropdown = GameObject.Find ("SessionDropdown").GetComponent<Dropdown> ();
		UsersList.patientsessioninfo.SessionDate = sessionDropdown.captionText.text;
        SessionDate = sessionDropdown.captionText.text;
        SessionUserID = UsersList.patientsessioninfo.ID;
        //Debug.LogWarning(UsersList.patientsessioninfo.Name + " " + UsersList.patientsessioninfo.ID + " " + UsersList.patientsessioninfo.Username + " " + UsersList.patientsessioninfo.SessionDate);
        Debug.LogError(UsersList.patientsessioninfo.Name + " " + SessionUserID + " " + UsersList.patientsessioninfo.Username + " " + SessionDate + " --:-- " + UsersList.patientsessioninfo.COMport);

        if (SessionDate != null)
        {
            //PanelSelectSession.SetActive(false);
            SceneManager.LoadScene("CheckSession", LoadSceneMode.Single);
        }
    }

	public IEnumerator GetSessionsCoroutine()
	{	
		//patientsessioninf = UsersList.patientsessioninfo; 
		//WWWForm getidformwww = new WWWForm();
		//getidformwww.AddField("Name", Login.PatientName);
		//WWW getidwww = new WWW(ServerAddress + "/getidforsessions.php",getidformwww);

		//yield return getidwww; //Espera a que la clase WWW retorne algo
		////Debug.LogWarning(Login.PatientName + " con ID: " + getidwww.text);
		//yield return StartCoroutine(GetIDCoroutine(getidwww));
		id_selected = UsersList.patientsessioninfo.ID;

		WWWForm getsessionsformwww = new WWWForm();
		getsessionsformwww.AddField("ID", id_selected);
		WWW getsessionswww = new WWW(ServerAddress + "/getsessions.php",getsessionsformwww);

		yield return getsessionswww; //Espera a que la clase WWW retorne algo
		//Debug.LogWarning(Login.PatientName + " con ID: " + getidwww.text);
		yield return StartCoroutine(GetSessionsCoroutine(getsessionswww));
	}

	IEnumerator GetIDCoroutine(WWW getidw)
	{
		if (getidw.error == null)
		{
			id_selected = getidw.text;
			Debug.LogWarning ("ID: "+id_selected);
		}
		else
		{
			Debug.LogWarning("Error: " + getidw.error);
		}
		yield return id_selected;

	}

	IEnumerator GetSessionsCoroutine(WWW getsessionsw)
	{
		if (getsessionsw.error == null)
		{
			string data = getsessionsw.text;
			string[] sessionsVector = data.Split (',');
			string[] sessionsVectorSub = new string[sessionsVector.Length - 1];
			Array.Copy(sessionsVector, 0, sessionsVectorSub, 0, sessionsVector.Length - 1); //Get a 30 bytes frame from the readFrame (210 bytes).
			// Array.Copy(source array, start index in source array, destination array, start index in destination array, elements to copy);
			Dropdown sessionDropdown = GameObject.Find ("SessionDropdown").GetComponent<Dropdown> ();
			foreach (string c in sessionsVectorSub) {
				sessionDropdown.options.Add (new Dropdown.OptionData () { text = c });
			}

		}
		else
		{
			Debug.LogWarning("Error: " + getsessionsw.error);
		}
		yield return getsessionsw;

	}


}
