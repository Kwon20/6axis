using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ptp : MonoBehaviour {
    private int currectNode = 0;
    private List<Transform> nodes;
    public Transform path;
    public bool Tp = false;

    void Start()
    {

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

    private void Update()
    {
        Debug.Log(currectNode);
        if (currectNode >= 1)
        {
            if (Input.GetButtonDown("Joystick Button 2"))
            {
                transform.position = new Vector3(nodes[currectNode - 1].position.x, nodes[currectNode - 1].position.y + 0.5f, nodes[currectNode - 1].position.z);
                transform.rotation = nodes[currectNode - 1].rotation;
                currectNode--;
                GetComponent<SimpleCarController>().Car_Reset();
            }
        }

    }


    void OnTriggerEnter(Collider coll)
    {


        if (coll.tag == "WayPoint")
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
