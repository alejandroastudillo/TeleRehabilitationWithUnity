using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public class SessionInfo : MonoBehaviour {

	public GameObject SessionInfoPanel;
	public GameObject SessionDateLabel;
	public GameObject NameLabel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnEnable() {
		//print("Session info panel was enabled");
		UsersList.patientsessioninfo.SessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		SessionDateLabel.GetComponent<Text> ().text = UsersList.patientsessioninfo.SessionDate;
		NameLabel.GetComponent<Text> ().text = UsersList.patientsessioninfo.Name;
	}


}
