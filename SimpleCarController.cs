using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[System.Serializable]
public class Wheel
{
    public WheelCollider wheelCollider;
    public bool
        Steering,
        Motor,
        Handbrake;
    public float DefSidewaysStiffness;
}

public class SimpleCarController : MonoBehaviour
{
    public bool isKeyboard = false;
    public int StartGearDelay = 0;
    public Wheel[] Wheels;
    public float MotorPower;
    public float SteeringAngle;
    public float BrakingTorque;
    public int accel_k, Wheel_Break_k;
    public float accel, Wheel_Break;
    public float Speed;
    public float Max_Speed = 0; //최대 제한 스피드
    Rigidbody M_Rig;

    public AudioSource shiftSource;


    public int Gear = 0; // 기어 상태
    public bool Gear_Left_Change = false;
    public bool Gear_Left_Change_k = false;
    public bool Gear_Right_Change = false;
    public bool Gear_Right_Change_k = false;

    public float Gear_Count = 0f;

    [Range(10, 200)]
    public float SteeringSpeed;

    private float SmoothSteeringInput;


    [Header("Motion")]
    public Controller Motion_Controller;

    [Header("Motion")]
    float Light_timer = 0f;
    public GameObject Front_Lights;
    public GameObject Back_Lights;

    [Header("Test UI")]
    public Text UI_Gear;
    public Text UI_Speed;

    [Header("DUO")]
    public bool DUO = true;

    [Header("Rpm")]
    public float Rpm = 1400; // 엔진 분당 회전 속도
    public float Ratio = 12.88f; // 기어비
    public float Max_Rpm = 8000;
    public AudioSource Rpm_Audio;
    void Start()
    {
        Motion_Controller = this.GetComponent<Controller>();
        M_Rig = this.transform.GetComponent<Rigidbody>();
        foreach (var wheel in Wheels)
            wheel.DefSidewaysStiffness = wheel.wheelCollider.sidewaysFriction.stiffness;
    }

