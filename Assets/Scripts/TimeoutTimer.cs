using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to time the input timeout during the test

public class TimeoutTimer
{
    public bool timeout;
    public float time;

    private float lastTime, duration;

    public TimeoutTimer()
    {
        //
    }

    public void start(float duration)
    {
        this.duration = duration;
        this.time = 0.0f;
        this.lastTime = Time.time;
        this.timeout = false;
    }

    public void update()
    {
        float t = Time.time;
        time += (t - lastTime);
        lastTime = t;

        if (time > duration)
            timeout = true;
    }
}
