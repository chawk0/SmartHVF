using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class BrowseTestHistoryPanelControl : MonoBehaviour
{
    private Main main;

    private Text patientNameLabel;
    private Text patientAgeLabel;
    private Text eyeLabel;
    private Text testDurationLabel;
    private Text testDateTimeLabel;
    private Image testResultsImage;

    void Awake()
    {
        Debug.Log("BrowseTestHistoryPanelControl:Awake()!");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        patientNameLabel = GameObject.Find("/Canvas/TestResultsPanel/PatientNameLabel").GetComponent<Text>();
        patientAgeLabel = GameObject.Find("/Canvas/TestResultsPanel/PatientAgeLabel").GetComponent<Text>();
        eyeLabel = GameObject.Find("/Canvas/TestResultsPanel/EyeLabel").GetComponent<Text>();
        testDurationLabel = GameObject.Find("/Canvas/TestResultsPanel/TestDurationLabel").GetComponent<Text>();
        testDateTimeLabel = GameObject.Find("/Canvas/TestResultsPanel/TestDateTimeLabel").GetComponent<Text>();
        testResultsImage = GameObject.Find("/Canvas/TestResultsPanel/TestResultsImage").GetComponent<Image>();
    }

    private void OnEnable()
    {
        //
    }

    void Update()
    {
        //
    }
}
