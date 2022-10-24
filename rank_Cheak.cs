using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rank_Cheak : MonoBehaviour {

    public GameObject[] Cars;
    private int Max_state=0;
    private GameObject tmp;
    private int index;
    public GameObject Path;
 
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		for(int i=0;i<6;i++)
        {
            Max_state = Cars[i].GetComponent<PlayCtl>().state;
            index = i;
            for(int j=i+1;j<6;j++)
            {
                if(Max_state<Cars[j].GetComponent<PlayCtl>().state)
                {
                    Max_state = Cars[j].GetComponent<PlayCtl>().state;
                    index = j;
                }
                else if(Max_state== Cars[j].GetComponent<PlayCtl>().state)
                {
                    //다음 노드까지의 거리계산
                    float dis1=Vector3.Distance(Cars[index].transform.position, Path.transform.GetChild((Max_state-Cars[index].GetComponent<PlayCtl>().Lap*100)%63).position);
                    float dis2 =Vector3.Distance(Cars[j].transform.position, Path.transform.GetChild((Max_state - Cars[j].GetComponent<PlayCtl>().Lap * 100) % 63).position);
                    if(dis1>dis2)
                    {
                        Max_state = Cars[j].GetComponent<PlayCtl>().state;
                        index = j;
                    }
                }
            }
            tmp = Cars[i];
            Cars[i] = Cars[index];
            Cars[index] = tmp;
        }
        for(int i=0;i<6;i++)
        {
            Cars[i].GetComponent<PlayCtl>().rank = i + 1;
        }
	}
}
