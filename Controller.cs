using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Controller : MonoBehaviour {

    [DllImport("AvSimDllMotionExternC")]
    private static extern int MotionControl__Initial();
    [DllImport("AvSimDllMotionExternC")]
    private static extern int MotionControl__Destroy();
    [DllImport("AvSimDllMotionExternC")]
    private static extern void MotionControl__DOF_and_Blower(int nRoll, int nPitch, int nYaw, int nSway, int nSurge, int nHeave, int nSpeed, int nBlower);

    [Range(0, 20000)]
    public int nRoll = 10000;
    [Range(0, 20000)]
    public int nPitch = 10000;
    [Range(0, 20000)]
    public int nYaw = 10000;
    [Range(0, 20000)]
    public int nSway = 10000;
    [Range(0, 20000)]
    public int nSurge = 10000;
    [Range(0, 20000)]
    public int nHeave = 10000;
    [Range(0, 255)]
    public int nSpeed = 10;
    [Range(0, 100)]
    public int nBlower = 0;


    private void Awake()
    {
       
    }

    // Use this for initialization
    void Start () {
        MotionControl__Initial();
    }
	
	// Update is called once per frame
    void Update() {
        
    }

    void OnDestroy()
    {
        MotionControl__Destroy();
    }

    public void My_Motion(float Roll, float Pitch, float Yaw, float Surge, float Blower)
    {
        if (Roll > 180)
        {
            float tmp = 360 - Roll;

            Roll = -Mathf.Abs(tmp);
        }
        if (Pitch > 180)
        {
            float tmp = 360 - Pitch;

            Pitch = -Mathf.Abs(tmp);
        }

        MotionControl__DOF_and_Blower(10000 - (int)(Roll * 300),
            10000 - (int)(Pitch * 200),
            10000 + (int)(Yaw * 300),
            nSway,
            10000 - (int)(Surge * 30),
            nHeave,
            nSpeed,
            (int)Blower);
       // Debug.Log((10000 - (int)(Surge * 50)).ToString());
    }
}
