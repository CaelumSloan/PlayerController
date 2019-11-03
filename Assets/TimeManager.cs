using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    public Timer timer;

    void Start()
    {
        timer = Timer.CreateTimer(60);
    }
}
