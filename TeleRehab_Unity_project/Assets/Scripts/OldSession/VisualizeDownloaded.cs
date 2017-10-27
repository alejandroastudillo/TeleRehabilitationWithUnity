using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class VisualizeDownloaded : MonoBehaviour {

    public GameObject graphPrefab, graphPrefabBottom;
    public WMG_Axis_Graph graphPost, graphBottomPost;
    GameObject graphGOpost, graphGOBottompost;
    public float testInterval;
    WMG_Series s1post, s2post, s3post, s4post;
    public GameObject SessionDateLabel, NameLabel;

    public Dropdown TopPostDropdown1, TopPostDropdown2, BottomPostDropdown1, BottomPostDropdown2;

    private int TopPostDropdown1index, TopPostDropdown2index, BottomPostDropdown1index, BottomPostDropdown2index;

    private List<string> dropdownlist = new List<string>();
    private List<string> variables = new List<string>();

    public static bool postAcquisition;
    private List<Vector2> S1DataList, S2DataList, S3DataList, S4DataList;
    private GameObject StartButton, StopButton, SaveDataButton, NewSessionButton, TopPostPlotPanel, BottomPostPlotPanel;
    private GameObject PlayPostButton, PausePostButton, StopPostButton, CheckNodesButton;
    private bool play_bool, post_plotting, already_played;

    public Transform Pelvis, LThigh, LCalf, LFoot, RThigh, RCalf, RFoot;
    private Quaternion Quat_Pelvis, Quat_LeftThigh, Quat_LeftCalf, Quat_LeftFoot, Quat_RightThigh, Quat_RightCalf, Quat_RightFoot;
    private int frame;
    private float current_time, init_time, pause_time, init_pause_time, delta_time, last_time;
    private int frameCycle = 0, UpdateFrame = 0;

    public Camera FrontalCamera, SagitalCamera, PerspectiveCamera;
    string CameraView = "Perspective";
    public GameObject BackButton;
    string PDMDataAsString = "";

    List<string> PDM_Data = new List<string>();
    private int PDM_DATA_Size;
    private float[] Time_PDM, Quat_P_X, Quat_P_Y, Quat_P_Z, Quat_P_W, Quat_LT_X, Quat_LT_Y, Quat_LT_Z, Quat_LT_W, Quat_LC_X, Quat_LC_Y, Quat_LC_Z, Quat_LC_W, Quat_LF_X, Quat_LF_Y, Quat_LF_Z, Quat_LF_W, Quat_RT_X, Quat_RT_Y, Quat_RT_Z, Quat_RT_W, Quat_RC_X, Quat_RC_Y, Quat_RC_Z, Quat_RC_W, Quat_RF_X, Quat_RF_Y, Quat_RF_Z, Quat_RF_W, LHipAngle_Sag, RHipAngle_Sag, LKneeAngle_Sag, RKneeAngle_Sag, LAnkleAngle_Sag, RAnkleAngle_Sag, AccW_P_X, AccW_P_Y, AccW_P_Z, AccW_LT_X, AccW_LT_Y, AccW_LT_Z, AccW_LC_X, AccW_LC_Y, AccW_LC_Z, AccW_LF_X, AccW_LF_Y, AccW_LF_Z, AccW_RT_X, AccW_RT_Y, AccW_RT_Z, AccW_RC_X, AccW_RC_Y, AccW_RC_Z, AccW_RF_X, AccW_RF_Y, AccW_RF_Z, AngVelocity_P_Sag, AngVelocity_P_Trans, AngVelocity_P_Front, AngVelocity_LT_Sag, AngVelocity_LT_Trans, AngVelocity_LT_Front, AngVelocity_LC_Sag, AngVelocity_LC_Trans, AngVelocity_LC_Front, AngVelocity_LF_Sag, AngVelocity_LF_Trans, AngVelocity_LF_Front, AngVelocity_RT_Sag, AngVelocity_RT_Trans, AngVelocity_RT_Front, AngVelocity_RC_Sag, AngVelocity_RC_Trans, AngVelocity_RC_Front, AngVelocity_RF_Sag, AngVelocity_RF_Trans, AngVelocity_RF_Front;


    // Use this for initialization
    void Start () {

        SagitalCamera.enabled = false;
        PerspectiveCamera.enabled = true;
        FrontalCamera.enabled = false;
        CameraView = "Perspective";

        SessionDateLabel.GetComponent<Text>().text = SessionSelect.SessionDate;
        NameLabel.GetComponent<Text>().text = UsersList.patientsessioninfo.Name;

        TopPostPlotPanel = GameObject.Find("TopPostPlotPanel");
        BottomPostPlotPanel = GameObject.Find("BottomPostPlotPanel");
        PlayPostButton = GameObject.Find("PlayPostButton");
        PausePostButton = GameObject.Find("PausePostButton");
        StopPostButton = GameObject.Find("StopPostButton");

        PopulateLists();
        InitializeDropdowns();

        StartCoroutine(GetSessionDataCoroutine()); //Get PDM Data

        UpdateFrame = 2;
        frame = 0;
        post_plotting = true;
        InitializeGraphics();
    }

    private void InitializeGraphics()
    {
        graphGOpost = Instantiate(graphPrefab) as GameObject;
        graphGOBottompost = Instantiate(graphPrefabBottom) as GameObject;
        graphPost = graphGOpost.GetComponent<WMG_Axis_Graph>();
        graphBottomPost = graphGOBottompost.GetComponent<WMG_Axis_Graph>();
        graphPost.changeSpriteParent(graphGOpost, TopPostPlotPanel);
        graphBottomPost.changeSpriteParent(graphGOBottompost, BottomPostPlotPanel);

        graphPost.xAxis.AxisLabelColor = Color.black;
        graphPost.yAxis.AxisLabelColor = Color.black;
        graphPost.yAxis.AxisNumTicks = 6;
        graphPost.legend.labelColor = Color.black;
        graphPost.paddingLeftRight = new Vector2(40, 30);
        graphBottomPost.xAxis.AxisLabelColor = Color.black;
        graphBottomPost.yAxis.AxisLabelColor = Color.black;
        graphBottomPost.yAxis.AxisNumTicks = 6;
        graphBottomPost.legend.labelColor = Color.black;
        graphBottomPost.paddingLeftRight = new Vector2(40, 30);


        graphPost.xAxis.SetLabelsUsingMaxMin = true;
        graphPost.xAxis.LabelType = WMG_Axis.labelTypes.ticks;
        graphPost.xAxis.numDecimalsAxisLabels = 2;
        graphPost.xAxis.AxisLabelSpaceOffset = 2;
        graphPost.yAxis.numDecimalsAxisLabels = 2;
        graphPost.yAxis.AxisLabelSpaceOffset = 2;

        graphBottomPost.xAxis.SetLabelsUsingMaxMin = true;
        graphBottomPost.xAxis.LabelType = WMG_Axis.labelTypes.ticks;
        graphBottomPost.xAxis.numDecimalsAxisLabels = 2;
        graphBottomPost.xAxis.AxisLabelSpaceOffset = 2;
        graphBottomPost.yAxis.numDecimalsAxisLabels = 2;
        graphBottomPost.yAxis.AxisLabelSpaceOffset = 2;


        if (s1post == null)
        {
            s1post = graphPost.lineSeries[0].GetComponent<WMG_Series>();
        }
        s1post.hidePoints = true;
        s1post.lineColor = Color.red;
        s1post.dataLabelsColor = Color.black;
        s1post.seriesName = "None";

        if (s2post == null)
        {
            s2post = graphPost.lineSeries[1].GetComponent<WMG_Series>();
        }
        s2post.hidePoints = true;
        s2post.lineColor = Color.blue;
        s2post.dataLabelsColor = Color.black;
        s2post.seriesName = "None";

        if (s3post == null)
        {
            s3post = graphBottomPost.lineSeries[0].GetComponent<WMG_Series>();
        }
        s3post.hidePoints = true;
        s3post.lineColor = Color.red;
        s3post.dataLabelsColor = Color.black;
        s3post.seriesName = "None";

        if (s4post == null)
        {
            s4post = graphBottomPost.lineSeries[1].GetComponent<WMG_Series>();
        }
        s4post.hidePoints = true;
        s4post.lineColor = Color.blue;
        s4post.dataLabelsColor = Color.black;
        s4post.seriesName = "None";

        s1post.hideLines = true;
        s2post.hideLines = true;
        s3post.hideLines = true;
        s4post.hideLines = true;
    }


    public void ChangeCameraViewBUTTON()
    {
        if (CameraView.Equals("Perspective"))
        {
            PerspectiveCamera.enabled = false;
            SagitalCamera.enabled = true;
            FrontalCamera.enabled = false;
            CameraView = "Sagital";
        }
        else if (CameraView.Equals("Sagital"))
        {
            SagitalCamera.enabled = false;
            PerspectiveCamera.enabled = false;
            FrontalCamera.enabled = true;
            CameraView = "Frontal";
        }
        else if (CameraView.Equals("Frontal"))
        {
            SagitalCamera.enabled = false;
            PerspectiveCamera.enabled = true;
            FrontalCamera.enabled = false;
            CameraView = "Perspective";
        }
        Debug.Log("Camera Changed");
    }


    private void PopulateLists()
    {
        dropdownlist.Add("None"); variables.Add("None");
        dropdownlist.Add("L Hip Sagital Angle"); variables.Add("LHipAngle");
        dropdownlist.Add("L Knee Sagital Angle"); variables.Add("LKneeAngle");
        dropdownlist.Add("L Ankle Sagital Angle"); variables.Add("LAnkleAngle");
        dropdownlist.Add("R Hip Sagital Angle"); variables.Add("RHipAngle");
        dropdownlist.Add("R Knee Sagital Angle"); variables.Add("RKneeAngle");
        dropdownlist.Add("R Ankle Sagital Angle"); variables.Add("RAnkleAngle");
        dropdownlist.Add("Pelvis X Acc"); variables.Add("accWorldPelvis.x");    //East Acceleration
        dropdownlist.Add("Pelvis Y Acc"); variables.Add("accWorldPelvis.y");    //North Acceleration
        dropdownlist.Add("Pelvis Z Acc"); variables.Add("accWorldPelvis.z");    //Up Acceleration
        dropdownlist.Add("L Thigh X Acc"); variables.Add("accWorldLeftThigh.x");
        dropdownlist.Add("L Thigh Y Acc"); variables.Add("accWorldLeftThigh.y");
        dropdownlist.Add("L Thigh Z Acc"); variables.Add("accWorldLeftThigh.z");
        dropdownlist.Add("L Leg X Acc"); variables.Add("accWorldLeftCalf.x");
        dropdownlist.Add("L Leg Y Acc"); variables.Add("accWorldLeftCalf.y");
        dropdownlist.Add("L Leg Z Acc"); variables.Add("accWorldLeftCalf.z");
        dropdownlist.Add("L Foot X Acc"); variables.Add("accWorldLeftFoot.x");
        dropdownlist.Add("L Foot Y Acc"); variables.Add("accWorldLeftFoot.y");
        dropdownlist.Add("L Foot Z Acc"); variables.Add("accWorldLeftFoot.z");
        dropdownlist.Add("R Thigh X Acc"); variables.Add("accWorldRightThigh.x");
        dropdownlist.Add("R Thigh Y Acc"); variables.Add("accWorldRightThigh.y");
        dropdownlist.Add("R Thigh Z Acc"); variables.Add("accWorldRightThigh.z");
        dropdownlist.Add("R Leg X Acc"); variables.Add("accWorldRightCalf.x");
        dropdownlist.Add("R Leg Y Acc"); variables.Add("accWorldRightCalf.y");
        dropdownlist.Add("R Leg Z Acc"); variables.Add("accWorldRightCalf.z");
        dropdownlist.Add("R Foot X Acc"); variables.Add("accWorldRightFoot.x");
        dropdownlist.Add("R Foot Y Acc"); variables.Add("accWorldRightFoot.y");
        dropdownlist.Add("R Foot Z Acc"); variables.Add("accWorldRightFoot.z");

    }
    private void InitializeDropdowns()
    {
        TopPostDropdown1.onValueChanged.AddListener(delegate {
            TopPostDropdown1ValueChangedHandler(TopPostDropdown1);
        });
        TopPostDropdown1.options.Clear();
        foreach (string t in dropdownlist)
        {
            TopPostDropdown1.options.Add(new Dropdown.OptionData() { text = t });
        }

        TopPostDropdown2.onValueChanged.AddListener(delegate {
            TopPostDropdown2ValueChangedHandler(TopPostDropdown2);
        });
        TopPostDropdown2.options.Clear();
        foreach (string t in dropdownlist)
        {
            TopPostDropdown2.options.Add(new Dropdown.OptionData() { text = t });
        }

        BottomPostDropdown1.onValueChanged.AddListener(delegate {
            BottomPostDropdown1ValueChangedHandler(BottomPostDropdown1);
        });
        BottomPostDropdown1.options.Clear();
        foreach (string t in dropdownlist)
        {
            BottomPostDropdown1.options.Add(new Dropdown.OptionData() { text = t });
        }

        BottomPostDropdown2.onValueChanged.AddListener(delegate {
            BottomPostDropdown2ValueChangedHandler(BottomPostDropdown2);
        });
        BottomPostDropdown2.options.Clear();
        foreach (string t in dropdownlist)
        {
            BottomPostDropdown2.options.Add(new Dropdown.OptionData() { text = t });
        }

    }
    void Destroy()
    {
        TopPostDropdown1.onValueChanged.RemoveAllListeners();
        TopPostDropdown2.onValueChanged.RemoveAllListeners();
        BottomPostDropdown1.onValueChanged.RemoveAllListeners();
        BottomPostDropdown2.onValueChanged.RemoveAllListeners();
    }

    private void TopPostDropdown1ValueChangedHandler(Dropdown target)
    {
        TopPostDropdown1index = target.value;
        Debug.Log("selected: " + TopPostDropdown1index + "   Text: " + dropdownlist[TopPostDropdown1index].ToString());

        Debug.LogWarning("Attempting to change S1");
        s1post.hideLines = false;
        s1post.seriesName = dropdownlist[TopPostDropdown1index].ToString();
        s1post.UseXDistBetweenToSpace = false;
        s1post.pointValues.Clear();

        if (TopPostDropdown2index == 0) { s2post.pointValues.Clear(); }

        if (TopPostDropdown1index == 0)
        {
            s1post.pointValues.Clear();
        }
        else
        {
            S1DataList = FillPlotSeries(Time_PDM, SelectDataSource(target.value));
            StartCoroutine(S1PostPlot());
        }


    }
    private void TopPostDropdown2ValueChangedHandler(Dropdown target)
    {
        TopPostDropdown2index = target.value;
        Debug.Log("selected: " + TopPostDropdown2index + "   Text: " + dropdownlist[TopPostDropdown2index].ToString());

        Debug.LogWarning("Attempting to change S2");
        s2post.hideLines = false;
        s2post.seriesName = dropdownlist[TopPostDropdown2index].ToString();
        s2post.UseXDistBetweenToSpace = false;
        s2post.pointValues.Clear();

        if (TopPostDropdown1index == 0) { s1post.pointValues.Clear(); }
        if (TopPostDropdown2index == 0)
        {
            s2post.pointValues.Clear();
        }
        else
        {
            S2DataList = FillPlotSeries(Time_PDM, SelectDataSource(target.value));
            StartCoroutine(S2PostPlot());
        }

    }
    private void BottomPostDropdown1ValueChangedHandler(Dropdown target)
    {
        BottomPostDropdown1index = target.value;
        Debug.Log("selected: " + BottomPostDropdown1index + "   Text: " + dropdownlist[BottomPostDropdown1index].ToString());

        Debug.LogWarning("Attempting to change S3");
        s3post.hideLines = false;
        s3post.seriesName = dropdownlist[BottomPostDropdown1index].ToString();
        s3post.UseXDistBetweenToSpace = false;
        s3post.pointValues.Clear();

        if (BottomPostDropdown2index == 0) { s4post.pointValues.Clear(); }

        if (BottomPostDropdown1index == 0)
        {
            s3post.pointValues.Clear();
        }
        else
        {
            S3DataList = FillPlotSeries(Time_PDM, SelectDataSource(target.value));
            StartCoroutine(S3PostPlot());
        }
    }
    private void BottomPostDropdown2ValueChangedHandler(Dropdown target)
    {
        BottomPostDropdown2index = target.value;
        Debug.Log("selected: " + BottomPostDropdown2index + "   Text: " + dropdownlist[BottomPostDropdown2index].ToString());
        s4post.hideLines = false;
        s4post.seriesName = dropdownlist[BottomPostDropdown2index].ToString();
        s4post.UseXDistBetweenToSpace = false;
        s4post.pointValues.Clear();

        if (BottomPostDropdown1index == 0) { s3post.pointValues.Clear(); }

        if (BottomPostDropdown2index == 0)
        {
            s4post.pointValues.Clear();
        }
        else
        {
            S4DataList = FillPlotSeries(Time_PDM, SelectDataSource(target.value));
            StartCoroutine(S4PostPlot());
        }
    }

    IEnumerator S1PostPlot()
    {
        graphPost.xAxis.AxisMinValue = 0;
        graphPost.xAxis.AxisMaxValue = Time_PDM[(Time_PDM.Length-1)];
        print("finalTime: " + Time_PDM[(Time_PDM.Length - 1)]);
        s1post.pointValues.SetList(S1DataList);
        yield return null;
    }
    IEnumerator S2PostPlot()
    {
        graphPost.xAxis.AxisMinValue = 0;
        graphPost.xAxis.AxisMaxValue = Time_PDM[(Time_PDM.Length - 1)];
        print("finalTime: " + Time_PDM[(Time_PDM.Length - 1)]);
        s2post.pointValues.SetList(S2DataList);
        yield return null;
    }
    IEnumerator S3PostPlot()
    {
        graphBottomPost.xAxis.AxisMinValue = 0;
        graphBottomPost.xAxis.AxisMaxValue = Time_PDM[(Time_PDM.Length - 1)];
        print("finalTime: " + Time_PDM[(Time_PDM.Length - 1)]);
        s3post.pointValues.SetList(S3DataList);
        yield return null;
    }
    IEnumerator S4PostPlot()
    {
        graphBottomPost.xAxis.AxisMinValue = 0;
        graphBottomPost.xAxis.AxisMaxValue = Time_PDM[(Time_PDM.Length - 1)];
        print("finalTime: " + Time_PDM[(Time_PDM.Length - 1)]);
        s4post.pointValues.SetList(S4DataList);
        yield return null;
    }

    // Update is called once per frame

    void Update()
    {
        if (post_plotting)
        {
            frameCycle++;
            if (frameCycle == UpdateFrame)
            {
                frameCycle = 0;
                if (play_bool)
                {
                    delta_time = Time.time - last_time;
                    current_time = current_time + delta_time - init_time - pause_time;
                    last_time = current_time;
                    if (current_time > Time_PDM[frame])
                    {
                        current_time = Time_PDM[frame] - 0.01f;
                        init_time = current_time;
                    }

                    GetFrameData();
                    MoveBodyPost();
                }
            }
        }
    }


    public float[] SelectDataSource(int indexDS)
    {
        Debug.LogWarning("Getting Data Source");
        float[] datasource = new float[Time_PDM.Length];
        string variableName = variables[indexDS].ToString();

        if (variableName == "LHipAngle") { datasource = LHipAngle_Sag; }
        else if (variableName == "LKneeAngle") { datasource = LKneeAngle_Sag; }
        else if (variableName == "LAnkleAngle") { datasource = LAnkleAngle_Sag; }
        else if (variableName == "RHipAngle") { datasource = RHipAngle_Sag; }
        else if (variableName == "RKneeAngle") { datasource = RKneeAngle_Sag; }
        else if (variableName == "RAnkleAngle") { datasource = RAnkleAngle_Sag; }
        else if (variableName == "accWorldPelvis.x") { datasource = AccW_P_X; }
        else if (variableName == "accWorldPelvis.y") { datasource = AccW_P_Y; }
        else if (variableName == "accWorldPelvis.z") { datasource = AccW_P_Z; }
        else if (variableName == "accWorldLeftThigh.x") { datasource = AccW_LT_X; }
        else if (variableName == "accWorldLeftThigh.y") { datasource = AccW_LT_Y; }
        else if (variableName == "accWorldLeftThigh.z") { datasource = AccW_LT_Z; }
        else if (variableName == "accWorldLeftCalf.x") { datasource = AccW_LC_X; }
        else if (variableName == "accWorldLeftCalf.y") { datasource = AccW_LC_Y; }
        else if (variableName == "accWorldLeftCalf.z") { datasource = AccW_LC_Z; }
        else if (variableName == "accWorldLeftFoot.x") { datasource = AccW_LF_X; }
        else if (variableName == "accWorldLeftFoot.y") { datasource = AccW_LF_Y; }
        else if (variableName == "accWorldLeftFoot.z") { datasource = AccW_LF_Z; }
        else if (variableName == "accWorldRightThigh.x") { datasource = AccW_RT_X; }
        else if (variableName == "accWorldRightThigh.y") { datasource = AccW_RT_Y; }
        else if (variableName == "accWorldRightThigh.z") { datasource = AccW_RT_Z; }
        else if (variableName == "accWorldRightCalf.x") { datasource = AccW_RC_X; }
        else if (variableName == "accWorldRightCalf.y") { datasource = AccW_RC_Y; }
        else if (variableName == "accWorldRightCalf.z") { datasource = AccW_RC_Z; }
        else if (variableName == "accWorldRightFoot.x") { datasource = AccW_RF_X; }
        else if (variableName == "accWorldRightFoot.y") { datasource = AccW_RF_Y; }
        else if (variableName == "accWorldRightFoot.z") { datasource = AccW_RF_Z; }
        Debug.LogWarning("Data Source Gotten");
        return datasource;
    }
    public List<Vector2> FillPlotSeries(float[] time, float[] data)
    {
        Debug.LogWarning("Filling Plot Series");
        List<Vector2> resultList = new List<Vector2>();

        for (int i = 0; i < data.Length; i++)
        {
            resultList.Add(new Vector2(time[i], data[i]));
        }
        Debug.LogWarning("Plot series Filled");
        return resultList;
    }
    public void OrganizeData()
    {
        PDM_DATA_Size = PDM_Data.Count;
        InitializeLists();

        Time_PDM = GetColumns(PDM_Data, 0);
        LHipAngle_Sag = GetColumns(PDM_Data, 1);
        RHipAngle_Sag = GetColumns(PDM_Data, 2);
        LKneeAngle_Sag = GetColumns(PDM_Data, 3);
        RKneeAngle_Sag = GetColumns(PDM_Data, 4);
        LAnkleAngle_Sag = GetColumns(PDM_Data, 5);
        RAnkleAngle_Sag = GetColumns(PDM_Data, 6);
        Quat_P_X = GetColumns(PDM_Data, 7);
        Quat_P_Y = GetColumns(PDM_Data, 8);
        Quat_P_Z = GetColumns(PDM_Data, 9);
        Quat_P_W = GetColumns(PDM_Data, 10);
        Quat_LT_X = GetColumns(PDM_Data, 11);
        Quat_LT_Y = GetColumns(PDM_Data, 12);
        Quat_LT_Z = GetColumns(PDM_Data, 13);
        Quat_LT_W = GetColumns(PDM_Data, 14);
        Quat_LC_X = GetColumns(PDM_Data, 15);
        Quat_LC_Y = GetColumns(PDM_Data, 16);
        Quat_LC_Z = GetColumns(PDM_Data, 17);
        Quat_LC_W = GetColumns(PDM_Data, 18);
        Quat_LF_X = GetColumns(PDM_Data, 19);
        Quat_LF_Y = GetColumns(PDM_Data, 20);
        Quat_LF_Z = GetColumns(PDM_Data, 21);
        Quat_LF_W = GetColumns(PDM_Data, 22);
        Quat_RT_X = GetColumns(PDM_Data, 23);
        Quat_RT_Y = GetColumns(PDM_Data, 24);
        Quat_RT_Z = GetColumns(PDM_Data, 25);
        Quat_RT_W = GetColumns(PDM_Data, 26);
        Quat_RC_X = GetColumns(PDM_Data, 27);
        Quat_RC_Y = GetColumns(PDM_Data, 28);
        Quat_RC_Z = GetColumns(PDM_Data, 29);
        Quat_RC_W = GetColumns(PDM_Data, 30);
        Quat_RF_X = GetColumns(PDM_Data, 31);
        Quat_RF_Y = GetColumns(PDM_Data, 32);
        Quat_RF_Z = GetColumns(PDM_Data, 33);
        Quat_RF_W = GetColumns(PDM_Data, 34);
        AccW_P_X = GetColumns(PDM_Data, 35);
        AccW_P_Y = GetColumns(PDM_Data, 36);
        AccW_P_Z = GetColumns(PDM_Data, 37);
        AccW_LT_X = GetColumns(PDM_Data, 38);
        AccW_LT_Y = GetColumns(PDM_Data, 39);
        AccW_LT_Z = GetColumns(PDM_Data, 40);
        AccW_LC_X = GetColumns(PDM_Data, 41);
        AccW_LC_Y = GetColumns(PDM_Data, 42);
        AccW_LC_Z = GetColumns(PDM_Data, 43);
        AccW_LF_X = GetColumns(PDM_Data, 44);
        AccW_LF_Y = GetColumns(PDM_Data, 45);
        AccW_LF_Z = GetColumns(PDM_Data, 46);
        AccW_RT_X = GetColumns(PDM_Data, 47);
        AccW_RT_Y = GetColumns(PDM_Data, 48);
        AccW_RT_Z = GetColumns(PDM_Data, 49);
        AccW_RC_X = GetColumns(PDM_Data, 50);
        AccW_RC_Y = GetColumns(PDM_Data, 51);
        AccW_RC_Z = GetColumns(PDM_Data, 52);
        AccW_RF_X = GetColumns(PDM_Data, 53);
        AccW_RF_Y = GetColumns(PDM_Data, 54);
        AccW_RF_Z = GetColumns(PDM_Data, 55);
        AngVelocity_P_Sag = GetColumns(PDM_Data, 56);
        AngVelocity_P_Trans = GetColumns(PDM_Data, 57);
        AngVelocity_P_Front = GetColumns(PDM_Data, 58);
        AngVelocity_LT_Sag = GetColumns(PDM_Data, 59);
        AngVelocity_LT_Trans = GetColumns(PDM_Data, 60);
        AngVelocity_LT_Front = GetColumns(PDM_Data, 61);
        AngVelocity_LC_Sag = GetColumns(PDM_Data, 62);
        AngVelocity_LC_Trans = GetColumns(PDM_Data, 63);
        AngVelocity_LC_Front = GetColumns(PDM_Data, 64);
        AngVelocity_LF_Sag = GetColumns(PDM_Data, 65);
        AngVelocity_LF_Trans = GetColumns(PDM_Data, 66);
        AngVelocity_LF_Front = GetColumns(PDM_Data, 67);
        AngVelocity_RT_Sag = GetColumns(PDM_Data, 68);
        AngVelocity_RT_Trans = GetColumns(PDM_Data, 69);
        AngVelocity_RT_Front = GetColumns(PDM_Data, 70);
        AngVelocity_RC_Sag = GetColumns(PDM_Data, 71);
        AngVelocity_RC_Trans = GetColumns(PDM_Data, 72);
        AngVelocity_RC_Front = GetColumns(PDM_Data, 73);
        AngVelocity_RF_Sag = GetColumns(PDM_Data, 74);
        AngVelocity_RF_Trans = GetColumns(PDM_Data, 75);
        AngVelocity_RF_Front = GetColumns(PDM_Data, 76);
    }
    private float[] GetColumns(List<string> ListData, int index)
    {
        int j = 0;
        float[] resultColumn = new float[PDM_DATA_Size - 2];
        string[] fields;

        for (int i = 2; i < PDM_DATA_Size; i++)
        {
            j = i - 2;
            fields = ListData[i].Split(';');
            resultColumn[j] = float.Parse(fields[index], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

        }
        return resultColumn;
    }
    void InitializeLists()
    {
        Debug.LogError("PDMDATASIZE: " + PDM_DATA_Size);
        Time_PDM = Quat_P_X = Quat_P_Y = Quat_P_Z = Quat_P_W = Quat_LT_X = Quat_LT_Y = Quat_LT_Z = Quat_LT_W = Quat_LC_X = Quat_LC_Y = Quat_LC_Z = Quat_LC_W = Quat_LF_X = Quat_LF_Y = Quat_LF_Z = Quat_LF_W = Quat_RT_X = Quat_RT_Y = Quat_RT_Z = Quat_RT_W = Quat_RC_X = Quat_RC_Y = Quat_RC_Z = Quat_RC_W = Quat_RF_X = Quat_RF_Y = Quat_RF_Z = Quat_RF_W = LHipAngle_Sag = RHipAngle_Sag = LKneeAngle_Sag = RKneeAngle_Sag = LAnkleAngle_Sag = RAnkleAngle_Sag = AccW_P_X = AccW_P_Y = AccW_P_Z = AccW_LT_X = AccW_LT_Y = AccW_LT_Z = AccW_LC_X = AccW_LC_Y = AccW_LC_Z = AccW_LF_X = AccW_LF_Y = AccW_LF_Z = AccW_RT_X = AccW_RT_Y = AccW_RT_Z = AccW_RC_X = AccW_RC_Y = AccW_RC_Z = AccW_RF_X = AccW_RF_Y = AccW_RF_Z = AngVelocity_P_Sag = AngVelocity_P_Trans = AngVelocity_P_Front = AngVelocity_LT_Sag = AngVelocity_LT_Trans = AngVelocity_LT_Front = AngVelocity_LC_Sag = AngVelocity_LC_Trans = AngVelocity_LC_Front = AngVelocity_LF_Sag = AngVelocity_LF_Trans = AngVelocity_LF_Front = AngVelocity_RT_Sag = AngVelocity_RT_Trans = AngVelocity_RT_Front = AngVelocity_RC_Sag = AngVelocity_RC_Trans = AngVelocity_RC_Front = AngVelocity_RF_Sag = AngVelocity_RF_Trans = AngVelocity_RF_Front = new float[PDM_DATA_Size - 2];
    }
    public void BackBUTTON()
    {
        SceneManager.LoadScene("LogIn", LoadSceneMode.Single);  //LoadSceneMode.Additive
    }
    IEnumerator GetSessionDataCoroutine()
    {
        Debug.LogWarning("Session Data--> ID: " + SessionSelect.SessionUserID + "  username: " + UsersList.patientsessioninfo.Username + "   sessiondate: " + SessionSelect.SessionDate);
        yield return StartCoroutine(DownloadDataCoroutine());
        

    }
    IEnumerator DownloadDataCoroutine()
    {
        WWWForm DownloadFormWWW = new WWWForm();
        DownloadFormWWW.AddField("ID", SessionSelect.SessionUserID);
        DownloadFormWWW.AddField("SessionDate", SessionSelect.SessionDate);
        WWW DownloadValidW = new WWW(Register.ServerAddress + "/downloadSessionData.php", DownloadFormWWW);
        yield return DownloadValidW;
        yield return StartCoroutine(DownloadValidationCoroutine(DownloadValidW));

        byte[] PDMDataAs64 = Convert.FromBase64String(PDMDataAsString); //Convert Downloaded BLOB to Byte arraw again

        PDM_Data = new List<string>();
        for (int i = 0; i < PDMDataAs64.Length; i++)
        {
            int end = i;
            while (PDMDataAs64[end] != 0) // Scan for zero byte
                end++;
            var length = end - i;
            var word = new byte[length];
            Array.Copy(PDMDataAs64, i, word, 0, length);
            PDM_Data.Add(Encoding.ASCII.GetString(word));
            i += length;
        }
        PDM_DATA_Size = PDM_Data.Count;
        OrganizeData();
        yield return PDM_Data;
    }
    IEnumerator DownloadValidationCoroutine(WWW w) //Comprueba si la respuesta del servidor fue UPLOAD-SUCCESSFUL
    {
        if (w.error == null)
        {
            PDMDataAsString = w.text;
            Debug.LogWarning(PDMDataAsString);
            Debug.LogWarning("BytesDownloaded: "+w.bytesDownloaded);          
        }
        else
        {
            Debug.LogWarning("Error: " + w.error);
        }
        yield return PDMDataAsString;
    }


    public void PlayVideoBUTTON()
    {
        if (already_played)
        {
            play_bool = true;
            PausePostButton.SetActive(true);
            PlayPostButton.SetActive(false);

            pause_time = Time.time - init_pause_time;
            //Debug.LogError("pause_time: " + pause_time + "current_time: "+current_time + "  current-pause: "+(current_time-pause_time));
        }
        else
        {
            play_bool = true;

            init_time = Time.time;

            PausePostButton.SetActive(true);
            PlayPostButton.SetActive(false);
            already_played = true;
            pause_time = 0;
        }
    }
    public void PauseVideoBUTTON()
    {
        play_bool = false;
        PausePostButton.SetActive(false);
        PlayPostButton.SetActive(true);
        init_pause_time = Time.time;
        pause_time = 0;

    }
    public void StopVideoBUTTON()
    {
        play_bool = false;
        frame = 0;
        init_time = Time.time;
        PausePostButton.SetActive(false);
        PlayPostButton.SetActive(true);
        already_played = false;
    }

    private void GetFrameData()
    {

        //Debug.LogWarning("Frame_  " + frame);

        if (frame < PDM_DATA_Size - 3)
        {
            if (Math.Abs(Time_PDM[frame] - current_time) < 0.1)
            {
                frame++;
            }
            //Debug.LogError("time:  "+PhysioThread.Time_PDM[frame] + "   currenttime: "+current_time);
            Quat_Pelvis = new Quaternion(Quat_P_X[frame], Quat_P_Y[frame], Quat_P_Z[frame], Quat_P_W[frame]);
            Quat_LeftThigh = new Quaternion(Quat_LT_X[frame], Quat_LT_Y[frame], Quat_LT_Z[frame], Quat_LT_W[frame]);
            Quat_LeftCalf = new Quaternion(Quat_LC_X[frame], Quat_LC_Y[frame], Quat_LC_Z[frame], Quat_LC_W[frame]);
            Quat_LeftFoot = new Quaternion(Quat_LF_X[frame], Quat_LF_Y[frame], Quat_LF_Z[frame], Quat_LF_W[frame]);
            Quat_RightThigh = new Quaternion(Quat_RT_X[frame], Quat_RT_Y[frame], Quat_RT_Z[frame], Quat_RT_W[frame]);
            Quat_RightCalf = new Quaternion(Quat_RC_X[frame], Quat_RC_Y[frame], Quat_RC_Z[frame], Quat_RC_W[frame]);
            Quat_RightFoot = new Quaternion(Quat_RF_X[frame], Quat_RF_Y[frame], Quat_RF_Z[frame], Quat_RF_W[frame]);

        }
        else
        {
            StopVideoBUTTON();
            PausePostButton.SetActive(false);
            PlayPostButton.SetActive(true);
        }

    }
    private void MoveBodyPost()
    {
        //Debug.LogWarning("Attempting to move the video");
        Pelvis.rotation = Quat_Pelvis;
        LThigh.rotation = Quat_LeftThigh;
        LCalf.rotation = Quat_LeftCalf;
        LFoot.rotation = Quat_LeftFoot;
        RThigh.rotation = Quat_RightThigh;
        RCalf.rotation = Quat_RightCalf;
        RFoot.rotation = Quat_RightFoot;
    }
}
