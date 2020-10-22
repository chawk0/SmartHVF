using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadPatientPanelControl : MonoBehaviour
{
    private Main main;
    private InputField patientDataFilePathTextInput;
    private InputField patientNameTextInput, patientAgeTextInput, patientGUIDText;
    private Button newPatientButton, saveButton, cancelButton, browseButton;
    private GameObject enterPatientInfoLabelObject;
 
    void Awake()
    {
        //Debug.Log("Hello from LoadPatientPanelControl.cs");

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        // get references to child UI objects
        patientDataFilePathTextInput = GameObject.Find("/Canvas/LoadPatientPanel/PatientDataFilePathTextInput").GetComponent<InputField>();
        patientNameTextInput = GameObject.Find("/Canvas/LoadPatientPanel/PatientNameTextInput").GetComponent<InputField>();
        patientAgeTextInput = GameObject.Find("/Canvas/LoadPatientPanel/PatientAgeTextInput").GetComponent<InputField>();
        patientGUIDText = GameObject.Find("/Canvas/LoadPatientPanel/PatientGUIDText").GetComponent<InputField>();
        browseButton = GameObject.Find("/Canvas/LoadPatientPanel/BrowseButton").GetComponent<Button>();
        newPatientButton = GameObject.Find("/Canvas/LoadPatientPanel/NewPatientButton").GetComponent<Button>();
        saveButton = GameObject.Find("/Canvas/LoadPatientPanel/SaveButton").GetComponent<Button>();
        cancelButton = GameObject.Find("/Canvas/LoadPatientPanel/CancelButton").GetComponent<Button>();
        enterPatientInfoLabelObject = GameObject.Find("/Canvas/LoadPatientPanel/EnterPatientInfoLabel");

        GameObject.Find("/Canvas/LoadPatientPanel/SaveButton").SetActive(false);
        GameObject.Find("/Canvas/LoadPatientPanel/CancelButton").SetActive(false);

        disableNewPatientMode();

        if (main.currentPatient != null)
        {
            patientNameTextInput.readOnly = true;
            patientAgeTextInput.readOnly = true;
            enterPatientInfoLabelObject.SetActive(false);

        }
    }
    void OnEnable()
    {
        Debug.Log("LoadPatientPanel enabled");
    }

    void Update()
    {
        
    }

    public void BrowseButton_Click()
    {
        FileBrowser.ShowLoadDialog(OnBrowseSuccess, OnBrowseCancel, true, false, Application.persistentDataPath, "Load patient file");
    }

    private void OnBrowseSuccess(string[] paths)
    {
        Debug.Log("string[] paths length: " + paths.Length + ", paths[0]: " + paths[0]);

        main.currentPatient = Patient.readFromDirectory(paths[0]);
        setPatientDataFields(main.currentPatient);
        patientDataFilePathTextInput.text = main.currentPatient.patientID + ".xml";
    }

    private void OnBrowseCancel()
    {
        Debug.Log("FileBrowser.ShowLoadDialog canceled");
    }

    public void NewPatientButton_Click()
    {
        setPatientDataFields(null);
        enableNewPatientMode();
        patientGUIDText.text = Guid.NewGuid().ToString();
    }

    public void SaveButton_Click()
    {
        Patient newPatient = new Patient(patientNameTextInput.text, int.Parse(patientAgeTextInput.text), patientGUIDText.text);
        main.currentPatient = newPatient;
        newPatient.saveToFile();

        patientDataFilePathTextInput.text = newPatient.patientID + ".xml";
        disableNewPatientMode();
    }

    public void CancelButton_Click()
    {
        setPatientDataFields(main.currentPatient);
        disableNewPatientMode();
    }

    public void DoneButton_Click()
    {
        main.setActivePanel(UIPanel.MainMenu);
    }

    private void enableNewPatientMode()
    {
        enterPatientInfoLabelObject.SetActive(true);
        patientDataFilePathTextInput.interactable = false;
        browseButton.interactable = false;
        newPatientButton.interactable = false;

        patientNameTextInput.interactable = true;
        patientAgeTextInput.interactable = true;
        patientGUIDText.interactable = true;

        saveButton.gameObject.SetActive(true);
        saveButton.interactable = true;
        cancelButton.gameObject.SetActive(true);
        cancelButton.interactable = true;
    }

    private void disableNewPatientMode()
    {
        enterPatientInfoLabelObject.SetActive(false);
        patientDataFilePathTextInput.interactable = true;
        browseButton.interactable = true;
        newPatientButton.interactable = true;

        patientNameTextInput.interactable = false;
        patientAgeTextInput.interactable = false;
        patientGUIDText.interactable = false;

        saveButton.gameObject.SetActive(false);
        saveButton.interactable = true;
        cancelButton.gameObject.SetActive(false);
        cancelButton.interactable = true;
    }

    private void setPatientDataFields(Patient p)
    {
        if (p == null)
        {
            patientNameTextInput.text = "";
            patientAgeTextInput.text = "";
            patientGUIDText.text = "";
        }
        else
        {
            patientNameTextInput.text = p.name;
            patientAgeTextInput.text = p.age.ToString();
            patientGUIDText.text = p.guid;
        }
    }
}
