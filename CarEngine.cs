using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarEngine : MonoBehaviour
{


    public Transform path;
    public float maxSteerAngle = 40f;
    public float TurnSpeed = 3f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public float maxMotorTorque = 80f;
    public float maxBreakTorque = 150f;
    public float currentSpeed;
    public float maxSpeed = 100f;
    public Vector3 CenterOfMass;

    [Header("CarControl")]
    public bool isBreaking = false;
    public int Count = 0;
    public int FlashTime = 0;
    public bool Flash = false;
    //public bool BackDrive = false;
    public float Timer=0;
    private List<Transform> nodes;
    private bool avoiding = false;

    [HideInInspector] [Header("Node")]
    private int currectNode = 0;
    //private int PreviousNode;
    //private int NextNode = 1;

    [Header("sensers")]
    public float sensorLength = 3f;
    public float frontSensorPosition = 0.5f;
    public float frontSideSensorPosition = 0.2f;
    public float frontSensorAngle = 30f;
    private float targetSteerAngle = 0;

    public BackDriving Back;
    public Rigidbody Rig;
    void Start()
    {
        
        Rig = GetComponent<Rigidbody>();
        Rig.centerOfMass = CenterOfMass;
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        //PreviousNode = nodes.Count - 1;

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        Timer += Time.deltaTime;
        //Sensors();
        ApplySteer();
        if (Timer > 3)
        {
            Drive();
            Breaking();
        }
        Teleport();
        CheckWaypointDistance();
        LerpToSteerAngle();
        BackDriving();
//        Debug.Log(currectNode);
    }

    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        //sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos.z += frontSensorPosition;
        float avoidMultiplier = 0;
        avoiding = false;



        // front right sensor
        sensorStartPos.x += frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 1f;
            }
        }

        // front right angle sensor
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 0.5f;
            }
        }

        // front left sensor
        sensorStartPos.x -= 2 * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 1f;
            }
        }

        // front left angle sensor
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 0.5f;
            }
        }

        // front centor sensor
        if (avoidMultiplier == 0)
        {
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
            {
                if (!hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    if (hit.normal.x < 0)
                    {
                        avoidMultiplier = -1;
                    }
                    else
                    {
                        avoidMultiplier = 1;
                    }
                }
            }
        }

        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidMultiplier;
            Debug.Log("안해");
        }
    }

    private void ApplySteer()
    {
        if (avoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currectNode].position);
        float newSteer = relativeVector.x / relativeVector.magnitude * maxSteerAngle * 0.3f;
        targetSteerAngle = newSteer;
    }

    private void Drive()
    {
        currentSpeed = Rig.velocity.magnitude * 3.6f;


        if (currentSpeed < maxSpeed && !isBreaking)
        {
            wheelFL.motorTorque = maxMotorTorque;
            wheelFR.motorTorque = maxMotorTorque;
        }
        else
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
    }

    private void Teleport()
    {
        if (currentSpeed < 2f)
            Count++;

        if (Count > 200)
        {
            //Debug.Log(transform.rotation);
            transform.position = new Vector3(nodes[currectNode].position.x, nodes[currectNode].position.y + 1.8f, nodes[currectNode].position.z);
            transform.rotation = nodes[currectNode].rotation;

            Flash = true;
            
            //Debug.Log(transform.rotation);
            wheelFL.motorTorque = maxMotorTorque;
            wheelFR.motorTorque = maxMotorTorque;
            isBreaking = false;
            currentSpeed = 60f;
            Count = 0;
        }

        if (Flash)
        {
            GetComponentInChildren<MeshRenderer>().enabled = false;
            FlashTime++;
            if (FlashTime % 2 == 0)
            {
                for (int i = 0; i <= 4; i++)
                {
                    this.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
                }

                for (int i = 0; i <= 7; i++)
                {
                    this.transform.GetChild(0).GetChild(5).GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                    this.transform.GetChild(0).GetChild(6).GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                    this.transform.GetChild(0).GetChild(7).GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                    this.transform.GetChild(0).GetChild(8).GetChild(i).GetComponent<MeshRenderer>().enabled = false;
                }

                this.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                for (int i = 0; i <= 4; i++)
                {
                    this.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
                }

                for (int i = 0; i <= 7; i++)
                {
                    this.transform.GetChild(0).GetChild(5).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                    this.transform.GetChild(0).GetChild(6).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                    this.transform.GetChild(0).GetChild(7).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                    this.transform.GetChild(0).GetChild(8).GetChild(i).GetComponent<MeshRenderer>().enabled = true;
                }
                this.transform.GetChild(1).gameObject.SetActive(true);
            }

            if (FlashTime > 60)
            {
                Flash = false;
                FlashTime = 0;
                this.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                this.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currectNode].position) < 15f && Vector3.Distance(transform.position, nodes[currectNode].position) >1.1f)
        {
            isBreaking = true;
        }
        else if(Vector3.Distance(transform.position, nodes[currectNode].position) > 1.0f)
        {
            isBreaking = false;
        }

        //if (Vector3.Distance(transform.position, nodes[currectNode].position) > 1)

        //if (Vector3.Distance(transform.position, nodes[currectNode].position) < 1.0f)
        //{
        //    if (currectNode == nodes.Count - 1)
        //    {
        //        currectNode = 0;
        //    }
        //    else
        //    {
        //        currectNode++;
        //    }
        //}
    }

    private void Breaking()
    {
        if (isBreaking == true && currentSpeed > 60f)
        {
            //Debug.Log("브레이크");
            wheelRL.brakeTorque = maxBreakTorque;
            wheelRR.brakeTorque = maxBreakTorque;
        }
        else
        {
            
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            wheelRL.motorTorque = maxMotorTorque;
            wheelRR.motorTorque = maxMotorTorque;
        }
    }

    private void LerpToSteerAngle()
    {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * TurnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * TurnSpeed);

    }

    private void BackDriving()
    {
        if(Back.BackDrive)
        {
            wheelFL.motorTorque = -maxMotorTorque * 1.5f;
            wheelFR.motorTorque = -maxMotorTorque * 1.5f;
            Back.Count++;
            if(Back.Count > 250)
            {
                Back.BackDrive = false;
                Back.Count = 0;
            }
        }

    }

    //void oncollsionEnter(Collider other)
    //{
    //    BackDrive = true;
    //}

     void OnTriggerEnter(Collider coll)
    {


        if(coll.tag == "WayPoint")
        {
            
                if (currectNode == nodes.Count - 1)
                {
                    currectNode = 0;
                }
                else
                {
                    currectNode++;
                }
        }
    }


}