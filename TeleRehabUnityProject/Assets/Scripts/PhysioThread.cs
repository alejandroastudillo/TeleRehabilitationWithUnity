using UnityEngine;
using System.Collections;
using System.IO.Ports;
using uint8_t = System.Byte;
using int16_t = System.Int16;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using UnityEditor;

public class PhysioThread : MonoBehaviour
{
    public static Thread readSerial;
    public bool threadAlive;

    public GameObject BackButton;

    public Camera PerspectiveCamera, SagitalCamera, FrontalCamera;
    string CameraView = "Perspective";

    public Transform Pelvis, LThigh, LCalf, LFoot, RThigh, RCalf, RFoot;

    int offset = 0, bytesRead, bytesExpected = 210;

    private int frameCycle = 0, frameCycle2 = 0;
    public static int BodyMoveUpdateFrame = 2;
    private int CheckNodesUpdateFrame = 100;
    public static bool acquiring = false;
    private static float GRAVITY = 9.81f;
    private Vector3 GravityVector = new Vector3(0, 0, GRAVITY);
    public float speed = 10000f;

    List<string> Raw_Data = new List<string>();
    List<string> PDM_Data = new List<string>();
    public static int PDM_DATA_Size;
    public static List<string> Time_RAW, Pelvis_AccX, Pelvis_AccY, Pelvis_AccZ, Pelvis_GyroX, Pelvis_GyroY, Pelvis_GyroZ, Pelvis_MagX, Pelvis_MagY, Pelvis_MagZ, Pelvis_QuatX, Pelvis_QuatY, Pelvis_QuatZ, Pelvis_QuatW, LThigh_AccX, LThigh_AccY, LThigh_AccZ, LThigh_GyroX, LThigh_GyroY, LThigh_GyroZ, LThigh_MagX, LThigh_MagY, LThigh_MagZ, LThigh_QuatX, LThigh_QuatY, LThigh_QuatZ, LThigh_QuatW, LCalf_AccX, LCalf_AccY, LCalf_AccZ, LCalf_GyroX, LCalf_GyroY, LCalf_GyroZ, LCalf_MagX, LCalf_MagY, LCalf_MagZ, LCalf_QuatX, LCalf_QuatY, LCalf_QuatZ, LCalf_QuatW, LFoot_AccX, LFoot_AccY, LFoot_AccZ, LFoot_GyroX, LFoot_GyroY, LFoot_GyroZ, LFoot_MagX, LFoot_MagY, LFoot_MagZ, LFoot_QuatX, LFoot_QuatY, LFoot_QuatZ, LFoot_QuatW, RThigh_AccX, RThigh_AccY, RThigh_AccZ, RThigh_GyroX, RThigh_GyroY, RThigh_GyroZ, RThigh_MagX, RThigh_MagY, RThigh_MagZ, RThigh_QuatX, RThigh_QuatY, RThigh_QuatZ, RThigh_QuatW, RCalf_AccX, RCalf_AccY, RCalf_AccZ, RCalf_GyroX, RCalf_GyroY, RCalf_GyroZ, RCalf_MagX, RCalf_MagY, RCalf_MagZ, RCalf_QuatX, RCalf_QuatY, RCalf_QuatZ, RCalf_QuatW, RFoot_AccX, RFoot_AccY, RFoot_AccZ, RFoot_GyroX, RFoot_GyroY, RFoot_GyroZ, RFoot_MagX, RFoot_MagY, RFoot_MagZ, RFoot_QuatX, RFoot_QuatY, RFoot_QuatZ, RFoot_QuatW;
    public static float[] Time_PDM, Quat_P_X, Quat_P_Y, Quat_P_Z, Quat_P_W, Quat_LT_X, Quat_LT_Y, Quat_LT_Z, Quat_LT_W, Quat_LC_X, Quat_LC_Y, Quat_LC_Z, Quat_LC_W, Quat_LF_X, Quat_LF_Y, Quat_LF_Z, Quat_LF_W, Quat_RT_X, Quat_RT_Y, Quat_RT_Z, Quat_RT_W, Quat_RC_X, Quat_RC_Y, Quat_RC_Z, Quat_RC_W, Quat_RF_X, Quat_RF_Y, Quat_RF_Z, Quat_RF_W, LHipAngle_Sag, RHipAngle_Sag, LKneeAngle_Sag, RKneeAngle_Sag, LAnkleAngle_Sag, RAnkleAngle_Sag, AccW_P_X, AccW_P_Y, AccW_P_Z, AccW_LT_X, AccW_LT_Y, AccW_LT_Z, AccW_LC_X, AccW_LC_Y, AccW_LC_Z, AccW_LF_X, AccW_LF_Y, AccW_LF_Z, AccW_RT_X, AccW_RT_Y, AccW_RT_Z, AccW_RC_X, AccW_RC_Y, AccW_RC_Z, AccW_RF_X, AccW_RF_Y, AccW_RF_Z, AngVelocity_P_Sag, AngVelocity_P_Trans, AngVelocity_P_Front, AngVelocity_LT_Sag, AngVelocity_LT_Trans, AngVelocity_LT_Front, AngVelocity_LC_Sag, AngVelocity_LC_Trans, AngVelocity_LC_Front, AngVelocity_LF_Sag, AngVelocity_LF_Trans, AngVelocity_LF_Front, AngVelocity_RT_Sag, AngVelocity_RT_Trans, AngVelocity_RT_Front, AngVelocity_RC_Sag, AngVelocity_RC_Trans, AngVelocity_RC_Front, AngVelocity_RF_Sag, AngVelocity_RF_Trans, AngVelocity_RF_Front;

    bool saveRawData = true;
    bool savePDMData = true;
    public string RawDataFile, PDMDataFile;

    //SerialPort stream = new SerialPort("\\\\.\\"+UsersList.patientsessioninfo.COMport, 38400); //   "\\\\.\\COM13",38400
    public static SerialPort stream = new SerialPort("\\\\.\\COM11", 38400); //   "\\\\.\\COM13",38400
    public byte[] readBytes = new byte[210];
    public byte[] readBytes30 = new byte[30];
    byte[] subframe = new byte[30];
    public byte[] readFrame = new byte[210];

    int serialTimeout = 100;

    public struct ImuData
    {
        // Basic info

        public byte id;
        public bool online;
        public byte battery;

        // Callibration status

        // system <-- callibration[0]
        // gyr<-- callibration[1]
        // mag<-- callibration[2]
        // acc <-- callibration[3]
        public byte[] callibration;
        // Sensor data

        public Vector3 accelerometer;
        public Vector3 magnetometer;
        public Vector3 gyroscope;
        public Quaternion quaternion;

    };

    const double gyr_lsb = 900.0;
    const double acc_lsb = 100.0;
    const double mag_lsb = 16.0;
    const double quat_scale = (1.0 / (1 << 14));

    //  C++ unsigned char = C# byte
    //  C++ char = C# sbyte

    int PELVIS_NODE = 1;
    int LEFT_THIGH_NODE = 2;
    int LEFT_CALF_NODE = 3;
    int LEFT_FOOT_NODE = 4;
    int RIGHT_THIGH_NODE = 5;
    int RIGHT_CALF_NODE = 6;
    int RIGHT_FOOT_NODE = 7;

    public static ImuData PELVIS, LEFT_THIGH, LEFT_CALF, LEFT_FOOT, RIGHT_THIGH, RIGHT_CALF, RIGHT_FOOT, output;
    //ImuData output --> Dummie variable to get the data and then assign it to one of the other ImuData vars.

    public static float time, lasttime, deltatime, init_time, actualtime;
    public float serialInitTime, serialFinalTime;
    string hex1; 

