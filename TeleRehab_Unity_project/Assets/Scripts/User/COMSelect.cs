using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO.Ports;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public class COMSelect : MonoBehaviour {

	public GameObject PanelSelectPatient;
	public GameObject PanelSelectCOM;
	public string[] COMports;

	// Use this for initialization
	void Start () {
		FillCOMDropdown ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void FillCOMDropdown()
	{
		//StartCoroutine(GetCOMportsCoroutine());
	}

	public void COMContinueBT()
	{
		//DateTime localDate = DateTime.Now;
		UsersList.patientsessioninfo.SessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		Dropdown COMDropdown = GameObject.Find ("COMDropdown").GetComponent<Dropdown> ();
		UsersList.patientsessioninfo.COMport = COMDropdown.captionText.text;
		Debug.LogWarning(UsersList.patientsessioninfo.Name + " " + UsersList.patientsessioninfo.ID + " " + UsersList.patientsessioninfo.Username + " " + UsersList.patientsessioninfo.SessionDate + " " + UsersList.patientsessioninfo.COMport);

		if (UsersList.patientsessioninfo.COMport != "...") {
			SceneManager.LoadScene("BodyMotion", LoadSceneMode.Single);
		}
		PanelSelectCOM.SetActive (false);
		PanelSelectPatient.SetActive (false);
		//SceneManager.LoadScene("PhysioConfiguration", LoadSceneMode.Single); //Cargar siguiente escena
		//SceneManager.UnloadScene("LogIn");
	}

	public void COMBackBT()
	{
		PanelSelectCOM.SetActive (false);
		PanelSelectPatient.SetActive (true);
	}

	public IEnumerator GetCOMportsCoroutine()
	{
		COMports = SerialPort.GetPortNames();
		Dropdown COMDropdown = GameObject.Find ("COMDropdown").GetComponent<Dropdown> ();
		COMDropdown.options.Clear();
		COMDropdown.options.Add (new Dropdown.OptionData () { text = "..." });
		COMDropdown.captionText.text = "...";

		foreach (string c in COMports) {
			COMDropdown.options.Add (new Dropdown.OptionData () { text = c });
		}
		yield return COMports;
	}
}
