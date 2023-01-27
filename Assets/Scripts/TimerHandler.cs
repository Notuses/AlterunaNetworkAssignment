using System;
using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine;

public class TimerHandler : AttributesSync
{
    public Dictionary<Timer, string> timerDict;
    private List<Timer> timerList;
    
    public void CreateTimer(string name)
    {
        Timer timer = new Timer();
        timerDict.Add(timer, name);
        timerList.Add(timer);
    }

    void Update()
    {
        /*
        foreach (var timer in timerList)
            timer.Update();
        */
    }
}
