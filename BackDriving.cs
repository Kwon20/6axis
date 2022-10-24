using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackDriving : MonoBehaviour {

    public int Count = 0;
    public bool BackDrive = false;

    public CarEngine carEngine;

    void Update()
    {
        BackDriveTurnOn();
    }

    private void BackDriveTurnOn()
    {
        if(carEngine.currentSpeed < 1f)
        {
            Count++;
        }

        if(Count > 10)
        {
            BackDrive = true;
        }
    }
}
