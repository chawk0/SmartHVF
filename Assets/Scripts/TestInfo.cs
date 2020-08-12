using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TestType
{
    LeftEye,
    RightEye
}
public class TestInfo
{
    public TestType type;
    public int stimulusSize;
    public DateTime dateTime;
    public int duration;
    public int patientAge;

    public TestInfo()
    { }
}
