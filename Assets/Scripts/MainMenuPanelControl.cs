using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanelControl : MonoBehaviour
{
    private Main main;
    private Button newTestButton, browseTestHistoryButton;
    //public int testInt;

    void Awake()
    {
        Debug.Log("Hello from MainMenuPanelControl.cs");
        //Debug.Log("testInt is " + testInt);

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        newTestButton = GameObject.Find("/Canvas/MainMenuPanel/NewTestButton").GetComponent<Button>();
        // get references to child UI objects
        browseTestHistoryButton = GameObject.Find("/Canvas/MainMenuPanel/BrowseTestHistoryButton").GetComponent<Button>();
        // set initial states
        newTestButton.interactable = false;
        browseTestHistoryButton.interactable = false;
    }

    private void OnEnable()
    {
        Debug.Log("MainMenuPanel enabled");
    }

    void Update()
    {
        
    }
    public void LoadPatientButton_Click()
    {
        main.setActivePanel(UIPanel.LoadPatient);
    }

    public void ExitButton_Click()
    {
        Application.Quit();
    }
}
