using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NodesCheck : MonoBehaviour {

    public GameObject CheckNodesPanel;
    public GameObject CheckButton;
    public GameObject UpdateCheckButton;
    private bool checknodesPanelActive;
    private int UpdateFrame = 100;
    private static bool updateOrder = false;

    public static int ID_P, ID_LT, ID_LC, ID_LF, ID_RT, ID_RC, ID_RF;
    public static bool Online_P, Online_LT, Online_LC, Online_LF, Online_RT, Online_RC, Online_RF;
    public static int Battery_P, Battery_LT, Battery_LC, Battery_LF, Battery_RT, Battery_RC, Battery_RF;
    public static int CalAcc_P, CalAcc_LT, CalAcc_LC, CalAcc_LF, CalAcc_RT, CalAcc_RC, CalAcc_RF;
    public static int CalGyro_P, CalGyro_LT, CalGyro_LC, CalGyro_LF, CalGyro_RT, CalGyro_RC, CalGyro_RF;
    public static int CalMag_P, CalMag_LT, CalMag_LC, CalMag_LF, CalMag_RT, CalMag_RC, CalMag_RF;

    public GameObject ID_P_txt, ID_LT_txt, ID_LC_txt, ID_LF_txt, ID_RT_txt, ID_RC_txt, ID_RF_txt;
    public Image Online_P_led, Online_LT_led, Online_LC_led, Online_LF_led, Online_RT_led, Online_RC_led, Online_RF_led;
    public GameObject Battery_P_txt, Battery_LT_txt, Battery_LC_txt, Battery_LF_txt, Battery_RT_txt, Battery_RC_txt, Battery_RF_txt;
    public Image CalAcc_P_led, CalAcc_LT_led, CalAcc_LC_led, CalAcc_LF_led, CalAcc_RT_led, CalAcc_RC_led, CalAcc_RF_led;
    public Image CalGyro_P_led, CalGyro_LT_led, CalGyro_LC_led, CalGyro_LF_led, CalGyro_RT_led, CalGyro_RC_led, CalGyro_RF_led;
    public Image CalMag_P_led, CalMag_LT_led, CalMag_LC_led, CalMag_LF_led, CalMag_RT_led, CalMag_RC_led, CalMag_RF_led;

    // Use this for initialization
    void Start () {
        checknodesPanelActive = false;
        CheckNodesPanel.SetActive(false);
        CheckButton.GetComponentInChildren<Text>().text = "Check Nodes \n State";
        UpdateFrame = 0;
	}
		// Update is called once per frame
	void Update () {
        UpdateFrame++;
        if (UpdateFrame == 10)
        {
            if (checknodesPanelActive && PhysioThread.acquiring)
            {
                UpdateCheckButton.SetActive(false);
            }
            else if (checknodesPanelActive && !PhysioThread.acquiring)
            {
                UpdateCheckButton.SetActive(true);
            }

            if (updateOrder)
            {
                UpdateGUI();
                updateOrder = false;
            }

            UpdateFrame = 0;
        }
    }

    public void UpdateGUI()
    {
        ID_P_txt.GetComponentInChildren<Text>().text = ID_P.ToString();
        ID_LT_txt.GetComponentInChildren<Text>().text = ID_LT.ToString();
        ID_LC_txt.GetComponentInChildren<Text>().text = ID_LC.ToString();
        ID_LF_txt.GetComponentInChildren<Text>().text = ID_LF.ToString();
        ID_RT_txt.GetComponentInChildren<Text>().text = ID_RT.ToString();
        ID_RC_txt.GetComponentInChildren<Text>().text = ID_RC.ToString();
        ID_RF_txt.GetComponentInChildren<Text>().text = ID_RF.ToString();

        if (Online_P) { Online_P_led.color = new Color32(16, 226, 36, 255); }
        else { Online_P_led.color = new Color32(253, 13, 13, 255); }
        if (Online_LT) { Online_LT_led.color = new Color32(16, 226, 36, 255); }
        else { Online_LT_led.color = new Color32(253, 13, 13, 255); }
        if (Online_LC) { Online_LC_led.color = new Color32(16, 226, 36, 255); }
        else { Online_LC_led.color = new Color32(253, 13, 13, 255); }
        if (Online_LF) { Online_LF_led.color = new Color32(16, 226, 36, 255); }
        else { Online_LF_led.color = new Color32(253, 13, 13, 255); }
        if (Online_RT) { Online_RT_led.color = new Color32(16, 226, 36, 255); }
        else { Online_RT_led.color = new Color32(253, 13, 13, 255); }
        if (Online_RC) { Online_RC_led.color = new Color32(16, 226, 36, 255); }
        else { Online_RC_led.color = new Color32(253, 13, 13, 255); }
        if (Online_RF) { Online_RF_led.color = new Color32(16, 226, 36, 255); }
        else { Online_RF_led.color = new Color32(253, 13, 13, 255); }

        Battery_P_txt.GetComponentInChildren<Text>().text = (Battery_P * 100 / 7).ToString() + " %";
        Battery_LT_txt.GetComponentInChildren<Text>().text = (Battery_LT * 100 / 7).ToString() + " %";
        Battery_LC_txt.GetComponentInChildren<Text>().text = (Battery_LC * 100 / 7).ToString() + " %";
        Battery_LF_txt.GetComponentInChildren<Text>().text = (Battery_LF * 100 / 7).ToString() + " %";
        Battery_RT_txt.GetComponentInChildren<Text>().text = (Battery_RT * 100 / 7).ToString() + " %";
        Battery_RC_txt.GetComponentInChildren<Text>().text = (Battery_RC * 100 / 7).ToString() + " %";
        Battery_RF_txt.GetComponentInChildren<Text>().text = (Battery_RF * 100 / 7).ToString() + " %";

        if (CalAcc_P == 3 || CalAcc_P == 2) { CalAcc_P_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_P == 1) { CalAcc_P_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_P_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_LT == 3 || CalAcc_LT == 2) { CalAcc_LT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_LT == 1) { CalAcc_LT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_LT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_LC == 3 || CalAcc_LC == 2) { CalAcc_LC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_LC == 1) { CalAcc_LC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_LC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_LF == 3 || CalAcc_LF == 2) { CalAcc_LF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_LF == 1) { CalAcc_LF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_LF_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_RT == 3 || CalAcc_RT == 2) { CalAcc_RT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_RT == 1) { CalAcc_RT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_RT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_RC == 3 || CalAcc_RC == 2) { CalAcc_RC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_RC == 1) { CalAcc_RC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_RC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalAcc_RF == 3 || CalAcc_RF == 2) { CalAcc_RF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalAcc_RF == 1) { CalAcc_RF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalAcc_RF_led.color = new Color32(253, 13, 13, 255); } //Red

        if (CalGyro_P == 3 || CalGyro_P == 2) { CalGyro_P_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_P == 1) { CalGyro_P_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_P_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_LT == 3 || CalGyro_LT == 2) { CalGyro_LT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_LT == 1) { CalGyro_LT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_LT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_LC == 3 || CalGyro_LC == 2) { CalGyro_LC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_LC == 1) { CalGyro_LC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_LC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_LF == 3 || CalGyro_LF == 2) { CalGyro_LF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_LF == 1) { CalGyro_LF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_LF_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_RT == 3 || CalGyro_RT == 2) { CalGyro_RT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_RT == 1) { CalGyro_RT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_RT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_RC == 3 || CalGyro_RC == 2) { CalGyro_RC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_RC == 1) { CalGyro_RC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_RC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalGyro_RF == 3 || CalGyro_RF == 2) { CalGyro_RF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalGyro_RF == 1) { CalGyro_RF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalGyro_RF_led.color = new Color32(253, 13, 13, 255); } //Red

        if (CalMag_P == 3 || CalMag_P == 2) { CalMag_P_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_P == 1) { CalMag_P_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_P_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_LT == 3 || CalMag_LT == 2) { CalMag_LT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_LT == 1) { CalMag_LT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_LT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_LC == 3 || CalMag_LC == 2) { CalMag_LC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_LC == 1) { CalMag_LC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_LC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_LF == 3 || CalMag_LF == 2) { CalMag_LF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_LF == 1) { CalMag_LF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_LF_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_RT == 3 || CalMag_RT == 2) { CalMag_RT_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_RT == 1) { CalMag_RT_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_RT_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_RC == 3 || CalMag_RC == 2) { CalMag_RC_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_RC == 1) { CalMag_RC_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_RC_led.color = new Color32(253, 13, 13, 255); } //Red
        if (CalMag_RF == 3 || CalMag_RF == 2) { CalMag_RF_led.color = new Color32(16, 226, 36, 255); }//Green
        else if (CalMag_RF == 1) { CalMag_RF_led.color = new Color32(255, 237, 0, 255); }//Yellow
        else { CalMag_RF_led.color = new Color32(253, 13, 13, 255); } //Red
    }

    public static void UpdateCheckBUTTON()
    {
        updateOrder = true;
    }

    public void CheckNodesBUTTON()
    {
        if (!checknodesPanelActive)
        {
            checknodesPanelActive = true;
            CheckNodesPanel.SetActive(true);
            CheckButton.GetComponentInChildren<Text>().text = "Hide Nodes \n State";
            if (PhysioThread.acquiring)
            {
                UpdateCheckButton.SetActive(false);
            }
            else
            {
                UpdateCheckButton.SetActive(true);
            }
        }
        else
        {
            checknodesPanelActive = false;
            CheckNodesPanel.SetActive(false);
            CheckButton.GetComponentInChildren<Text>().text = "Check Nodes \n State";
        }

    }

}
