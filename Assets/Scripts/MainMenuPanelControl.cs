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
        //Debug.Log("Hello from MainMenuPanelControl.cs");
        //Debug.Log("testInt is " + testInt);

        // get a reference to the Main script
        main = GameObject.Find("AppControl").GetComponent<Main>();
        newTestButton = GameObject.Find("/Canvas/MainMenuPanel/NewTestButton").GetComponent<Button>();
        // get references to child UI objects
        browseTestHistoryButton = GameObject.Find("/Canvas/MainMenuPanel/BrowseTestHistoryButton").GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (main.currentPatient == null)
        {
            newTestButton.interactable = false;
            browseTestHistoryButton.interactable = false;
        }
        else
        {
            newTestButton.interactable = true;
            browseTestHistoryButton.interactable = true;
        }
    }

    void Update()
    {
        
    }

    public void NewTestButton_Click()
    {
        main.setActivePanel(UIPanel.NewTestSetup);
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
