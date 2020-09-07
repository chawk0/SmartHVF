using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the goal for this class is to encapsulate all relevant data for a given test.
// which eye was tested, the color/background settings, type of test (24-2, for example)
// the parameters of the stimulus presentation algorithm (TBD), the associated Patient
// object, the actual stimulus field values, etc.

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
    public Patient patient;

    public TestInfo()
    { }
}
