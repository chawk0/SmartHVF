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
        TestType tt = leftEyeToggle.isOn ? TestType.LeftEye : TestType.RightEye;
        int s = stimulusSizeDropdown.value;
        Debug.Log("New test requested with eye: " + tt + ", stimulus size index: " + s);

        TestInfo ti = new TestInfo(tt, main.currentPatient, main.mainCamera.orthographicSize, (GoldmannSize)s);

        //StartCoroutine(main.fieldTest2(ti));
        main.startTest(ti);
    }

    public void CancelButton_Click()
    {
        main.setActivePanel(UIPanel.MainMenu);
    }
}