    public Quaternion Quat_Pelvis, Quat_LeftThigh, Quat_LeftCalf, Quat_LeftFoot, Quat_RightThigh, Quat_RightCalf, Quat_RightFoot;
    public static Vector3 accWorldPelvis, accWorldLeftThigh, accWorldLeftCalf, accWorldLeftFoot, accWorldRightThigh, accWorldRightCalf, accWorldRightFoot;
    public static float LHipAngle, RHipAngle, LKneeAngle, RKneeAngle, LAnkleAngle, RAnkleAngle;
    public static float SagAngVelocity_P, SagAngVelocity_LT, SagAngVelocity_LC, SagAngVelocity_LF, SagAngVelocity_RT, SagAngVelocity_RC, SagAngVelocity_RF;
    public static float FrontAngVelocity_P, FrontAngVelocity_LT, FrontAngVelocity_LC, FrontAngVelocity_LF, FrontAngVelocity_RT, FrontAngVelocity_RC, FrontAngVelocity_RF;
    public static float TransAngVelocity_P, TransAngVelocity_LT, TransAngVelocity_LC, TransAngVelocity_LF, TransAngVelocity_RT, TransAngVelocity_RC, TransAngVelocity_RF;
    public static float cadence, step_length, stride_length, step_time, last_step_time;

    public Quaternion initQ_P, initQ_LT, initQ_LC, initQ_LF, initQ_RT, initQ_RC, initQ_RF;

    private GameObject SaveDataButton;

    // Use this for initialization
    void Start()
    {
        lasttime = Time.time;
        SaveDataButton = GameObject.Find("SaveButton");

        SagitalCamera.enabled = false;
        PerspectiveCamera.enabled = true;
        FrontalCamera.enabled = false;
        CameraView = "Perspective";
        BackButton.SetActive(true);
    }

    public void StartReadThread()
    {
        acquiring = true;
        BackButton.SetActive(false);
        
        threadAlive = false;

        init_time = Time.time;
        Raw_Data.Clear();
        Raw_Data.Add(UsersList.patientsessioninfo.Username+";"+UsersList.patientsessioninfo.SessionDate);
        Raw_Data.Add("Time; Pelvis.ID; Pelvis.WorldAcc.X; Pelvis.WorldAcc.Y; Pelvis.WorldAcc.Z; Pelvis.Gyro.X; Pelvis.Gyro.Y; Pelvis.Gyro.Z; Pelvis.Magnetometer.X; Pelvis.Magnetometer.Y; Pelvis.Magnetometer.Z; Pelvis.Quaternion.X; Pelvis.Quaternion.Y; Pelvis.Quaternion.Z; Pelvis.Quaternion.W; " +
                     "LeftThigh.ID; LeftThigh.WorldAcc.X; LeftThigh.WorldAcc.Y; LeftThigh.WorldAcc.Z; LeftThigh.Gyro.X; LeftThigh.Gyro.Y; LeftThigh.Gyro.Z; LeftThigh.Magnetometer.X; LeftThigh.Magnetometer.Y; LeftThigh.Magnetometer.Z; LeftThigh.Quaternion.X; LeftThigh.Quaternion.Y; LeftThigh.Quaternion.Z; LeftThigh.Quaternion.W" +
                     "LeftCalf.ID; LeftCalf.WorldAcc.X; LeftCalf.WorldAcc.Y; LeftCalf.WorldAcc.Z; LeftCalf.Gyro.X; LeftCalf.Gyro.Y; LeftCalf.Gyro.Z; LeftCalf.Magnetometer.X; LeftCalf.Magnetometer.Y; LeftCalf.Magnetometer.Z; LeftCalf.Quaternion.X; LeftCalf.Quaternion.Y; LeftCalf.Quaternion.Z; LeftCalf.Quaternion.W" +
                     "LeftFoot.ID; LeftFoot.WorldAcc.X; LeftFoot.WorldAcc.Y; LeftFoot.WorldAcc.Z; LeftFoot.Gyro.X; LeftFoot.Gyro.Y; LeftFoot.Gyro.Z; LeftFoot.Magnetometer.X; LeftFoot.Magnetometer.Y; LeftFoot.Magnetometer.Z; LeftFoot.Quaternion.X; LeftFoot.Quaternion.Y; LeftFoot.Quaternion.Z; LeftFoot.Quaternion.W" +
                     "RightThigh.ID; RightThigh.WorldAcc.X; RightThigh.WorldAcc.Y; RightThigh.WorldAcc.Z; RightThigh.Gyro.X; RightThigh.Gyro.Y; RightThigh.Gyro.Z; RightThigh.Magnetometer.X; RightThigh.Magnetometer.Y; RightThigh.Magnetometer.Z; RightThigh.Quaternion.X; RightThigh.Quaternion.Y; RightThigh.Quaternion.Z; RightThigh.Quaternion.W" +
                     "RightCalf.ID; RightCalf.WorldAcc.X; RightCalf.WorldAcc.Y; RightCalf.WorldAcc.Z; RightCalf.Gyro.X; RightCalf.Gyro.Y; RightCalf.Gyro.Z; RightCalf.Magnetometer.X; RightCalf.Magnetometer.Y; RightCalf.Magnetometer.Z; RightCalf.Quaternion.X; RightCalf.Quaternion.Y; RightCalf.Quaternion.Z; RightCalf.Quaternion.W" +
                     "RightFoot.ID; RightFoot.WorldAcc.X; RightFoot.WorldAcc.Y; RightFoot.WorldAcc.Z; RightFoot.Gyro.X; RightFoot.Gyro.Y; RightFoot.Gyro.Z; RightFoot.Magnetometer.X; RightFoot.Magnetometer.Y; RightFoot.Magnetometer.Z; RightFoot.Quaternion.X; RightFoot.Quaternion.Y; RightFoot.Quaternion.Z; RightFoot.Quaternion.W");
        PDM_Data.Clear();
        PDM_Data.Add(UsersList.patientsessioninfo.Username + ";" + UsersList.patientsessioninfo.SessionDate);
        PDM_Data.Add("Time; SagLHipAngle; SagRHipAngle; SagLKneeAngle; SagRKneeAngle; SagLAnkleAngle; SagRAnkleAngle; Quat_Pelvis.x; Quat_Pelvis.y; Quat_Pelvis.z; Quat_Pelvis.w; Quat_LeftThigh.x; Quat_LeftThigh.y; Quat_LeftThigh.z; Quat_LeftThigh.w; Quat_LeftCalf.x; Quat_LeftCalf.y; Quat_LeftCalf.z; Quat_LeftCalf.w; Quat_LeftFoot.x; Quat_LeftFoot.y; Quat_LeftFoot.z; Quat_LeftFoot.w; Quat_RightThigh.x; Quat_RightThigh.y; Quat_RightThigh.z; Quat_RightThigh.w; Quat_RightCalf.x; Quat_RightCalf.y; Quat_RightCalf.z; Quat_RightCalf.w; Quat_RightFoot.x; Quat_RightFoot.y; Quat_RightFoot.z; Quat_RightFoot.w; " +
                     "accWorldPelvis.x; accWorldPelvis.y; accWorldPelvis.z; accWorldLeftThigh.x; accWorldLeftThigh.y; accWorldLeftThigh.z; accWorldLeftCalf.x; accWorldLeftCalf.y; accWorldLeftCalf.z; accWorldLeftFoot.x; accWorldLeftFoot.y; accWorldLeftFoot.z; accWorldRightThigh.x; accWorldRightThigh.y; accWorldRightThigh.z; accWorldRightCalf.x; accWorldRightCalf.y; accWorldRightCalf.z; accWorldRightFoot.x; accWorldRightFoot.y; accWorldRightFoot.z; " +
                     "SagAngVelocity_P; TransAngVelocity_P; FrontAngVelocity_P; SagAngVelocity_LT; TransAngVelocity_LT; FrontAngVelocity_LT; SagAngVelocity_LC; TransAngVelocity_LC; FrontAngVelocity_LC; SagAngVelocity_LF; TransAngVelocity_LF; FrontAngVelocity_LF; SagAngVelocity_RT; TransAngVelocity_RT; FrontAngVelocity_RT; SagAngVelocity_RC; TransAngVelocity_RC; FrontAngVelocity_RC; SagAngVelocity_RF; TransAngVelocity_RF; FrontAngVelocity_RF");


        stream.ReadTimeout = serialTimeout;
        OpenConnection();
        ReadInitialConditions();
        frameCycle = 0;

        if (!threadAlive)
        {
            readSerial = new Thread(Run);
            readSerial.Start();
        }
        SetNodesData();
    }

