using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeoutTimer
{
    public bool timeout;

    private float startTime;
    private float duration;

    public TimeoutTimer()
    {
        //
    }

    public void start(float duration)
    {
        this.duration = duration;
        this.startTime = Time.time;
        timeout = false;
    }

    public void update()
    {
        if ((Time.time - startTime) > duration)
            timeout = true;
    }
}
