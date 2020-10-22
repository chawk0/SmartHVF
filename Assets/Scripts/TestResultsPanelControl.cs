using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class TestResultsPanelControl : MonoBehaviour
{
    private Main main;
    public TestInfo lastTest = null;

    private Text patientNameLabel;
    private Text patientAgeLabel;
    private Text eyeLabel;
    private Text testDurationLabel;
    private Text testDateTimeLabel;
    private Image testResultsImage;
    private Text saveConfirmationLabel;
    private TimeoutTimer fadeTimer;

    void Awake()
    {
        Debug.Log("TestResultsPanelControl:Awake()!");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        patientNameLabel = GameObject.Find("/Canvas/TestResultsPanel/PatientNameLabel").GetComponent<Text>();
        patientAgeLabel = GameObject.Find("/Canvas/TestResultsPanel/PatientAgeLabel").GetComponent<Text>();
        eyeLabel = GameObject.Find("/Canvas/TestResultsPanel/EyeLabel").GetComponent<Text>();
        testDurationLabel = GameObject.Find("/Canvas/TestResultsPanel/TestDurationLabel").GetComponent<Text>();
        testDateTimeLabel = GameObject.Find("/Canvas/TestResultsPanel/TestDateTimeLabel").GetComponent<Text>();
        testResultsImage = GameObject.Find("/Canvas/TestResultsPanel/TestResultsImage").GetComponent<Image>();
        saveConfirmationLabel = GameObject.Find("/Canvas/TestResultsPanel/SaveConfirmationLabel").GetComponent<Text>();

        saveConfirmationLabel.gameObject.SetActive(false);
        fadeTimer = new TimeoutTimer();
    }

    private void OnEnable()
    {
        Debug.Log("TestResultsPanelControl:OnEnable()!");

        if (lastTest == null || main.currentPatient == null)
        {
            if (lastTest == null)
                Debug.Log("lastTest is null! can't update TestResultsPanel");
            else if (main.currentPatient == null)
                Debug.Log("main.currentPatient is null! can't update TestResultsPanel");
        }
        else
        {
            testResultsImage.sprite = Sprite.Create(lastTest.eyeMap, new Rect(0, 0, lastTest.eyeMap.width, lastTest.eyeMap.height), Vector2.zero);

            patientNameLabel.text = "Patient Name: " + main.currentPatient.name;
            patientAgeLabel.text = "Patient Age: " + main.currentPatient.age;
            eyeLabel.text = lastTest.type == TestType.LeftEye ? "Eye: Left" : "Eye: Right";
            TimeSpan d = new TimeSpan(0, 0, lastTest.duration);
            testDurationLabel.text = "Test Duration: " + d.ToString("g");
            testDateTimeLabel.text = "Test Date: " + lastTest.dateTime.ToString("yyyy-MMM-dd HH:mm:ss");
        }
    }

    void Update()
    {
        if (saveConfirmationLabel.gameObject.activeSelf)
        {
            fadeTimer.update();

            if (fadeTimer.time >= 4.0f)
                saveConfirmationLabel.gameObject.SetActive(false);
            else if (fadeTimer.time >= 2.0f)
                saveConfirmationLabel.color = new Color(saveConfirmationLabel.color.r, saveConfirmationLabel.color.g, saveConfirmationLabel.color.b, 1.0f - (fadeTimer.time - 2.0f) / 2.0f);
        }
    }

    public void SaveButton_Click()
    {
        Debug.Log("Save results requested...");

        lastTest.testSave();
        saveConfirmationLabel.gameObject.SetActive(true);
        fadeTimer.start(2.0f);
    }

    public void BackButton_Click()
    {
        lastTest = null;
        main.setActivePanel(UIPanel.NewTestSetup);
    }
}
