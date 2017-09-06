using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO.Ports;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class UsersList : MonoBehaviour 
{
	public GameObject PhysioDropdown;
	public GameObject PhysioUserLabel;
	public GameObject PhysioContinueBT;
	public GameObject PanelSelectCOM;
	public GameObject PanelSelectPatient;
	public GameObject PanelSelectSession;
	//public Dropdown UserDropdown;
	public struct PatientSessionInfo
	{
		public string Name;
		public string ID;
		public string Username;
		public string COMport;
		public string SessionDate;
		public string PhysiotherapistUsername;
		public string PhysiotherapistName;
	};

	public static PatientSessionInfo patientsessioninfo;
	string[] idsVectorSub;
	string[] usernamesVectorSub;
	string[] namesVectorSub;

	private string ServerAddress = Register.ServerAddress;
	public string SelectedName = "";

	void Start()
	{
		FillDropdown ();
	}

	public void NewSessionButton()
	{

		Dropdown uDropdown = GameObject.Find ("UsersDropdown").GetComponent<Dropdown> ();
		patientsessioninfo.Name = uDropdown.captionText.text;
		int index1 = Array.IndexOf(namesVectorSub, patientsessioninfo.Name);
		patientsessioninfo.ID = idsVectorSub [index1];
		patientsessioninfo.Username = usernamesVectorSub [index1];
		patientsessioninfo.PhysiotherapistUsername = Login.Username;
		int index2 = Array.IndexOf(usernamesVectorSub, patientsessioninfo.PhysiotherapistUsername);
		patientsessioninfo.PhysiotherapistName = namesVectorSub [index2];

		Debug.LogWarning(patientsessioninfo.Name + " " + patientsessioninfo.ID + " " + patientsessioninfo.Username + " P: " + patientsessioninfo.PhysiotherapistName);

		PanelSelectPatient.SetActive(false);
		PanelSelectCOM.SetActive (true);
		COMSelect comselect = PanelSelectCOM.GetComponent<COMSelect> (); //Ejecuta la corrutina que busca las sesiones anteriores de un paciente
		comselect.StartCoroutine (comselect.GetCOMportsCoroutine());
		PanelSelectSession.SetActive (false);

	}

	public void OldSessionButton()
	{
		Dropdown uDropdown = GameObject.Find ("UsersDropdown").GetComponent<Dropdown> ();
		patientsessioninfo.Name = uDropdown.captionText.text;
		int index1 = Array.IndexOf(namesVectorSub, patientsessioninfo.Name);
		patientsessioninfo.ID = idsVectorSub [index1];
		patientsessioninfo.Username = usernamesVectorSub [index1];
		patientsessioninfo.PhysiotherapistUsername = Login.Username;
		int index2 = Array.IndexOf(usernamesVectorSub, patientsessioninfo.PhysiotherapistUsername);
		patientsessioninfo.PhysiotherapistName = namesVectorSub [index2];

		Debug.LogWarning(patientsessioninfo.Name + " " + patientsessioninfo.ID + " " + patientsessioninfo.Username + " P: " + patientsessioninfo.PhysiotherapistName);

		PanelSelectPatient.SetActive(false);
		PanelSelectCOM.SetActive (false);
		PanelSelectSession.SetActive (true);

		if (patientsessioninfo.Name != null) {
			SessionSelect sessionselect = PanelSelectSession.GetComponent<SessionSelect> (); //Ejecuta la corrutina que busca las sesiones anteriores de un paciente
			sessionselect.StartCoroutine (sessionselect.GetSessionsCoroutine ());
		}

    }

		

	public void FillDropdown()
	{
		StartCoroutine(UsersCoroutine());
	}

	IEnumerator UsersCoroutine()
	{
		WWWForm getwww = new WWWForm();
		getwww.AddField("", "");
		//WWW getuserswww = new WWW(ServerAddress + "/getallusers.php",getwww);
		WWW getuserswww = new WWW(ServerAddress + "/getalluserdata.php",getwww);

		yield return getuserswww; //Espera a que la clase WWW retorne algo
		Debug.LogWarning ("GetRespondGETUSERSW");
		yield return StartCoroutine(GetUsersCoroutine(getuserswww));
	}

	IEnumerator GetUsersCoroutine(WWW getusersw)
	{
		Debug.LogWarning ("START COROUTINE");
			if (getusersw.error == null)
			{
				string data = getusersw.text;
				string[] usersVector = data.Split (',');
				string[] usersVectorSub = new string[usersVector.Length - 1];
				Array.Copy(usersVector, 0, usersVectorSub, 0, usersVector.Length - 1);

				List<string> listids = new List<string>();
				List<string> listusernames = new List<string>();
				List<string> listnames = new List<string>();

				for (int j = 0; j <= usersVectorSub.Length-2; j = j + 3) {
					listids.Add(usersVectorSub[j]);
					listusernames.Add(usersVectorSub[j+1]);
					listnames.Add(usersVectorSub[j+2]);
				}
				idsVectorSub = listids.ToArray ();
				usernamesVectorSub = listusernames.ToArray ();
				namesVectorSub = listnames.ToArray ();
				
				string[] shownamesVectorSub = namesVectorSub;
				Array.Sort(shownamesVectorSub); //Organiza el vector en orden alfabético
				// Array.Copy(source array, start index in source array, destination array, start index in destination array, elements to copy);
				Dropdown uiDropdown = GameObject.Find ("UsersDropdown").GetComponent<Dropdown> ();
				//uiDropdown.options.Clear();
				foreach (string c in shownamesVectorSub) {
					uiDropdown.options.Add (new Dropdown.OptionData () { text = c });
				}
				Debug.LogWarning (data);

			}
			else
			{
			Debug.LogWarning("Error: " + getusersw.error);
			}
			yield return getusersw;

	}

}
