using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class PlotData : MonoBehaviour {
    public GameObject graphPrefab, graphPrefabBottom;
    public WMG_Axis_Graph graph, graphBottom;
    public WMG_Axis_Graph graphPost, graphBottomPost;
    public GameObject HumanControl;
    GameObject graphGO, graphGOBottom;
    GameObject graphGOpost, graphGOBottompost;
    public float testInterval;
    WMG_Series s1, s2, s3, s4;
    WMG_Series s1post, s2post, s3post, s4post;
    WMG_Data_Source ds1, ds2, ds3, ds4;

    public Dropdown TopDropdown1, TopDropdown2, BottomDropdown1, BottomDropdown2;
    public Dropdown TopPostDropdown1, TopPostDropdown2, BottomPostDropdown1, BottomPostDropdown2;

    private int TopDropdown1index, TopDropdown2index, BottomDropdown1index, BottomDropdown2index;
    private int TopPostDropdown1index, TopPostDropdown2index, BottomPostDropdown1index, BottomPostDropdown2index;

    private List<string> dropdownlist = new List<string>();
    private List<string> variables = new List<string>();

    public static bool postAcquisition;
    private List<Vector2> S1DataList, S2DataList, S3DataList, S4DataList;
    private GameObject TopPlotPanel, BottomPlotPanel, StartButton, StopButton, SaveDataButton, NewSessionButton, SessionDateLabel, TopPostPlotPanel, BottomPostPlotPanel;
    private GameObject PlayPostButton, PausePostButton, StopPostButton, CheckNodesButton;
    private bool play_bool, post_plotting, already_played;

    public Transform Pelvis, LThigh, LCalf, LFoot, RThigh, RCalf, RFoot;
    private Quaternion Quat_Pelvis, Quat_LeftThigh, Quat_LeftCalf, Quat_LeftFoot, Quat_RightThigh, Quat_RightCalf, Quat_RightFoot;
    private int frame;
    private float current_time, init_time, pause_time, init_pause_time, delta_time, last_time;
    private int frameCycle = 0, UpdateFrame = 0;

    void Start()
    {
        TopPlotPanel = GameObject.Find("TopPlotPanel");
        BottomPlotPanel = GameObject.Find("BottomPlotPanel");
        TopPostPlotPanel = GameObject.Find("TopPostPlotPanel");
        TopPostPlotPanel.SetActive(false);
        BottomPostPlotPanel = GameObject.Find("BottomPostPlotPanel");
        BottomPostPlotPanel.SetActive(false);
        StartButton = GameObject.Find("StartButton");
        StopButton = GameObject.Find("StopButton");
        SaveDataButton = GameObject.Find("SaveButton");
        NewSessionButton = GameObject.Find("NewSessionButton");
        SessionDateLabel = GameObject.Find("SessionTimeLabel");
        NewSessionButton.SetActive(false);

        PlayPostButton = GameObject.Find("PlayPostButton");
        PausePostButton = GameObject.Find("PausePostButton");
        StopPostButton = GameObject.Find("StopPostButton");
        CheckNodesButton = GameObject.Find("CheckButton");
        PlayPostButton.SetActive(false);
        PausePostButton.SetActive(false);
        StopPostButton.SetActive(false);

        graphGO = Instantiate(graphPrefab) as GameObject;
        graphGOBottom = Instantiate(graphPrefabBottom) as GameObject;

        graph = graphGO.GetComponent<WMG_Axis_Graph>();
        graphBottom = graphGOBottom.GetComponent<WMG_Axis_Graph>();

        graph.changeSpriteParent(graphGO, TopPlotPanel);
        graphBottom.changeSpriteParent(graphGOBottom, BottomPlotPanel);

        graph.lineSeries[0].GetComponent<WMG_Series>().hideLines = true;
        graph.lineSeries[0].GetComponent<WMG_Series>().hidePoints = true;
        graph.lineSeries[1].GetComponent<WMG_Series>().hideLines = true;
        graph.lineSeries[1].GetComponent<WMG_Series>().hidePoints = true;
        graphBottom.lineSeries[0].GetComponent<WMG_Series>().hideLines = true;
        graphBottom.lineSeries[0].GetComponent<WMG_Series>().hidePoints = true;
        graphBottom.lineSeries[1].GetComponent<WMG_Series>().hideLines = true;
        graphBottom.lineSeries[1].GetComponent<WMG_Series>().hidePoints = true;

        PopulateLists();
        InitializeDropdowns();

        play_bool = false;
        post_plotting = false;
        already_played = false;
        UpdateFrame = PhysioThread.BodyMoveUpdateFrame;
        current_time = 0;
        delta_time = 0;
    }
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
                    //Debug.LogWarning("PLay_bool yeah");
                    //current_time = Time.time - init_time - pause_time;
                    delta_time = Time.time - last_time;
                    current_time = current_time + delta_time - init_time - pause_time;
                    last_time = current_time;
                    if (current_time > PhysioThread.Time_PDM[frame]) {
                        //Debug.LogError(";;;;;;Current_time: "+ current_time + "  Time_PDM:  "+ PhysioThread.Time_PDM[frame]);
                        current_time = PhysioThread.Time_PDM[frame] - 0.01f;
                        init_time = current_time;
                    }
                    
                    GetFrameData();
                    MoveBodyPost();
                }
            }
        }
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
        TopDropdown1.onValueChanged.AddListener(delegate {
            TopDropdown1ValueChangedHandler(TopDropdown1);
        });
        TopDropdown1.options.Clear();
        foreach (string t in dropdownlist)
        {
            TopDropdown1.options.Add(new Dropdown.OptionData() { text = t });
        }

        TopDropdown2.onValueChanged.AddListener(delegate {
            TopDropdown2ValueChangedHandler(TopDropdown2);
        });
        TopDropdown2.options.Clear();
        foreach (string t in dropdownlist)
        {
            TopDropdown2.options.Add(new Dropdown.OptionData() { text = t });
        }

        BottomDropdown1.onValueChanged.AddListener(delegate {
            BottomDropdown1ValueChangedHandler(BottomDropdown1);
        });
        BottomDropdown1.options.Clear();
        foreach (string t in dropdownlist)
        {
            BottomDropdown1.options.Add(new Dropdown.OptionData() { text = t });
        }

        BottomDropdown2.onValueChanged.AddListener(delegate {
            BottomDropdown2ValueChangedHandler(BottomDropdown2);
        });
        BottomDropdown2.options.Clear();
        foreach (string t in dropdownlist)
        {
            BottomDropdown2.options.Add(new Dropdown.OptionData() { text = t });
        }


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
        TopDropdown1.onValueChanged.RemoveAllListeners();
        TopDropdown2.onValueChanged.RemoveAllListeners();
        BottomDropdown1.onValueChanged.RemoveAllListeners();
        BottomDropdown2.onValueChanged.RemoveAllListeners();
    }

    private void TopDropdown1ValueChangedHandler(Dropdown target)
    {
        TopDropdown1index = target.value;
        Debug.Log("selected: " + TopDropdown1index + "   Text: " + dropdownlist[TopDropdown1index].ToString());

        if (ds1 != null && s1 != null)
        {
            ds1.setVariableName(variables[TopDropdown1index].ToString());
            s1.seriesName = dropdownlist[TopDropdown1index].ToString();
        }
    }
    private void TopDropdown2ValueChangedHandler(Dropdown target)
    {
        TopDropdown2index = target.value;
        Debug.Log("selected: " + TopDropdown2index + "   Text: " + dropdownlist[TopDropdown2index].ToString());

        if (ds2 != null && s2 != null)
        {
            ds2.setVariableName(variables[TopDropdown2index].ToString());
            s2.seriesName = dropdownlist[TopDropdown2index].ToString();
        }
    }
    private void BottomDropdown1ValueChangedHandler(Dropdown target)
    {
        BottomDropdown1index = target.value;
        Debug.Log("selected: " + BottomDropdown1index + "   Text: " + dropdownlist[BottomDropdown1index].ToString());

        if (ds3 != null && s3 != null)
        {
            ds3.setVariableName(variables[BottomDropdown1index].ToString());
            s3.seriesName = dropdownlist[BottomDropdown1index].ToString();
        }
    }
    private void BottomDropdown2ValueChangedHandler(Dropdown target)
    {
        BottomDropdown2index = target.value;
        Debug.Log("selected: " + BottomDropdown2index + "   Text: " + dropdownlist[BottomDropdown2index].ToString());

        if (ds4 != null && s4 != null)
        {
            ds4.setVariableName(variables[BottomDropdown2index].ToString());
            s4.seriesName = dropdownlist[BottomDropdown2index].ToString();
        }
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

        if (TopPostDropdown1index == 0) {
            s1post.pointValues.Clear();
        }
        else
        {
            S1DataList = FillPlotSeries(PhysioThread.Time_PDM, SelectDataSource(target.value));
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
            S2DataList = FillPlotSeries(PhysioThread.Time_PDM, SelectDataSource(target.value));
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
            S3DataList = FillPlotSeries(PhysioThread.Time_PDM, SelectDataSource(target.value));
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
            S4DataList = FillPlotSeries(PhysioThread.Time_PDM, SelectDataSource(target.value));
            StartCoroutine(S4PostPlot());
        }
    }

    IEnumerator S1PostPlot()
    {
        graphPost.xAxis.AxisMinValue = 0;
        graphPost.xAxis.AxisMaxValue = PhysioThread.actualtime;
        print("finalTime: " + PhysioThread.actualtime);
        s1post.pointValues.SetList(S1DataList);
        yield return null;
    }
    IEnumerator S2PostPlot()
    {
        graphPost.xAxis.AxisMinValue = 0;
        graphPost.xAxis.AxisMaxValue = PhysioThread.actualtime;
        print("finalTime: " + PhysioThread.actualtime);
        s2post.pointValues.SetList(S2DataList);
        yield return null;
    }
    IEnumerator S3PostPlot()
    {
        graphBottomPost.xAxis.AxisMinValue = 0;
        graphBottomPost.xAxis.AxisMaxValue = PhysioThread.actualtime;
        print("finalTime: " + PhysioThread.actualtime);
        s3post.pointValues.SetList(S3DataList);
        yield return null;
    }
    IEnumerator S4PostPlot()
    {
        graphBottomPost.xAxis.AxisMinValue = 0;
        graphBottomPost.xAxis.AxisMaxValue = PhysioThread.actualtime;
        print("finalTime: " + PhysioThread.actualtime);
        s4post.pointValues.SetList(S4DataList);
        yield return null;
    }
    public void StartPlotting()
    {
        Destroy(graphGO);
        Destroy(graphGOBottom);
        graphGO = Instantiate(graphPrefab) as GameObject;
        graphGOBottom = Instantiate(graphPrefabBottom) as GameObject;

        graph = graphGO.GetComponent<WMG_Axis_Graph>();
        graphBottom = graphGOBottom.GetComponent<WMG_Axis_Graph>();

        graph.changeSpriteParent(graphGO, TopPlotPanel);
        graphBottom.changeSpriteParent(graphGOBottom, BottomPlotPanel);



        StartCoroutine(realTimePlot());
    }

    public void StopPlotting()
    {
        s1.StopRealTimeUpdate();
        s2.StopRealTimeUpdate();

        s3.StopRealTimeUpdate();
        s4.StopRealTimeUpdate();

        StartButton.SetActive(false);
        StopButton.SetActive(false);
        NewSessionButton.SetActive(true);
        TopPlotPanel.SetActive(false);
        BottomPlotPanel.SetActive(false);
        TopPostPlotPanel.SetActive(true);
        BottomPostPlotPanel.SetActive(true);
        PlayPostButton.SetActive(true);
        PausePostButton.SetActive(false);
        StopPostButton.SetActive(true);
        CheckNodesButton.SetActive(false);
        PostPlotResults();
        post_plotting = true;
        frame = 0;
    }

    IEnumerator realTimePlot()
    {
        ds1 = graph.lineSeries[0].AddComponent<WMG_Data_Source>();
        ds1.dataSourceType = WMG_Data_Source.WMG_DataSourceTypes.Single_Object_Single_Variable;
        s1 = graph.lineSeries[0].GetComponent<WMG_Series>();
        s1.hidePoints = true;
        s1.hideLines = false;
        s1.lineColor = Color.red;
        s1.dataLabelsColor = Color.black;
        ds1.setDataProvider<Component>(HumanControl.GetComponent("PhysioThread"));
        ds1.setVariableName(variables[TopDropdown1index].ToString());
        s1.realTimeDataSource = ds1;
        s1.seriesName = dropdownlist[TopDropdown1index].ToString();
        s1.UseXDistBetweenToSpace = false;

        ds2 = graph.lineSeries[1].AddComponent<WMG_Data_Source>();
        ds2.dataSourceType = WMG_Data_Source.WMG_DataSourceTypes.Single_Object_Single_Variable;
        s2 = graph.lineSeries[1].GetComponent<WMG_Series>();
        s2.hidePoints = true;
        s2.hideLines = false;
        s2.lineColor = Color.blue;
        s2.dataLabelsColor = Color.black;
        ds2.setDataProvider<Component>(HumanControl.GetComponent("PhysioThread"));
        ds2.setVariableName(variables[TopDropdown2index].ToString());
        s2.realTimeDataSource = ds2;
        s2.seriesName = dropdownlist[TopDropdown2index].ToString();
        s2.UseXDistBetweenToSpace = false;

        ds3 = graphBottom.lineSeries[0].AddComponent<WMG_Data_Source>();
        ds3.dataSourceType = WMG_Data_Source.WMG_DataSourceTypes.Single_Object_Single_Variable;
        s3 = graphBottom.lineSeries[0].GetComponent<WMG_Series>();
        s3.hidePoints = true;
        s3.hideLines = false;
        s3.lineColor = Color.red;
        s3.dataLabelsColor = Color.black;
        ds3.setDataProvider<Component>(HumanControl.GetComponent("PhysioThread"));
        ds3.setVariableName(variables[BottomDropdown1index].ToString());
        s3.realTimeDataSource = ds3;
        s3.seriesName = dropdownlist[BottomDropdown1index].ToString();
        s3.UseXDistBetweenToSpace = false;

        ds4 = graphBottom.lineSeries[1].AddComponent<WMG_Data_Source>();
        ds4.dataSourceType = WMG_Data_Source.WMG_DataSourceTypes.Single_Object_Single_Variable;
        s4 = graphBottom.lineSeries[1].GetComponent<WMG_Series>();
        s4.hidePoints = true;
        s4.hideLines = false;
        s4.lineColor = Color.blue;
        s4.dataLabelsColor = Color.black;
        ds4.setDataProvider<Component>(HumanControl.GetComponent("PhysioThread"));
        ds4.setVariableName(variables[BottomDropdown2index].ToString());
        s4.realTimeDataSource = ds4;
        s4.seriesName = dropdownlist[BottomDropdown2index].ToString();
        s4.UseXDistBetweenToSpace = false;

        graph.xAxis.AxisMaxValue = 0;
        graph.xAxis.AxisMaxValue = 3;
        graph.yAxis.AxisMinValue = -90;
        graph.yAxis.AxisMaxValue = 90;
        graph.xAxis.SetLabelsUsingMaxMin = true;
        graph.xAxis.LabelType = WMG_Axis.labelTypes.ticks;
        graph.xAxis.hideLabels = true;
        graph.xAxis.AxisLabelColor = Color.black;
        graph.yAxis.numDecimalsAxisLabels = 2;
        graph.yAxis.AxisLabelColor = Color.black;
        graph.yAxis.AxisNumTicks = 6;
        graph.legend.labelColor = Color.black;
        graph.paddingLeftRight = new Vector2(40, 30);

        graphBottom.xAxis.AxisMaxValue = 0;
        graphBottom.xAxis.AxisMaxValue = 3;
        graphBottom.yAxis.AxisMinValue = -90;
        graphBottom.yAxis.AxisMaxValue = 90;
        graphBottom.xAxis.SetLabelsUsingMaxMin = true;
        graphBottom.xAxis.LabelType = WMG_Axis.labelTypes.ticks;
        graphBottom.xAxis.hideLabels = true;
        graphBottom.xAxis.AxisLabelColor = Color.black;
        graphBottom.yAxis.numDecimalsAxisLabels = 2;
        graphBottom.yAxis.AxisLabelColor = Color.black;
        graphBottom.yAxis.AxisNumTicks = 6;
        graphBottom.legend.labelColor = Color.black;
        graphBottom.paddingLeftRight = new Vector2(40, 30);

        s1.StartRealTimeUpdate();
        s2.StartRealTimeUpdate();
        s3.StartRealTimeUpdate();
        s4.StartRealTimeUpdate();

        yield return null;

    }

    public void NewSessionBUTTON()
    {
        TopPostPlotPanel.SetActive(false);
        BottomPostPlotPanel.SetActive(false);
        TopPlotPanel.SetActive(true);
        BottomPlotPanel.SetActive(true);
        StartButton.SetActive(true);
        StopButton.SetActive(true);
        SaveDataButton.SetActive(true);
        NewSessionButton.SetActive(false);
        UsersList.patientsessioninfo.SessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SessionDateLabel.GetComponent<Text>().text = UsersList.patientsessioninfo.SessionDate;
        PlayPostButton.SetActive(false);
        PausePostButton.SetActive(false);
        StopPostButton.SetActive(false);
        CheckNodesButton.SetActive(true);

        Destroy(graphGOpost);
        Destroy(graphGOBottompost);
        graphGO = Instantiate(graphPrefab) as GameObject;
        graphGOBottom = Instantiate(graphPrefabBottom) as GameObject;
        graph = graphGO.GetComponent<WMG_Axis_Graph>();
        graphBottom = graphGOBottom.GetComponent<WMG_Axis_Graph>();
        graph.changeSpriteParent(graphGO, TopPlotPanel);
        graphBottom.changeSpriteParent(graphGOBottom, BottomPlotPanel);
        post_plotting = false;
    }
    public void PostPlotResults()
    {
        Destroy(graphGO);
        Destroy(graphGOBottom);
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
        graphPost.paddingLeftRight = new Vector2(40,30);
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
    public float[] SelectDataSource(int indexDS)
    {
        Debug.LogWarning("Getting Data Source");
        float[] datasource = new float[PhysioThread.Time_PDM.Length];
        string variableName = variables[indexDS].ToString();

        if (variableName == "LHipAngle") { datasource = PhysioThread.LHipAngle_Sag; }
        else if (variableName == "LKneeAngle") { datasource = PhysioThread.LKneeAngle_Sag; }
        else if (variableName == "LAnkleAngle") { datasource = PhysioThread.LAnkleAngle_Sag; }
        else if (variableName == "RHipAngle") { datasource = PhysioThread.RHipAngle_Sag; }
        else if (variableName == "RKneeAngle") { datasource = PhysioThread.RKneeAngle_Sag; }
        else if (variableName == "RAnkleAngle") { datasource = PhysioThread.RAnkleAngle_Sag; }
        else if (variableName == "accWorldPelvis.x") { datasource = PhysioThread.AccW_P_X; }
        else if (variableName == "accWorldPelvis.y") { datasource = PhysioThread.AccW_P_Y; }
        else if (variableName == "accWorldPelvis.z") { datasource = PhysioThread.AccW_P_Z; }
        else if (variableName == "accWorldLeftThigh.x") { datasource = PhysioThread.AccW_LT_X; }
        else if (variableName == "accWorldLeftThigh.y") { datasource = PhysioThread.AccW_LT_Y; }
        else if (variableName == "accWorldLeftThigh.z") { datasource = PhysioThread.AccW_LT_Z; }
        else if (variableName == "accWorldLeftCalf.x") { datasource = PhysioThread.AccW_LC_X; }
        else if (variableName == "accWorldLeftCalf.y") { datasource = PhysioThread.AccW_LC_Y; }
        else if (variableName == "accWorldLeftCalf.z") { datasource = PhysioThread.AccW_LC_Z; }
        else if (variableName == "accWorldLeftFoot.x") { datasource = PhysioThread.AccW_LF_X; }
        else if (variableName == "accWorldLeftFoot.y") { datasource = PhysioThread.AccW_LF_Y; }
        else if (variableName == "accWorldLeftFoot.z") { datasource = PhysioThread.AccW_LF_Z; }
        else if (variableName == "accWorldRightThigh.x") { datasource = PhysioThread.AccW_RT_X; }
        else if (variableName == "accWorldRightThigh.y") { datasource = PhysioThread.AccW_RT_Y; }
        else if (variableName == "accWorldRightThigh.z") { datasource = PhysioThread.AccW_RT_Z; }
        else if (variableName == "accWorldRightCalf.x") { datasource = PhysioThread.AccW_RC_X; }
        else if (variableName == "accWorldRightCalf.y") { datasource = PhysioThread.AccW_RC_Y; }
        else if (variableName == "accWorldRightCalf.z") { datasource = PhysioThread.AccW_RC_Z; }
        else if (variableName == "accWorldRightFoot.x") { datasource = PhysioThread.AccW_RF_X; }
        else if (variableName == "accWorldRightFoot.y") { datasource = PhysioThread.AccW_RF_Y; }
        else if (variableName == "accWorldRightFoot.z") { datasource = PhysioThread.AccW_RF_Z; }
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
    
    public void PlayVideoBUTTON()
    {
        if (already_played)
        {
            play_bool = true;
            PhysioThread.acquiring = false;
            PausePostButton.SetActive(true);
            PlayPostButton.SetActive(false);

            pause_time = Time.time - init_pause_time;
            //Debug.LogError("pause_time: " + pause_time + "current_time: "+current_time + "  current-pause: "+(current_time-pause_time));
        }
        else
        {
            PhysioThread.acquiring = false;
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

        if (frame < PhysioThread.PDM_DATA_Size-3)
        {
            if (Math.Abs(PhysioThread.Time_PDM[frame] - current_time) < 0.1)
            {
                frame++;
            }
            //Debug.LogError("time:  "+PhysioThread.Time_PDM[frame] + "   currenttime: "+current_time);
            Quat_Pelvis = new Quaternion(PhysioThread.Quat_P_X[frame], PhysioThread.Quat_P_Y[frame], PhysioThread.Quat_P_Z[frame], PhysioThread.Quat_P_W[frame]);
            Quat_LeftThigh = new Quaternion(PhysioThread.Quat_LT_X[frame], PhysioThread.Quat_LT_Y[frame], PhysioThread.Quat_LT_Z[frame], PhysioThread.Quat_LT_W[frame]);
            Quat_LeftCalf = new Quaternion(PhysioThread.Quat_LC_X[frame], PhysioThread.Quat_LC_Y[frame], PhysioThread.Quat_LC_Z[frame], PhysioThread.Quat_LC_W[frame]);
            Quat_LeftFoot = new Quaternion(PhysioThread.Quat_LF_X[frame], PhysioThread.Quat_LF_Y[frame], PhysioThread.Quat_LF_Z[frame], PhysioThread.Quat_LF_W[frame]);
            Quat_RightThigh = new Quaternion(PhysioThread.Quat_RT_X[frame], PhysioThread.Quat_RT_Y[frame], PhysioThread.Quat_RT_Z[frame], PhysioThread.Quat_RT_W[frame]);
            Quat_RightCalf = new Quaternion(PhysioThread.Quat_RC_X[frame], PhysioThread.Quat_RC_Y[frame], PhysioThread.Quat_RC_Z[frame], PhysioThread.Quat_RC_W[frame]);
            Quat_RightFoot = new Quaternion(PhysioThread.Quat_RF_X[frame], PhysioThread.Quat_RF_Y[frame], PhysioThread.Quat_RF_Z[frame], PhysioThread.Quat_RF_W[frame]);

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