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
    private Image testResultsImage;

    void Awake()
    {
        //Debug.Log("Hello from TestResultsPanelControl.cs");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        testResultsImage = GameObject.Find("/Canvas/TestResultsPanel/TestResultsImage").GetComponent<Image>();

        if (lastTest == null)
            Debug.Log("lastTest is null");
        else
        {
            Debug.Log("lastTest is not null!");
            testResultsImage.sprite = Sprite.Create(lastTest.eyeMap, new Rect(0, 0, lastTest.eyeMap.width, lastTest.eyeMap.height), Vector2.zero);
        }
    }

    void Update()
    {

    }

    public void SaveButton_Click()
    {
        Debug.Log("Save results requested...");
    }

    public void BackButton_Click()
    {
        lastTest = null;
        main.setActivePanel(UIPanel.NewTestSetup);
    }
}
