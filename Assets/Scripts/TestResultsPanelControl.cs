using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResultsPanelControl : MonoBehaviour
{
    private Main main;

    void Awake()
    {
        //Debug.Log("Hello from TestResultsPanelControl.cs");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();


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
        main.setActivePanel(UIPanel.NewTestSetup);
    }
}