    public void StopReadThread()
    {
        if (acquiring)
        {
            readSerial.Abort();
            threadAlive = false;
            if (stream.IsOpen)
            {
                stream.BaseStream.Flush();
                stream.Close();
                Debug.Log("Port closed!");
            }
        }
        acquiring = false;
        BackButton.SetActive(true);
        
        OrganizeData();

    }

    public void Run()
    {
        while (acquiring)
        {
            threadAlive = true;
            // START TO READ SERIAL DATA ----------------------------------------------------------------------------
            offset = 0; bytesExpected = 210;
            while (bytesExpected > 0 && (bytesRead = stream.Read(readFrame, offset, bytesExpected)) > 0)
            {
                offset += bytesRead;
                bytesExpected -= bytesRead;

                if (bytesExpected == 30 || bytesExpected == 60 || bytesExpected == 90 || bytesExpected == 120 || bytesExpected == 150 || bytesExpected == 180)
                {
                    stream.BaseStream.Flush();
                }
            }
            // FINISH TO READ SERIAL DATA --------------------------------------------------------------------------

            // START TO DECODE THE IMUs DATA -----------------------------------------------------------------------
            for (int i = 0; i <= 6; i++)
            {
                Array.Copy(readFrame, i * 30, subframe, 0, 30); //Get a 30 bytes frame from the readFrame (210 bytes).
                                                                // Array.Copy(source array, start index in source array, destination array, start index in destination array, elements to copy);
                output = decodeFrame(subframe, 30); //Decode each 30 bytes frame.

                if (output.id == PELVIS_NODE)
                {
                    PELVIS = output;
                }
                else if (output.id == LEFT_THIGH_NODE)
                {
                    LEFT_THIGH = output;
                }
                else if (output.id == LEFT_CALF_NODE)
                {
                    LEFT_CALF = output;
                }
                else if (output.id == LEFT_FOOT_NODE)
                {
                    LEFT_FOOT = output;
                }
                else if (output.id == RIGHT_THIGH_NODE)
                {
                    RIGHT_THIGH = output;
                }
                else if (output.id == RIGHT_CALF_NODE)
                {
                    RIGHT_CALF = output;
                }
                else if (output.id == RIGHT_FOOT_NODE)
                {
                    RIGHT_FOOT = output;
                }
            }
            // FINISH TO DECODE THE IMUs DATA ------------------------------------------------------------------
            threadAlive = false;
        }
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

    public void BackBUTTON()
    {
        SceneManager.LoadScene("LogIn", LoadSceneMode.Single);  //LoadSceneMode.Additive
    }

    public void ReadInitialConditions()
    {
        offset = 0; bytesExpected = 210;
        while (bytesExpected > 0 && (bytesRead = stream.Read(readFrame, offset, bytesExpected)) > 0)
        {
            offset += bytesRead;
            bytesExpected -= bytesRead;

            if (bytesExpected == 30 || bytesExpected == 60 || bytesExpected == 90 || bytesExpected == 120 || bytesExpected == 150 || bytesExpected == 180)
            {
                stream.BaseStream.Flush();
            }
        }

        //stream.BaseStream.Flush();

        for (int i = 0; i <= 6; i++)
        {
            Array.Copy(readFrame, i * 30, subframe, 0, 30); //Get a 30 bytes frame from the readFrame (210 bytes).
                                                            // Array.Copy(source array, start index in source array, destination array, start index in destination array, elements to copy);
            output = decodeFrame(subframe, 30); //Decode each 30 bytes frame.

            if (output.id == PELVIS_NODE)
            {
                PELVIS = output;
            }
            else if (output.id == LEFT_THIGH_NODE)
            {
                LEFT_THIGH = output;
            }
            else if (output.id == LEFT_CALF_NODE)
            {
                LEFT_CALF = output;
            }
            else if (output.id == LEFT_FOOT_NODE)
            {
                LEFT_FOOT = output;
            }
            else if (output.id == RIGHT_THIGH_NODE)
            {
                RIGHT_THIGH = output;
            }
            else if (output.id == RIGHT_CALF_NODE)
            {
                RIGHT_CALF = output;
            }
            else if (output.id == RIGHT_FOOT_NODE)
            {
                RIGHT_FOOT = output;
            }
        }
        initQ_P = PELVIS.quaternion;
        initQ_LT = LEFT_THIGH.quaternion;
        initQ_LC = LEFT_CALF.quaternion;
        initQ_LF = LEFT_FOOT.quaternion;
        initQ_RT = RIGHT_THIGH.quaternion;
        initQ_RC = RIGHT_CALF.quaternion;
        initQ_RF = RIGHT_FOOT.quaternion;

    }
    
    public void UpdateCheckNodes()
    {
        Debug.Log("PhysioThread UpdateCheck");
        OpenConnection();
        ReadInitialConditions();

        SetNodesData();

        if (stream.IsOpen)
        {
            stream.BaseStream.Flush();
            stream.Close();
            Debug.Log("Port closed!");
        }

        
    }

    private void SetNodesData()
    {
        frameCycle2 = 0;
        NodesCheck.ID_P = PELVIS.id;
        NodesCheck.ID_LT = LEFT_THIGH.id;
        NodesCheck.ID_LC = LEFT_CALF.id;
        NodesCheck.ID_LF = LEFT_FOOT.id;
        NodesCheck.ID_RT = RIGHT_THIGH.id;
        NodesCheck.ID_RC = RIGHT_CALF.id;
        NodesCheck.ID_RF = RIGHT_FOOT.id;
        NodesCheck.Online_P = PELVIS.online;
        NodesCheck.Online_LT = LEFT_THIGH.online;
        NodesCheck.Online_LC = LEFT_CALF.online;
        NodesCheck.Online_LF = LEFT_FOOT.online;
        NodesCheck.Online_RT = RIGHT_THIGH.online;
        NodesCheck.Online_RC = RIGHT_CALF.online;
        NodesCheck.Online_RF = RIGHT_FOOT.online;
        NodesCheck.Battery_P = PELVIS.battery;
        NodesCheck.Battery_LT = LEFT_THIGH.battery;
        NodesCheck.Battery_LC = LEFT_CALF.battery;
        NodesCheck.Battery_LF = LEFT_FOOT.battery;
        NodesCheck.Battery_RT = RIGHT_THIGH.battery;
        NodesCheck.Battery_RC = RIGHT_CALF.battery;
        NodesCheck.Battery_RF = RIGHT_FOOT.battery;
        NodesCheck.CalAcc_P = PELVIS.callibration[2];
        NodesCheck.CalAcc_LT = LEFT_THIGH.callibration[2];
        NodesCheck.CalAcc_LC = LEFT_CALF.callibration[2];
        NodesCheck.CalAcc_LF = LEFT_FOOT.callibration[2];
        NodesCheck.CalAcc_RT = RIGHT_THIGH.callibration[2];
        NodesCheck.CalAcc_RC = RIGHT_CALF.callibration[2];
        NodesCheck.CalAcc_RF = RIGHT_FOOT.callibration[2];
        NodesCheck.CalGyro_P = PELVIS.callibration[1];
        NodesCheck.CalGyro_LT = LEFT_THIGH.callibration[1];
        NodesCheck.CalGyro_LC = LEFT_CALF.callibration[1];
        NodesCheck.CalGyro_LF = LEFT_FOOT.callibration[1];
        NodesCheck.CalGyro_RT = RIGHT_THIGH.callibration[1];
        NodesCheck.CalGyro_RC = RIGHT_CALF.callibration[1];
        NodesCheck.CalGyro_RF = RIGHT_FOOT.callibration[1];
        NodesCheck.CalMag_P = PELVIS.callibration[3];
        NodesCheck.CalMag_LT = LEFT_THIGH.callibration[3];
        NodesCheck.CalMag_LC = LEFT_CALF.callibration[3];
        NodesCheck.CalMag_LF = LEFT_FOOT.callibration[3];
        NodesCheck.CalMag_RT = RIGHT_THIGH.callibration[3];
        NodesCheck.CalMag_RC = RIGHT_CALF.callibration[3];
        NodesCheck.CalMag_RF = RIGHT_FOOT.callibration[3];

        NodesCheck.UpdateCheckBUTTON();
    }

    void Update()
    {
        if (acquiring)
        {
            frameCycle++; frameCycle2++;
            MoveBody();
            if (frameCycle == BodyMoveUpdateFrame) { GetGaitParameters(); }
            if (frameCycle2 == CheckNodesUpdateFrame) { SetNodesData(); }
        }
    }

    public void MoveBody()
    {

        //frameCycle = 0;
        time = Time.time; // in seconds
        deltatime = (time - lasttime) * 1000; // in miliseconds
        lasttime = time;

        int moveMethod = 3;
        switch (moveMethod)
        {
            case 3:
                Quat_Pelvis = Quaternion.Inverse(PELVIS.quaternion) * initQ_P;
                Quat_Pelvis = new Quaternion(-Quat_Pelvis.y, Quat_Pelvis.x, -Quat_Pelvis.z, Quat_Pelvis.w);
                Pelvis.rotation = Quat_Pelvis;
                Quat_LeftThigh = Quaternion.Inverse(LEFT_THIGH.quaternion) * initQ_LT;
                Quat_LeftThigh = new Quaternion(-Quat_LeftThigh.z, Quat_LeftThigh.x, Quat_LeftThigh.y, Quat_LeftThigh.w);
                LThigh.rotation = Quat_LeftThigh;
                Quat_LeftCalf = Quaternion.Inverse(LEFT_CALF.quaternion) * initQ_LC;
                Quat_LeftCalf = new Quaternion(-Quat_LeftCalf.z, Quat_LeftCalf.x, Quat_LeftCalf.y, Quat_LeftCalf.w);
                LCalf.rotation = Quat_LeftCalf;
                Quat_LeftFoot = Quaternion.Inverse(LEFT_FOOT.quaternion) * initQ_LF;
                Quat_LeftFoot = new Quaternion(-Quat_LeftFoot.y, Quat_LeftFoot.z, Quat_LeftFoot.x, Quat_LeftFoot.w);
                LFoot.rotation = Quat_LeftFoot;
                Quat_RightThigh = Quaternion.Inverse(RIGHT_THIGH.quaternion) * initQ_RT;
                Quat_RightThigh = new Quaternion(Quat_RightThigh.z, Quat_RightThigh.x, -Quat_RightThigh.y, Quat_RightThigh.w);
                RThigh.rotation = Quat_RightThigh;
                Quat_RightCalf = Quaternion.Inverse(RIGHT_CALF.quaternion) * initQ_RC;
                Quat_RightCalf = new Quaternion(Quat_RightCalf.z, Quat_RightCalf.x, -Quat_RightCalf.y, Quat_RightCalf.w);
                RCalf.rotation = Quat_RightCalf;
                Quat_RightFoot = Quaternion.Inverse(RIGHT_FOOT.quaternion) * initQ_RF;
                Quat_RightFoot = new Quaternion(-Quat_RightFoot.y, Quat_RightFoot.z, Quat_RightFoot.x, Quat_RightFoot.w);
                RFoot.rotation = Quat_RightFoot;

                print("Case 3");
                break;
            case 4:
                Quat_Pelvis = Quaternion.Inverse(PELVIS.quaternion) * initQ_P;
                Pelvis.rotation = new Quaternion(-Quat_Pelvis.y, Quat_Pelvis.x, -Quat_Pelvis.z, Quat_Pelvis.w);
                Quat_LeftThigh = Quaternion.Inverse(LEFT_THIGH.quaternion) * initQ_LT;
                LThigh.rotation = new Quaternion(-Quat_LeftThigh.z, Quat_LeftThigh.x, Quat_LeftThigh.y, Quat_LeftThigh.w);
                Quat_LeftCalf = Quaternion.Inverse(LEFT_CALF.quaternion) * initQ_LC;
                LCalf.rotation = new Quaternion(-Quat_LeftCalf.z, Quat_LeftCalf.x, Quat_LeftCalf.y, Quat_LeftCalf.w);
                Quat_LeftFoot = Quaternion.Inverse(LEFT_FOOT.quaternion) * initQ_LF;
                LFoot.rotation = new Quaternion(-Quat_LeftFoot.y, Quat_LeftFoot.z, Quat_LeftFoot.x, Quat_LeftFoot.w);
                Quat_RightThigh = Quaternion.Inverse(RIGHT_THIGH.quaternion) * initQ_RT;
                RThigh.rotation = new Quaternion(Quat_RightThigh.z, Quat_RightThigh.x, -Quat_RightThigh.y, Quat_RightThigh.w);
                Quat_RightCalf = Quaternion.Inverse(RIGHT_CALF.quaternion) * initQ_RC;
                RCalf.rotation = new Quaternion(Quat_RightCalf.z, Quat_RightCalf.x, -Quat_RightCalf.y, Quat_RightCalf.w);
                Quat_RightFoot = Quaternion.Inverse(RIGHT_FOOT.quaternion) * initQ_RF;
                RFoot.rotation = new Quaternion(-Quat_RightFoot.y, Quat_RightFoot.z, Quat_RightFoot.x, Quat_RightFoot.w);

                print("Case 4");
                break;

        }
        
    }

    public void OpenConnection()
    {
        if (stream != null)
        {
            if (stream.IsOpen)
            {
                stream.Close();
                Debug.Log("Closing port, because it was already open!");
            }
            else
            {
                stream.Open();
                //stream.ReadTimeout = 50;
                stream.ReadTimeout = serialTimeout;
                Debug.Log("Port Opened!");
            }
        }
        else
        {
            if (stream.IsOpen)
            {
                print("Port is already open");
            }
            else
            {
                print("Port == null");
            }
        }
    }

    void OnApplicationQuit()
    {
        if (acquiring)
        {
            if (stream.IsOpen)
            {
                stream.BaseStream.Flush();
                stream.Close();
                Debug.Log("Port closed!");
            }
        }
        acquiring = false;
    }

    public static ImuData decodeFrame(byte[] inputData, int inputSize)
    {
        ImuData outputData;
        // First, check bytes [28] and [29]
        // If signature doesn't match, return false

        // Check size in order support sending 28 bytes to debug (from Arduino)
        if (inputSize > 28)
        {
            if ((((uint8_t)inputData[28]) != 255) && (((uint8_t)inputData[29]) != 10))
            {
                //return false;
            }
        }


        // x-> is syntactic sugar for (*x).

        // ABCD EFGH

        // Sensor ID
        outputData.id = (byte)((inputData[0] >> 4) & 15); // 0000ABCD

        // Sensor status
        outputData.online = Convert.ToBoolean((inputData[0] & 8) >> 3);         // 0000000E

        // Battery
        outputData.battery = (byte)(inputData[0] & 7);       // 00000FGH

        // TO-DO: decode from levels to voltage

        //	switch( gato.battery )
        //	{
        //		case 5:
        //			// do stuff
        //		break;
        //	}

        // Callibration values
        // Endianness doesn't matter here only a byte is being read.

        //outputData.callibration.system = (byte)((inputData[1] >> 6) & 3);
        //outputData.callibration.gyr = (byte)((inputData[1] >> 4) & 3);
        //outputData.callibration.acc = (byte)((inputData[1] >> 2) & 3);
        //outputData.callibration.mag = (byte)(inputData[1] & 3);
        outputData.callibration = new byte[] { (byte)((inputData[1] >> 6) & 3), (byte)((inputData[1] >> 4) & 3), (byte)((inputData[1] >> 2) & 3), (byte)(inputData[1] & 3) };

        //qDebug() << gato.callibration.system << gato.callibration.acc << gato.callibration.mag << gato.callibration.gyr;

        // [1] Temp variables to do the joining and int16_t cast:

        int16_t w, x, y, z;
        w = x = y = z = 0;

        /*

	    // When inputData[X] is cast as int16_t directly
	    // an implicit conversion occurs. See [W].

	    // MSB: Preserve the original sign with a signed type because the
	    //      physical variables (and thus our results) are signed.
	    //
	    // This appends ones to the left if the actual size > 16 and when shifing

	    // LSB: CAST AS UNSIGNED so there's no autofill with 1s to the left
	    //      when expanding (as this would interfere with the OR operation).
	    //
	    // Could use a mask, both solutions should be independent of the underlying size!
	    // Here uint8_t is implicitly promoted to int16_t do the bitwise op,
	    // but it's value isn't changed.

	    // Endianness doesn't matter, all bitwise operations and shifts in C/C++
	    // are in "logical order" (MSB ... LSB)

	    */

        // Accelerometer



        //x = ( ((int16_t)inputData[3]) << 8 ) | ( (uint8_t)inputData[2] );
        //y = ( ((int16_t)inputData[5]) << 8 ) | ( (uint8_t)inputData[4] );
        //z = ( ((int16_t)inputData[7]) << 8 ) | ( (uint8_t)inputData[6] );
        x = (int16_t)((int16_t)((inputData[3]) << 8) + inputData[2]);
        y = (int16_t)((int16_t)((inputData[5]) << 8) + inputData[4]);
        z = (int16_t)((int16_t)((inputData[7]) << 8) + inputData[6]);

        outputData.accelerometer = new Vector3( (float)(x / acc_lsb), (float)(y / acc_lsb), (float)(z/ acc_lsb) );

        // Magnetometer  

        x = (int16_t)((int16_t)((inputData[9]) << 8) + inputData[8]);
        y = (int16_t)((int16_t)((inputData[11]) << 8) + inputData[10]);
        z = (int16_t)((int16_t)((inputData[13]) << 8) + inputData[12]);

        outputData.magnetometer = new Vector3((float)(x / mag_lsb), (float)(y / mag_lsb), (float)(z / mag_lsb));

        // Gyroscope

        x = (int16_t)((int16_t)((inputData[15]) << 8) + inputData[14]);
        y = (int16_t)((int16_t)((inputData[17]) << 8) + inputData[16]);
        z = (int16_t)((int16_t)((inputData[19]) << 8) + inputData[18]);

        outputData.gyroscope = new Vector3((float)(x / gyr_lsb), (float)(y / gyr_lsb), (float)(z / gyr_lsb));
        // Quaternions



        // Same idea as above

        w = (int16_t)((int16_t)((inputData[21]) << 8) + inputData[20]);
        x = (int16_t)((int16_t)((inputData[23]) << 8) + inputData[22]);
        y = (int16_t)((int16_t)((inputData[25]) << 8) + inputData[24]);
        z = (int16_t)((int16_t)((inputData[27]) << 8) + inputData[26]);

        // Implicit (double) promotion of w x y z because quat_scale is double

        outputData.quaternion = new Quaternion((float)(x * quat_scale), (float)(y * quat_scale), (float)(z * quat_scale), (float)(w * quat_scale));

        // Print bytes in order to track errors.

        // bytes sample in monospace:
        // idcaax  ay  az  mx  my  mz  gx  gy  gz  qw  qx  qy  qz
        // 1f30e6ff2d00d60300fdcaffd2fefeff00000000fb3f6b01db00fcff

        /*// Debug START

	    Serial.print("DecodeFrame input bytes");
	    Serial.println("");

	    for( int i = 8 ; i <= 13 ; i++ ) //output
	    //for(int i = 6 ; i <= 11; i++) input
	    {
		    Serial.print("Byte ");
		    Serial.print(i,DEC);
		    Serial.print(": ");
		    Serial.print( inputData[i] , HEX ); //output
		    Serial.println("");
	    }

	    Serial.println("");

	    // Debug END*/

        //return true;
        return outputData;

    }

    public void GetGaitParameters()
    {
        frameCycle = 0;

        accWorldPelvis = (PELVIS.quaternion * PELVIS.accelerometer) - GravityVector; // Rotate the Body frame accelerations to get Global frame accelerations and substract the gravity
        accWorldLeftThigh = (LEFT_THIGH.quaternion * LEFT_THIGH.accelerometer) - GravityVector;
        accWorldLeftCalf = (LEFT_CALF.quaternion * LEFT_CALF.accelerometer) - GravityVector;
        accWorldLeftFoot = (LEFT_FOOT.quaternion * LEFT_FOOT.accelerometer) - GravityVector;
        accWorldRightThigh = (RIGHT_THIGH.quaternion * RIGHT_THIGH.accelerometer) - GravityVector;
        accWorldRightCalf = (RIGHT_CALF.quaternion * RIGHT_CALF.accelerometer) - GravityVector;
        accWorldRightFoot = (RIGHT_FOOT.quaternion * RIGHT_FOOT.accelerometer) - GravityVector;
        
        LHipAngle = (LThigh.rotation * Quaternion.Inverse(Pelvis.rotation)).eulerAngles.x;
        if (LHipAngle > 180) { LHipAngle -= 360; }
        RHipAngle = (RThigh.rotation * Quaternion.Inverse(Pelvis.rotation)).eulerAngles.x;
        if (RHipAngle > 180) { RHipAngle -= 360; }
        LKneeAngle = (LCalf.rotation * Quaternion.Inverse(LThigh.rotation)).eulerAngles.x;
        if (LKneeAngle > 180) { LKneeAngle -= 360; }
        RKneeAngle = (RCalf.rotation * Quaternion.Inverse(RThigh.rotation)).eulerAngles.x;
        if (RKneeAngle > 180) { RKneeAngle -= 360; }
        LAnkleAngle = (LFoot.rotation * Quaternion.Inverse(LCalf.rotation)).eulerAngles.x;
        if (LAnkleAngle > 180) { LAnkleAngle -= 360; }
        RAnkleAngle = (RFoot.rotation * Quaternion.Inverse(RCalf.rotation)).eulerAngles.x;
        if (RAnkleAngle > 180) { RAnkleAngle -= 360; }

        SagAngVelocity_P = -PELVIS.gyroscope.y;
        SagAngVelocity_LT = -LEFT_THIGH.gyroscope.z;
        SagAngVelocity_LC = -LEFT_CALF.gyroscope.z;
        SagAngVelocity_LF = -LEFT_FOOT.gyroscope.y;
        SagAngVelocity_RT = RIGHT_THIGH.gyroscope.z;
        SagAngVelocity_RC = RIGHT_CALF.gyroscope.z;
        SagAngVelocity_RF = -RIGHT_FOOT.gyroscope.y;
        FrontAngVelocity_P = -PELVIS.gyroscope.z;
        FrontAngVelocity_LT = LEFT_THIGH.gyroscope.y;
        FrontAngVelocity_LC = LEFT_CALF.gyroscope.y;
        FrontAngVelocity_LF = LEFT_FOOT.gyroscope.x;
        FrontAngVelocity_RT = -RIGHT_THIGH.gyroscope.y;
        FrontAngVelocity_RC = -RIGHT_CALF.gyroscope.y;
        FrontAngVelocity_RF = RIGHT_FOOT.gyroscope.x;

        TransAngVelocity_P = PELVIS.gyroscope.x;
        TransAngVelocity_LT = LEFT_THIGH.gyroscope.x;
        TransAngVelocity_LC = LEFT_CALF.gyroscope.x;
        TransAngVelocity_LF = LEFT_FOOT.gyroscope.z;
        TransAngVelocity_RT = RIGHT_THIGH.gyroscope.x;
        TransAngVelocity_RC = RIGHT_CALF.gyroscope.x;
        TransAngVelocity_RF = RIGHT_FOOT.gyroscope.z;

        actualtime = (Time.time - init_time);
        //Debug.Log("Time: "+actualtime+"    1. LHip: "+LHipAngle+"  RHip: "+RHipAngle+"  LKnee: "+LKneeAngle+"  RKnee: "+RKneeAngle+"  LAnkle: "+LAnkleAngle+"  RAnkle: "+RAnkleAngle);

        //Check that we've received any data
        if (PELVIS.online || LEFT_THIGH.online || LEFT_CALF.online || LEFT_FOOT.online || RIGHT_THIGH.online || RIGHT_CALF.online || RIGHT_FOOT.online)
        {
            if (saveRawData)
            {
                Raw_Data.Add(actualtime + ";" +
                      PELVIS.id + ";" + accWorldPelvis.x + ";" + accWorldPelvis.y + ";" + accWorldPelvis.z + ";" + SagAngVelocity_P + ";" + TransAngVelocity_P + ";" + FrontAngVelocity_P + ";" + PELVIS.magnetometer.x + ";" + PELVIS.magnetometer.y + ";" + PELVIS.magnetometer.z + ";" + PELVIS.quaternion.x + ";" + PELVIS.quaternion.y + ";" + PELVIS.quaternion.z + ";" + PELVIS.quaternion.w + ";" +
                      LEFT_THIGH.id + ";" + accWorldLeftThigh.x + ";" + accWorldLeftThigh.y + ";" + accWorldLeftThigh.z + ";" + SagAngVelocity_LT + ";" + TransAngVelocity_LT + ";" + FrontAngVelocity_LT + ";" + LEFT_THIGH.magnetometer.x + ";" + LEFT_THIGH.magnetometer.y + ";" + LEFT_THIGH.magnetometer.z + ";" + LEFT_THIGH.quaternion.x + ";" + LEFT_THIGH.quaternion.y + ";" + LEFT_THIGH.quaternion.z + ";" + LEFT_THIGH.quaternion.w + ";" +
                      LEFT_CALF.id + ";" + accWorldLeftCalf.x + ";" + accWorldLeftCalf.y + ";" + accWorldLeftCalf.z + ";" + SagAngVelocity_LC + ";" + TransAngVelocity_LC + ";" + FrontAngVelocity_LC + ";" + LEFT_CALF.magnetometer.x + ";" + LEFT_CALF.magnetometer.y + ";" + LEFT_CALF.magnetometer.z + ";" + LEFT_CALF.quaternion.x + ";" + LEFT_CALF.quaternion.y + ";" + LEFT_CALF.quaternion.z + ";" + LEFT_CALF.quaternion.w + ";" +
                      LEFT_FOOT.id + ";" + accWorldLeftFoot.x + ";" + accWorldLeftFoot.y + ";" + accWorldLeftFoot.z + ";" + SagAngVelocity_LF + ";" + TransAngVelocity_LF + ";" + FrontAngVelocity_LF + ";" + LEFT_FOOT.magnetometer.x + ";" + LEFT_FOOT.magnetometer.y + ";" + LEFT_FOOT.magnetometer.z + ";" + LEFT_FOOT.quaternion.x + ";" + LEFT_FOOT.quaternion.y + ";" + LEFT_FOOT.quaternion.z + ";" + LEFT_FOOT.quaternion.w + ";" +
                      RIGHT_THIGH.id + ";" + accWorldRightThigh.x + ";" + accWorldRightThigh.y + ";" + accWorldRightThigh.z + ";" + SagAngVelocity_RT + ";" + TransAngVelocity_RT + ";" + FrontAngVelocity_RT + ";" + RIGHT_THIGH.magnetometer.x + ";" + RIGHT_THIGH.magnetometer.y + ";" + RIGHT_THIGH.magnetometer.z + ";" + RIGHT_THIGH.quaternion.x + ";" + RIGHT_THIGH.quaternion.y + ";" + RIGHT_THIGH.quaternion.z + ";" + RIGHT_THIGH.quaternion.w + ";" +
                      RIGHT_CALF.id + ";" + accWorldRightCalf.x + ";" + accWorldRightCalf.y + ";" + accWorldRightCalf.z + ";" + SagAngVelocity_RC + ";" + TransAngVelocity_RC + ";" + FrontAngVelocity_RC + ";" + RIGHT_CALF.magnetometer.x + ";" + RIGHT_CALF.magnetometer.y + ";" + RIGHT_CALF.magnetometer.z + ";" + RIGHT_CALF.quaternion.x + ";" + RIGHT_CALF.quaternion.y + ";" + RIGHT_CALF.quaternion.z + ";" + RIGHT_CALF.quaternion.w + ";" +
                      RIGHT_FOOT.id + ";" + accWorldRightFoot.x + ";" + accWorldRightFoot.y + ";" + accWorldRightFoot.z + ";" + SagAngVelocity_RF + ";" + TransAngVelocity_RF + ";" + FrontAngVelocity_RF + ";" + RIGHT_FOOT.magnetometer.x + ";" + RIGHT_FOOT.magnetometer.y + ";" + RIGHT_FOOT.magnetometer.z + ";" + RIGHT_FOOT.quaternion.x + ";" + RIGHT_FOOT.quaternion.y + ";" + RIGHT_FOOT.quaternion.z + ";" + RIGHT_FOOT.quaternion.w);
            }
            if (savePDMData)
            {
                PDM_Data.Add(actualtime + ";" + LHipAngle + ";" + RHipAngle + ";" + LKneeAngle + ";" + RKneeAngle + ";" + LAnkleAngle + ";" + RAnkleAngle + ";" +
                     Quat_Pelvis.x + ";" + Quat_Pelvis.y + ";" + Quat_Pelvis.z + ";" + Quat_Pelvis.w + ";" + Quat_LeftThigh.x + ";" + Quat_LeftThigh.y + ";" + Quat_LeftThigh.z + ";" + Quat_LeftThigh.w + ";" + Quat_LeftCalf.x + ";" + Quat_LeftCalf.y + ";" + Quat_LeftCalf.z + ";" + Quat_LeftCalf.w + ";" + Quat_LeftFoot.x + ";" + Quat_LeftFoot.y + ";" + Quat_LeftFoot.z + ";" + Quat_LeftFoot.w + ";" + Quat_RightThigh.x + ";" + Quat_RightThigh.y + ";" + Quat_RightThigh.z + ";" + Quat_RightThigh.w + ";" + Quat_RightCalf.x + ";" + Quat_RightCalf.y + ";" + Quat_RightCalf.z + ";" + Quat_RightCalf.w + ";" + Quat_RightFoot.x + ";" + Quat_RightFoot.y + ";" + Quat_RightFoot.z + ";" + Quat_RightFoot.w + ";" +
                     accWorldPelvis.x + ";" + accWorldPelvis.y + ";" + accWorldPelvis.z + ";" + accWorldLeftThigh.x + ";" + accWorldLeftThigh.y + ";" + accWorldLeftThigh.z + ";" + accWorldLeftCalf.x + ";" + accWorldLeftCalf.y + ";" + accWorldLeftCalf.z + ";" + accWorldLeftFoot.x + ";" + accWorldLeftFoot.y + ";" + accWorldLeftFoot.z + ";" + accWorldRightThigh.x + ";" + accWorldRightThigh.y + ";" + accWorldRightThigh.z + ";" + accWorldRightCalf.x + ";" + accWorldRightCalf.y + ";" + accWorldRightCalf.z + ";" + accWorldRightFoot.x + ";" + accWorldRightFoot.y + ";" + accWorldRightFoot.z + ";" + 
                     SagAngVelocity_P + ";" + TransAngVelocity_P + ";" + FrontAngVelocity_P + ";" + SagAngVelocity_LT + ";" + TransAngVelocity_LT + ";" + FrontAngVelocity_LT + ";" + SagAngVelocity_LC + ";" + TransAngVelocity_LC + ";" + FrontAngVelocity_LC + ";" + SagAngVelocity_LF + ";" + TransAngVelocity_LF + ";" + FrontAngVelocity_LF + ";" + SagAngVelocity_RT + ";" + TransAngVelocity_RT + ";" + FrontAngVelocity_RT + ";" + SagAngVelocity_RC + ";" + TransAngVelocity_RC + ";" + FrontAngVelocity_RC + ";" + SagAngVelocity_RF + ";" + TransAngVelocity_RF + ";" + FrontAngVelocity_RF );               
            }
        }
    }



    // Post-processing ----------------------------------------------------
    public void OrganizeData()
    {
        PDM_DATA_Size = PDM_Data.Count;
        InitializeLists();
        string[] fields;
        if (saveRawData)
        {
            for (int i = 2; i < Raw_Data.Count; i++)
            {
                fields = Raw_Data[i].Split(';');
                Time_RAW.Add(fields[0]);
                Pelvis_AccX.Add(fields[2]); Pelvis_AccY.Add(fields[3]); Pelvis_AccZ.Add(fields[4]);
                Pelvis_GyroX.Add(fields[5]); Pelvis_GyroY.Add(fields[6]); Pelvis_GyroZ.Add(fields[7]);
                Pelvis_MagX.Add(fields[8]); Pelvis_MagY.Add(fields[9]); Pelvis_MagZ.Add(fields[10]);
                Pelvis_QuatX.Add(fields[11]); Pelvis_QuatY.Add(fields[12]); Pelvis_QuatZ.Add(fields[13]); Pelvis_QuatW.Add(fields[14]);
                LThigh_AccX.Add(fields[16]); LThigh_AccY.Add(fields[17]); LThigh_AccZ.Add(fields[18]);
                LThigh_GyroX.Add(fields[19]); LThigh_GyroY.Add(fields[20]); LThigh_GyroZ.Add(fields[21]);
                LThigh_MagX.Add(fields[22]); LThigh_MagY.Add(fields[23]); LThigh_MagZ.Add(fields[24]);
                LThigh_QuatX.Add(fields[25]); LThigh_QuatY.Add(fields[26]); LThigh_QuatZ.Add(fields[27]); LThigh_QuatW.Add(fields[28]);
                LCalf_AccX.Add(fields[30]); LCalf_AccY.Add(fields[31]); LCalf_AccZ.Add(fields[32]);
                LCalf_GyroX.Add(fields[33]); LCalf_GyroY.Add(fields[34]); LCalf_GyroZ.Add(fields[35]);
                LCalf_MagX.Add(fields[36]); LCalf_MagY.Add(fields[37]); LCalf_MagZ.Add(fields[38]);
                LCalf_QuatX.Add(fields[39]); LCalf_QuatY.Add(fields[40]); LCalf_QuatZ.Add(fields[41]); LCalf_QuatW.Add(fields[42]);
                LFoot_AccX.Add(fields[44]); LFoot_AccY.Add(fields[45]); LFoot_AccZ.Add(fields[46]);
                LFoot_GyroX.Add(fields[47]); LFoot_GyroY.Add(fields[48]); LFoot_GyroZ.Add(fields[49]);
                LFoot_MagX.Add(fields[50]); LFoot_MagY.Add(fields[51]); LFoot_MagZ.Add(fields[52]);
                LFoot_QuatX.Add(fields[53]); LFoot_QuatY.Add(fields[54]); LFoot_QuatZ.Add(fields[55]); LFoot_QuatW.Add(fields[56]);
                RThigh_AccX.Add(fields[58]); RThigh_AccY.Add(fields[59]); RThigh_AccZ.Add(fields[60]);
                RThigh_GyroX.Add(fields[61]); RThigh_GyroY.Add(fields[62]); RThigh_GyroZ.Add(fields[63]);
                RThigh_MagX.Add(fields[64]); RThigh_MagY.Add(fields[65]); RThigh_MagZ.Add(fields[66]);
                RThigh_QuatX.Add(fields[67]); RThigh_QuatY.Add(fields[68]); RThigh_QuatZ.Add(fields[69]); RThigh_QuatW.Add(fields[70]);
                RCalf_AccX.Add(fields[72]); RCalf_AccY.Add(fields[73]); RCalf_AccZ.Add(fields[74]);
                RCalf_GyroX.Add(fields[75]); RCalf_GyroY.Add(fields[76]); RCalf_GyroZ.Add(fields[77]);
                RCalf_MagX.Add(fields[78]); RCalf_MagY.Add(fields[79]); RCalf_MagZ.Add(fields[80]);
                RCalf_QuatX.Add(fields[81]); RCalf_QuatY.Add(fields[82]); RCalf_QuatZ.Add(fields[83]); RCalf_QuatW.Add(fields[84]);
                RFoot_AccX.Add(fields[86]); RFoot_AccY.Add(fields[87]); RFoot_AccZ.Add(fields[88]);
                RFoot_GyroX.Add(fields[89]); RFoot_GyroY.Add(fields[90]); RFoot_GyroZ.Add(fields[91]);
                RFoot_MagX.Add(fields[92]); RFoot_MagY.Add(fields[93]); RFoot_MagZ.Add(fields[94]);
                RFoot_QuatX.Add(fields[95]); RFoot_QuatY.Add(fields[96]); RFoot_QuatZ.Add(fields[97]); RFoot_QuatW.Add(fields[98]);
            }
        }
        if (savePDMData)
        {
            PDM_DATA_Size = PDM_Data.Count;

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
            AngVelocity_P_Front  = GetColumns(PDM_Data, 58);
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
            

            /*Debug.LogError("Time_PDM_COUNT: " + Time_PDM.Length);
            Debug.LogError("QuatP_PDM_COUNT: " + Quat_P_X.Length);
            Debug.LogError("AccW_P_X_PDM_COUNT: " + AccW_P_X.Length);
            Debug.LogError("PDM_COUNT: " + PDM_Data.Count);
            Debug.LogError("Time_PDM_["+ (Time_PDM.Length - 1) + "]: " + Time_PDM[Time_PDM.Length-1]);
            Debug.LogError("RHipAngle_Sag[" + (Time_PDM.Length - 1) + "]: " + RHipAngle_Sag[Time_PDM.Length - 1]);*/
        }
        
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
        Time_RAW = Pelvis_AccX = Pelvis_AccY = Pelvis_AccZ = Pelvis_GyroX = Pelvis_GyroY = Pelvis_GyroZ = Pelvis_MagX = Pelvis_MagY = Pelvis_MagZ = Pelvis_QuatX = Pelvis_QuatY = Pelvis_QuatZ = Pelvis_QuatW = LThigh_AccX = LThigh_AccY = LThigh_AccZ = LThigh_GyroX = LThigh_GyroY = LThigh_GyroZ = LThigh_MagX = LThigh_MagY = LThigh_MagZ = LThigh_QuatX = LThigh_QuatY = LThigh_QuatZ = LThigh_QuatW = LCalf_AccX = LCalf_AccY = LCalf_AccZ = LCalf_GyroX = LCalf_GyroY = LCalf_GyroZ = LCalf_MagX = LCalf_MagY = LCalf_MagZ = LCalf_QuatX = LCalf_QuatY = LCalf_QuatZ = LCalf_QuatW = LFoot_AccX = LFoot_AccY = LFoot_AccZ = LFoot_GyroX = LFoot_GyroY = LFoot_GyroZ = LFoot_MagX = LFoot_MagY = LFoot_MagZ = LFoot_QuatX = LFoot_QuatY = LFoot_QuatZ = LFoot_QuatW = RThigh_AccX = RThigh_AccY = RThigh_AccZ = RThigh_GyroX = RThigh_GyroY = RThigh_GyroZ = RThigh_MagX = RThigh_MagY = RThigh_MagZ = RThigh_QuatX = RThigh_QuatY = RThigh_QuatZ = RThigh_QuatW = RCalf_AccX = RCalf_AccY = RCalf_AccZ = RCalf_GyroX = RCalf_GyroY = RCalf_GyroZ = RCalf_MagX = RCalf_MagY = RCalf_MagZ = RCalf_QuatX = RCalf_QuatY = RCalf_QuatZ = RCalf_QuatW = RFoot_AccX = RFoot_AccY = RFoot_AccZ = RFoot_GyroX = RFoot_GyroY = RFoot_GyroZ = RFoot_MagX = RFoot_MagY = RFoot_MagZ = RFoot_QuatX = RFoot_QuatY = RFoot_QuatZ = RFoot_QuatW = new List<string>();
        Time_PDM = Quat_P_X = Quat_P_Y = Quat_P_Z = Quat_P_W = Quat_LT_X = Quat_LT_Y = Quat_LT_Z = Quat_LT_W = Quat_LC_X = Quat_LC_Y = Quat_LC_Z = Quat_LC_W = Quat_LF_X = Quat_LF_Y = Quat_LF_Z = Quat_LF_W = Quat_RT_X = Quat_RT_Y = Quat_RT_Z = Quat_RT_W = Quat_RC_X = Quat_RC_Y = Quat_RC_Z = Quat_RC_W = Quat_RF_X = Quat_RF_Y = Quat_RF_Z = Quat_RF_W = LHipAngle_Sag = RHipAngle_Sag = LKneeAngle_Sag = RKneeAngle_Sag = LAnkleAngle_Sag = RAnkleAngle_Sag = AccW_P_X = AccW_P_Y = AccW_P_Z = AccW_LT_X = AccW_LT_Y = AccW_LT_Z = AccW_LC_X = AccW_LC_Y = AccW_LC_Z = AccW_LF_X = AccW_LF_Y = AccW_LF_Z = AccW_RT_X = AccW_RT_Y = AccW_RT_Z = AccW_RC_X = AccW_RC_Y = AccW_RC_Z = AccW_RF_X = AccW_RF_Y = AccW_RF_Z = AngVelocity_P_Sag = AngVelocity_P_Trans = AngVelocity_P_Front = AngVelocity_LT_Sag = AngVelocity_LT_Trans = AngVelocity_LT_Front = AngVelocity_LC_Sag = AngVelocity_LC_Trans = AngVelocity_LC_Front = AngVelocity_LF_Sag = AngVelocity_LF_Trans = AngVelocity_LF_Front = AngVelocity_RT_Sag = AngVelocity_RT_Trans = AngVelocity_RT_Front = AngVelocity_RC_Sag = AngVelocity_RC_Trans = AngVelocity_RC_Front = AngVelocity_RF_Sag = AngVelocity_RF_Trans = AngVelocity_RF_Front = new float[PDM_DATA_Size-2];
    }
    public void SaveData()
    {
        if (saveRawData)
        {
            //RawDataFile = Application.persistentDataPath +"RAW_DATA_" + UsersList.patientsessioninfo.Username + ".GIMU";
            //System.IO.File.WriteAllLines(RawDataFile, Raw_Data.ToArray());
            //Debug.Log("Raw Data has been saved");
        }
        if (savePDMData)
        {
            PDMDataFile = EditorUtility.SaveFilePanel("Save Session Data","","PDM_DATA_" + UsersList.patientsessioninfo.Username + ".GIMU","GIMU");

            if (PDMDataFile.Length != 0)
            {
                System.IO.File.WriteAllLines(PDMDataFile, PDM_Data.ToArray());
                Debug.Log("PDM Data has been saved");
            }
        }
        //print("persistentDataPath: " + Application.persistentDataPath + "  DataPath: " + Application.dataPath);

        //StartCoroutine(SaveDataCoroutine());
        StartCoroutine(UploadDataCoroutine());
    }
    /*IEnumerator SaveDataCoroutine()
    {
        //OrganizeData();
        yield return StartCoroutine(UploadDataCoroutine());
    }*/
    IEnumerator UploadDataCoroutine()
    {
        print("Username: "+UsersList.patientsessioninfo.Username+", User_ID: "+ UsersList.patientsessioninfo.ID +", SesionDate: "+ UsersList.patientsessioninfo.SessionDate);

        WWWForm UploadformWWW = new WWWForm();
        UploadformWWW.AddField("ID", UsersList.patientsessioninfo.ID);
        UploadformWWW.AddField("SessionDate", UsersList.patientsessioninfo.SessionDate);


        Byte[] RawdataAsBytes = Raw_Data.SelectMany(s => Encoding.ASCII.GetBytes(s + '\0')).ToArray();
        Byte[] PDMdataAsBytes = PDM_Data.SelectMany(s => Encoding.ASCII.GetBytes(s + '\0')).ToArray();

        string RawDataAsString = Convert.ToBase64String(RawdataAsBytes); 
        string PDMDataAsString = Convert.ToBase64String(PDMdataAsBytes);

        UploadformWWW.AddField("raw_session_data", RawDataAsString);
        UploadformWWW.AddField("pdm_session_data", PDMDataAsString);
        /*
        //-------------------------------------------------------------------------------------------
        //FOR DOWNLOAD
        byte[] RawDataAs64 = Convert.FromBase64String(RawDataAsString); //Convert Downloaded BLOB to Byte arraw again
        byte[] PDMDataAs64 = Convert.FromBase64String(PDMDataAsString); //Convert Downloaded BLOB to Byte arraw again

        List<string> DownloadedRawData = new List<string>();
        for (int i = 0; i < RawDataAs64.Length; i++)
        {
            int end = i;
            while (RawDataAs64[end] != 0) // Scan for zero byte
                end++;
            var length = end - i;
            var word = new byte[length];
            Array.Copy(RawDataAs64, i, word, 0, length);
            DownloadedRawData.Add(Encoding.ASCII.GetString(word));
            i += length;
        }
        print(DownloadedRawData[0]);
        print(DownloadedRawData[1]);
        
        List<string> DownloadedPDMData = new List<string>();
        for (int i = 0; i < PDMDataAs64.Length; i++)
        {
            int end = i;
            while (PDMDataAs64[end] != 0) // Scan for zero byte
                end++;
            var length = end - i;
            var word = new byte[length];
            Array.Copy(PDMDataAs64, i, word, 0, length);
            DownloadedPDMData.Add(Encoding.ASCII.GetString(word));
            i += length;
        }
        print(DownloadedPDMData[0]);
        print(DownloadedPDMData[1]);
        //--------------------------------------------------------------------------------
       */

        WWW uploadvalidw = new WWW(Register.ServerAddress + "/uploadSessionData.php", UploadformWWW);
        yield return uploadvalidw;
        yield return StartCoroutine(UploadValidationCoroutine(uploadvalidw));
    }
    IEnumerator UploadValidationCoroutine(WWW w) //Comprueba si la respuesta del servidor fue UPLOAD-SUCCESSFUL
    {
        if (w.error == null)
        {
            Debug.LogWarning(w.text);
            if (w.text == "UPLOAD-SUCCESSFUL")
            {
                SaveDataButton.SetActive(false);
            }
            else
            {
                Debug.LogError("It wasn't uploaded");
            }
        }
        else
        {
            Debug.LogWarning("Error: " + w.error);
        }
        yield return null;
    }
}
