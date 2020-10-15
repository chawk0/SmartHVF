/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// this script is attached to the TestResultsPanel GUI element and is used to update the
// relevant GUI elements with test/patient info once the panel becomes active.
// the OnEnable method fetches the last test info from the global stored in Main.cs
public class TestResultsUpdater : MonoBehaviour
{
    private string formatDuration(int duration)
    {
        TimeSpan ts = TimeSpan.FromSeconds(duration);
        return string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
    }

    private string formatTestType(TestType tt)
    {
        if (tt == TestType.LeftEye)
            return "Left";
        else if (tt == TestType.RightEye)
            return "Right";
        else
            return "?";
    }

    private void OnEnable()
    {
        TestInfo ti = GameObject.Find("AppControl").GetComponent<Main>().testInfo;

        if (ti != null)
        {
            Text patientNameLabel = GameObject.Find("PatientNameLabel").GetComponent<Text>();
            Text patientAgeLabel = GameObject.Find("PatientAgeLabel").GetComponent<Text>();
            Text eyeLabel = GameObject.Find("EyeLabel").GetComponent<Text>();
            Text testDurationLabel = GameObject.Find("TestDurationLabel").GetComponent<Text>();

            patientNameLabel.text = "Patient Name: " + ti.patient.name;
            patientAgeLabel.text = "Patient Age: " + ti.patient.age;
            eyeLabel.text = "Eye: " + formatTestType(ti.type);
            testDurationLabel.text = "Test Duration: " + formatDuration(ti.duration);
        }
    }
}
*/