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
        patientNameLabel = GameObject.Find("/Canvas/BrowseTestHistoryPanel/PatientNameLabel").GetComponent<Text>();
        patientAgeLabel = GameObject.Find("/Canvas/BrowseTestHistoryPanel/PatientAgeLabel").GetComponent<Text>();
        eyeLabel = GameObject.Find("/Canvas/BrowseTestHistoryPanel/EyeLabel").GetComponent<Text>();
        testDurationLabel = GameObject.Find("/Canvas/BrowseTestHistoryPanel/TestDurationLabel").GetComponent<Text>();
        testDateTimeLabel = GameObject.Find("/Canvas/BrowseTestHistoryPanel/TestDateTimeLabel").GetComponent<Text>();
        testResultsImage = GameObject.Find("/Canvas/BrowseTestHistoryPanel/TestResultsImage").GetComponent<Image>();
    }

    private void OnEnable()
    {
        Debug.Log("BrowseTestHistoryPanelControl:OnEnable()!");

        if (main.currentPatient == null)
            Debug.Log("BrowseTestHistoryPanel accessed with null patient!");
        else
        {
            patientNameLabel.text = "Patient Name: " + main.currentPatient.name;
            patientAgeLabel.text = "Patient Age: " + main.currentPatient.age;
        }
    }

    void Update()
    {
        //
    }

    public void BackButton_Click()
    {
        main.setActivePanel(UIPanel.MainMenu);
    }
}
