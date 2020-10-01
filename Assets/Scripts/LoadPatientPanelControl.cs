using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadPatientPanelControl : MonoBehaviour
{
    private Main main;
    private InputField patientNameTextInput, patientAgeTextInput;
    private Button saveButton, cancelButton;
    
    void Awake()
    {
        Debug.Log("Hello from LoadPatientPanelControl.cs");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        // get references to child UI objects
        patientNameTextInput = GameObject.Find("/Canvas/LoadPatientPanel/PatientNameTextInput").GetComponent<InputField>();
        patientAgeTextInput = GameObject.Find("/Canvas/LoadPatientPanel/PatientAgeTextInput").GetComponent<InputField>();
        saveButton = GameObject.Find("/Canvas/LoadPatientPanel/SaveButton").GetComponent<Button>();
        cancelButton = GameObject.Find("/Canvas/LoadPatientPanel/CancelButton").GetComponent<Button>();

        //saveButton

        if (main.currentPatient != null)
        {
            //
        }
        else
            disablePatientInputFields();
    }
    private void OnEnable()
    {
        Debug.Log("LoadPatientPanel enabled");
    }

    void Update()
    {
        
    }

    public void DoneButton_Click()
    {
        main.setActivePanel(UIPanel.MainMenu);
    }

    private void disablePatientInputFields()
    {
        patientNameTextInput.interactable = false;
        patientAgeTextInput.interactable = false;

    }
}
