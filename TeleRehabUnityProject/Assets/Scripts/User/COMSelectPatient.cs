using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO.Ports;
using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

public class COMSelectPatient : MonoBehaviour {

	public GameObject PanelConf;
	public GameObject PanelSelectCOMPatient;
	public GameObject loginFather;
	public GameObject registerFather;
	public GameObject PhysioUserLabel;
	public GameObject LoginPanel;
	public GameObject RegisterPanel;

	public string[] COMports;

	private string ServerAddress = Register.ServerAddress;
	string[] idsVectorSub;
	string[] usernamesVectorSub;
	string[] namesVectorSub;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	public void COMContinueBT()
	{
		UsersList.patientsessioninfo.SessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		Dropdown COMDropdown = GameObject.Find ("COMPatientDropdown").GetComponent<Dropdown> ();
		UsersList.patientsessioninfo.COMport = COMDropdown.captionText.text;
		Debug.LogWarning(UsersList.patientsessioninfo.Name + " " + UsersList.patientsessioninfo.ID + " " + UsersList.patientsessioninfo.Username + " " + UsersList.patientsessioninfo.SessionDate + " " + UsersList.patientsessioninfo.COMport);
		//Debug.LogWarning(UsersList.patientsessioninfo.Username);
		if (UsersList.patientsessioninfo.COMport != "...") {
			SceneManager.LoadScene("BodyMotion", LoadSceneMode.Single);
			PanelSelectCOMPatient.SetActive (false);
			PanelConf.SetActive (false);
		}

		//SceneManager.LoadScene("PhysioConfiguration", LoadSceneMode.Single); //Cargar siguiente escena
		//SceneManager.UnloadScene("LogIn");
	}

	public void COMBackBT()
	{
		PanelConf.SetActive (false);
		LoginPanel.SetActive (true);
		RegisterPanel.SetActive (true);
	}

	public IEnumerator GetCOMPORTSCoroutine()
	{
		PhysioUserLabel.GetComponent<Text>().text = "Welcome " + UsersList.patientsessioninfo.Username;
		COMports = SerialPort.GetPortNames();
		Dropdown COMDropdown = GameObject.Find ("COMPatientDropdown").GetComponent<Dropdown> ();
		COMDropdown.options.Clear();
		COMDropdown.options.Add (new Dropdown.OptionData () { text = "..." });
		COMDropdown.captionText.text = "...";
		foreach (string c in COMports) {
			COMDropdown.options.Add (new Dropdown.OptionData () { text = c });
		}
		yield return COMports;
		yield return StartCoroutine(UsersCoroutine());
	}

	IEnumerator UsersCoroutine()
	{
		WWWForm getwww = new WWWForm();
		getwww.AddField("Username", UsersList.patientsessioninfo.Username);
		WWW getuserswww = new WWW(ServerAddress + "/getspecificuserdata.php",getwww);

		yield return getuserswww; //Espera a que la clase WWW retorne algo
		yield return StartCoroutine(GetUserCoroutine(getuserswww));
	}

	IEnumerator GetUserCoroutine(WWW getusersw)
	{
		if (getusersw.error == null)
		{
			string data = getusersw.text;
			string[] userVector = data.Split (',');

			UsersList.patientsessioninfo.ID = userVector [0];
			UsersList.patientsessioninfo.Name = userVector [1];

		}
		else
		{
			Debug.LogWarning("Error: " + getusersw.error);
		}
		yield return getusersw;

	}
}
