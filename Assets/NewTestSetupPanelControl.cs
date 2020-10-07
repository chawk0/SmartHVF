using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewTestSetupPanelControl : MonoBehaviour
{
    private Main main;
    private Toggle leftEyeToggle;
    private Dropdown stimulusSizeDropdown;

    void Awake()
    {
        //Debug.Log("Hello from NewTestSetupPanelControl.cs");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        // get references to child UI objects
        leftEyeToggle = GameObject.Find("/Canvas/NewTestSetupPanel/LeftEyeToggle").GetComponent<Toggle>();
        stimulusSizeDropdown = GameObject.Find("/Canvas/NewTestSetupPanel/StimulusSizeDropdown").GetComponent<Dropdown>();


    }

    void Update()
    {
        
    }

    public void StartTestButton_Click()
    {
        TestType t = leftEyeToggle.isOn ? TestType.LeftEye : TestType.RightEye;
        int s = stimulusSizeDropdown.value;
        Debug.Log("New test requested with eye: " + t + ", stimulus size index: " + s);
    }

    public void CancelButton_Click()
    {
        main.setActivePanel(UIPanel.MainMenu);
    }
}