    void GetSmoothSteeringInput() // 좌우 스티어링 자연스럽게 하기위한 값
    {
        SmoothSteeringInput = Mathf.MoveTowards(SmoothSteeringInput, Input.GetAxisRaw("Horizontal"), SteeringSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {
        Debug.Log(Wheels[1].wheelCollider.brakeTorque);
        Debug.Log("모터토크(업뎃시)"+Wheels[1].wheelCollider.motorTorque);
        accel = 0;
        accel_k = 0;
        Wheel_Break_k = 0;
        Speed = M_Rig.velocity.magnitude * 3.6f; // 속도 구하기
        

        UI_Speed.text = ((int)Speed).ToString();
        if (Gear == -1) UI_Gear.text = "R";
        else if (Gear == 0) UI_Gear.text = "N";
        else UI_Gear.text = Gear.ToString();


        Motion_Controller.My_Motion(this.transform.localRotation.eulerAngles.z,
            this.transform.localRotation.eulerAngles.x,
            SmoothSteeringInput * SteeringAngle,
            Speed,
            (int)Speed); // 모션 컨트롤

        Light_Check(); // 라이트 체크

        GetSmoothSteeringInput();
        foreach (var wheel in Wheels)
        {
            WheelFrictionCurve _wfc = wheel.wheelCollider.sidewaysFriction;
            WheelFrictionCurve _wff = wheel.wheelCollider.forwardFriction;
            _wfc.stiffness = wheel.DefSidewaysStiffness;
            _wff.stiffness = wheel.DefSidewaysStiffness;


            Rpm_Audio.pitch = 1 + ((Rpm - 1400) * 0.0002f);
            Fix_Input_Value(); // 인풋 벨류 사용할수 있는 값으로 수정
            

            if (Rpm <= 1400) // 기본 알피엠 1400이하로 떨어지면
            {
                Rpm += Ratio;
            }
            else // 박으면 알피엠 내리기
            {
                if(isKeyboard)
                {
                    if (accel_k == 1) //엑셀 밟고 있는 상태이면
                    {
                        if (Gear == -1) // 후진
                        {
                            if (Speed <= Max_Speed)
                            {
                                wheel.wheelCollider.motorTorque = -((MotorPower * 5) * (Rpm * 0.000125f));
                                wheel.wheelCollider.brakeTorque = 0;
                            }
                            else
                            {
                                wheel.wheelCollider.motorTorque = 0;
                                wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            }
                        }
                        else if (Gear != 0)
                        {
                            wheel.wheelCollider.brakeTorque = 0;  //  
                            if (Speed <= Max_Speed)
                            {
                                if (accel_k == 1)
                                {
                                    wheel.wheelCollider.motorTorque = ((MotorPower) * (6 - (Gear))) * (Rpm * 0.000125f);
                                    
                                }
                                else
                                {
                                    wheel.wheelCollider.motorTorque = 0;
                                    wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                                }
                            }
                            else
                            {
                                wheel.wheelCollider.motorTorque = 0;
                                wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            }
                        }
                        else // Gear == 0
                        {
                            wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            wheel.wheelCollider.motorTorque = 0;
                        }

                        if (Rpm < Max_Rpm)
                        {                       
                                Rpm += Ratio + (Ratio * accel_k * 2);                          
                        }
                    }
                    Rpm -= Ratio * 2f; // 떨어질때는 오른거 두배로떨어짐
                }
                else
                {
                    if (accel >= 0) //엑셀 밟고 있는 상태이면
                    {
                        if (Gear == -1) // 후진
                        {
                            if (Speed <= Max_Speed)
                            {
                                wheel.wheelCollider.motorTorque = -((MotorPower * 5) * (Rpm * 0.000125f));
                                wheel.wheelCollider.brakeTorque = 0;
                            }
                            else
                            {
                                wheel.wheelCollider.motorTorque = 0;
                                wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            }
                        }
                        else if (Gear != 0)
                        {
                            wheel.wheelCollider.brakeTorque = 0;  //  
                            if (Speed <= Max_Speed)
                            {
                                if (accel != 0)
                                {
                                    wheel.wheelCollider.motorTorque = ((MotorPower) * (6 - (Gear))) * (Rpm * 0.000125f);
                                    //Debug.Log
                                }
                                else
                                {
                                    wheel.wheelCollider.motorTorque = 0;
                                    wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                                }
                            }
                            else
                            {
                                wheel.wheelCollider.motorTorque = 0;
                                wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            }
                        }
                        else // Gear == 0
                        {
                            wheel.wheelCollider.brakeTorque = BrakingTorque * 2.5f;
                            wheel.wheelCollider.motorTorque = 0;
                        }

                        if (Rpm < Max_Rpm)
                        {                            
                                Rpm += Ratio + (Ratio * accel);
                        }
                    }
                    Rpm -= Ratio * 2f; // 떨어질때는 오른거 두배로떨어짐
                }
            }



            if (wheel.Motor && wheel.wheelCollider != null) // 전진 후진
            {
                Debug.Log("실행중");
               if(isKeyboard)
                {
                    if (accel_k == 1) // 엑셀 밟고 있을때
                    {
                        Debug.Log("실행중2");
                        if (Speed <= Max_Speed)
                        {
                            if (Gear == -1 && Speed < 10)
                            {
                                wheel.wheelCollider.motorTorque = -((float)accel_k * MotorPower * 2f);
                            }
                            else if (Gear_Speed_Cheak())
                            {
                                Debug.Log("실행중3");
                                wheel.wheelCollider.motorTorque = (float)accel_k * MotorPower;
                                Debug.Log("모터토크 : "+wheel.wheelCollider.motorTorque);
                            }
                        }
                        else
                        {
                            wheel.wheelCollider.motorTorque = 0; // 속도 더이상 올리지 않음
                            wheel.wheelCollider.brakeTorque = BrakingTorque / 10;
                        }
                    }
                }
                else
                {
                    if (accel > 0) // 엑셀 밟고 있을때
                    {
                        if (Speed <= Max_Speed)
                        {
                            if (Gear == -1 && Speed < 10)
                            {
                                wheel.wheelCollider.motorTorque = -(accel * MotorPower * 2f);
                            }
                            else if (Gear_Speed_Cheak())
                            {
                                wheel.wheelCollider.motorTorque = accel * MotorPower;
                            }
                        }
                        else
                        {
                            wheel.wheelCollider.motorTorque = 0; // 속도 더이상 올리지 않음
                            wheel.wheelCollider.brakeTorque = BrakingTorque / 10;
                        }
                    }
                }
            }

            if (wheel.Steering && wheel.wheelCollider != null) // 좌우 스티어링
                wheel.wheelCollider.steerAngle = SmoothSteeringInput * SteeringAngle;




           // 밑에 이걸 어떻게 해야될까....
            if (Mathf.Sign(Input.GetAxis("Joystick Axis 3")) != Mathf.Sign(wheel.wheelCollider.rpm) && Mathf.Abs(wheel.wheelCollider.rpm) > 20
                && Input.GetAxis("Joystick Axis 3") != 0 /*&& Input.GetKey(KeyCode.UpArrow) == false*/) // 입력값이 없다면,현재 움직인다면 저항값
            {
                wheel.wheelCollider.brakeTorque = BrakingTorque; // 브레이크
            }
            else wheel.wheelCollider.brakeTorque = 0; // 현재 움직이고 있다면

            if(isKeyboard)
            {
                if (Wheel_Break_k > 0) //브레이크 밟을때
                {
                    Back_Lights.SetActive(true);
                    wheel.wheelCollider.motorTorque = 0;

                    wheel.wheelCollider.brakeTorque = (BrakingTorque * Wheel_Break_k) * 1.5f;
                    _wff.stiffness = 1 + Wheel_Break_k;

                }
                else
                {
                    Back_Lights.SetActive(false);
                    _wff.stiffness = wheel.DefSidewaysStiffness;
                }
            }
            else
            {
                if (Wheel_Break > 0) //브레이크 밟을때
                {
                    Back_Lights.SetActive(true);
                    wheel.wheelCollider.motorTorque = 0;

                    wheel.wheelCollider.brakeTorque = (BrakingTorque * Wheel_Break) * 1.5f;
                    _wff.stiffness = 1 + Wheel_Break;

                }
                else
                {
                    Back_Lights.SetActive(false);
                    _wff.stiffness = wheel.DefSidewaysStiffness;
                }
            }
            if (wheel.Handbrake)
                if (Input.GetKey(KeyCode.Space))
                {
                    wheel.wheelCollider.brakeTorque = 1000;
                    _wfc.stiffness = 0.7f;
                }

            wheel.wheelCollider.sidewaysFriction = _wfc;
            wheel.wheelCollider.forwardFriction = _wff;

        }
        Gear_Update(); // 기어 업데이트
        Debug.Log("모터토크(업뎃끝날시)" + Wheels[1].wheelCollider.motorTorque);
    }

    void Gear_Update() // 기어 세팅
    {
        Gear_Count += Time.deltaTime;

        Gear_Right_Change = Input.GetButton("Joystick Button 5");// 기어 상승시
        Gear_Right_Change_k = Input.GetKey(KeyCode.X);
        if ((Gear_Right_Change == true || Gear_Right_Change_k)&& Gear_Count > 0.5f )
        {
            if (Gear == -1)
            {
                Gear = 0;
                Rpm = 1400;
                Gear_Change_Sound();
            }
            else if (Gear == 0)
            {
                Gear = 1;
                Rpm = 1400;
                Gear_Change_Sound();
            }
            else if (Gear == 1)
            {
                Gear = 2;
                Rpm *= 0.65f;
                Gear_Change_Sound();
            }
            else if (Gear == 2)
            {
                Gear = 3;
                Rpm *= 0.65f;
                Gear_Change_Sound();
            }
            else if (Gear == 3)
            {
                Gear = 4;
                Rpm *= 0.65f;
                Gear_Change_Sound();
            }
            else if (Gear == 4)
            {
                Gear = 5;
                Rpm *= 0.65f;
                Gear_Change_Sound();
            }
            else if (Gear == 5)
            {

            }
            Gear_Right_Change = false;
            Gear_Count = 0;
        }

        Gear_Left_Change = Input.GetButton("Joystick Button 6"); // 기어 하강시
        Gear_Left_Change_k = Input.GetKey(KeyCode.Z);
        if ((Gear_Left_Change == true || Gear_Left_Change_k) && Gear_Count > 0.5f )
        {
            if (Gear == -1)
            {

            }
            else if (Gear == 0)
            {
                Gear = -1;
                Rpm = 1400;
                Gear_Change_Sound();
            }
            else if (Gear == 1)
            {
                Gear = 0;
                Rpm *= 1.2f;
                Gear_Change_Sound();
            }
            else if (Gear == 2)
            {
                Gear = 1;
                Rpm *= 1.2f;
                Gear_Change_Sound();
            }
            else if (Gear == 3)
            {
                Gear = 2;
                Rpm *= 1.2f;
                Gear_Change_Sound();
            }
            else if (Gear == 4)
            {
                Gear = 3;
                Rpm *= 1.2f;
                Gear_Change_Sound();
            }
            else if (Gear == 5)
            {
                Gear = 4;
                Rpm *= 1.2f;
                Gear_Change_Sound();
            }
            Gear_Left_Change = false;
            Gear_Count = 0;
        }

        //기어비에 따라 최대속도 설정
        if (StartGearDelay == 1)
        {
            if (Gear == -1)
            {
                Max_Speed = 20;
                Ratio = 28.88f;
            }
            else if (Gear == 0)
            {
                Max_Speed = 0;
                Ratio = 68.26f;
            }
            else if (Gear == 1)
            {
                Max_Speed = 35;
                Ratio = 12.88f;
            }
            else if (Gear == 2)
            {
                Max_Speed = 65;
                Ratio = 6.32f;
            }
            else if (Gear == 3)
            {
                Max_Speed = 95;
                Ratio = 4.75f;
            }
            else if (Gear == 4)
            {
                Max_Speed = 135;
                Ratio = 3.56f;
            }
            else if (Gear == 5)
            {
                Max_Speed = 160;
                Ratio = 2.83f;
            }
        }
        
    }

    bool Gear_Speed_Cheak() // 기어가 내려갔을때 속도 체크
    {
        if (Gear == -1) { return true; }
        else if (Gear == 0) { if (Speed > 0) { return true; } }
        else if (Gear == 1) { if (Speed > 0) { return true; } }
        else if (Gear == 2) { if (Speed > 25) { return true; } }
        else if (Gear == 3) { if (Speed > 60) { return true; } }
        else if (Gear == 4) { if (Speed > 80) { return true; } }
        else if (Gear == 5) { if (Speed > 100) { return true; } }

        return false;
    }
    void Fix_Input_Value() // 인풋값 픽스
    {
       if(isKeyboard)
        {
            if (Input.GetKey(KeyCode.UpArrow))
                accel_k = 1;

            if (Input.GetKey(KeyCode.DownArrow))
                Wheel_Break_k = 1;
        }
        else
        {
            accel = Input.GetAxis("Joystick Axis 3"); // 엑셀
            Wheel_Break = Input.GetAxis("Joystick Axis 4"); // 브레이크
            if (accel < 0) // 기본 엑셀값이 음수 인경우
            {
                accel = accel + 1f;
            }
            else // 엑셀이 양수인경우
            {
                accel = accel + 1f;
            }
            if (Wheel_Break < 0)
            {
                Wheel_Break = Wheel_Break + 1f;
            }
            else
            {
                Wheel_Break = Wheel_Break + 1f;
            }
        }
    }
    void Light_Check()
    {
        Light_timer += Time.deltaTime;

        if (Light_timer > 0.1f)
        {
            if (Input.GetButton("Joystick Button 1"))
            {
               // Front_Lights.SetActive(!Front_Lights.active);
            }
            Light_timer = 0f;
        }
    }
    void Gear_Change_Sound()                        //귀여운 두영이 기어변속 사운드
    {
        shiftSource.Play();
        if (DUO == true)
        {
          //  GetComponent<Car_Audio>().sound.Stop();
           // GetComponent<Car_Audio>().Sound_stop = true;
        }
    }

    public void Car_Reset()
    {
        foreach (var wheel in Wheels)
        {
            wheel.wheelCollider.motorTorque = 0f;
            this.M_Rig.velocity = new Vector3(0, 0, 0);
        }
    }
}
