using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// enum type to specify which UI panel is currently active/shown.
// the InTest state would have everything hidden while the test coroutine runs
public enum UIPanel
{
    MainMenu, LoadPatient, BrowseTestHistory, NewTestSetup, InTest, TestResults
}

public class Main : MonoBehaviour
{
    // linked in the inspector
    public GameObject stimulusPrefab;
    public GameObject crosshair;
    public RenderTexture resultsTexture;

    public GameObject[] UIPanels;
    /*
    public GameObject mainMenuPanel;
    public GameObject testConfigPanel;
    public GameObject patientDataPanel;
    public GameObject testResultsPanel;
    public GameObject loadPatientPanel;
    */

    public Camera mainCamera;
    public GameObject testResultsPreviewBackdrop;

    // state variables for testing and input
    private bool inTest, abortTest;
    private bool stimulusSeen;

    // used for the abort test functionality
    private float lastTouchStartTime;

    // simple timer to trigger on user timeout
    private TimeoutTimer tot;

    // used in generating the field and grayscale map
    private float camOrthoSize;
    private float stepSize;

    //[HideInInspector]
    public Patient currentPatient = null;

    // holds the most recent test result's generated eyemap
    //private Texture2D testResultEyeMap;
    public TestInfo testInfo;

    // java objects to interface with the SmartHVF-Input library for BT commss
    private AndroidJavaObject unityContext;
    private AndroidJavaObject btLib;

    void Start()
    {
        Debug.Log("Starting app ...");
        Debug.Log("Screen size: " + Screen.width + ", " + Screen.height);

        camOrthoSize = mainCamera.orthographicSize;

        Debug.Log("Camera ortho size: " + camOrthoSize);
        Debug.Log("World size: " + ((float)Screen.width / Screen.height * camOrthoSize * 2.0f) + ", " + (camOrthoSize * 2.0f));

        // set initial states
        inTest = false;
        abortTest = false;
        stimulusSeen = false;
        lastTouchStartTime = 0;

        // start app on the main menu panel
        setActivePanel(UIPanel.MainMenu);

        /*
        if (currentPatient == null)
        {
            GameObject.Find("/Canvas/MainMenuPanel/NewTestButton").GetComponent<Button>().interactable = false;
            GameObject.Find("/Canvas/MainMenuPanel/BrowseTestHistoryButton").GetComponent<Button>().interactable = false;
        }
        */

        // create the timeout timer
        tot = new TimeoutTimer();

        // create the stimulus field objects
        //buildStimulusField();

        // setup the Android Java objects that let us communicate to the SmartHVF-Input project and
        // receive bluetooth comms
        /*
        AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityContext = player.GetStatic<AndroidJavaObject>("currentActivity");

        btLib = new AndroidJavaObject("com.example.testlibrary.TestClass");
        //btLib.Call("InitBluetooth", new object[] { unityContext });
        */

        //Patient p = new Patient("Philip Lancaster", 20);
        //p.testSerialize();

        //Debug.Log("new patient GUID: " + p.guid);

        /*
        Patient p = new Patient();
        p.testRead();

        Debug.Log("Patient p: name: " + p.name + ", age: " + p.age);
        Debug.Log("Patient p.testList == null?: " + (p.testList == null).ToString());
        Debug.Log("Patient p GUID: " + p.guid);

        Guid g = new Guid("bbe733a5-9e6d-43b0-a711-aa9ebf981959");

        Debug.Log("sample guid: " + g);
        Debug.Log("sample guid == p.guid?: " + (g.Equals(p.guid)).ToString());
        */
        
    }

    
    /*
    public void TestOnSuccess(string[] paths)
    {
        Debug.Log("string[] paths length: " + paths.Length + ", paths[0]: " + paths[0]);
    }

    public void TestOnCancel()
    {
        Debug.Log("FileBrowser.ShowLoadDialog canceled");
    }*/

    public void TestButton_Click()
    {
        //mainMenuPanel.GetComponent<CanvasRenderer>().cull = true;

        //FileBrowser.ShowLoadDialog(TestOnSuccess, TestOnCancel, false, false, Application.persistentDataPath);
        /*
        string path = Application.persistentDataPath;
        string cwd = Directory.GetCurrentDirectory();
        string[] dirs = Directory.GetDirectories(cwd);
        string[] files = Directory.GetFiles(cwd);

        Debug.Log("Current working directory: " + cwd);
        Debug.Log("Directories:");
        foreach (string d in dirs)
            Debug.Log(d);
        Debug.Log("Files:");
        foreach (string f in files)
            Debug.Log(f);
        */
    }

    public void setActivePanel(UIPanel newState)
    {
        // hide any active UI panels
        foreach (GameObject o in UIPanels)
            o.SetActive(false);

        // activate the requested one
        switch (newState)
        {
            case UIPanel.MainMenu:          UIPanels[0].SetActive(true); break;
            case UIPanel.NewTestSetup:      UIPanels[1].SetActive(true); break;
            case UIPanel.LoadPatient:       UIPanels[2].SetActive(true); break;
            case UIPanel.BrowseTestHistory: UIPanels[3].SetActive(true); break;
            case UIPanel.TestResults:       UIPanels[4].SetActive(true); break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            stimulusSeen = true;
            
        // detect any touching of the screen
        if (Input.touchCount > 0)
        {
            // only 1 finger
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // only handle touch input while in a test
                if (inTest)
                {
                    // only respond to the initial touch
                    if (touch.phase == TouchPhase.Began)
                    {
                        lastTouchStartTime = Time.time;
                        //Debug.Log("Touch input received...");
                        stimulusSeen = true;
                    }
                    // else, if user holds for 3 seconds, abort the test
                    else if (touch.phase == TouchPhase.Stationary && ((Time.time - lastTouchStartTime) > 3.0f))
                    {
                        //Debug.Log("in stationary touch (last time: " + lastTouchStartTime.ToString("F1") + ", current time: " + Time.time.ToString("F1") + ")");
                        abortTest = true;
                    }
                }
            }
        }

        // if there was data received over BT, count that too
        //if (btLib.Call<bool>("GetInput") == true)
        //    stimulusSeen = true;

        // keep the timeout timer updated if in testing
        if (inTest)
            tot.update();
    }

    /*
    public void TestSaveButton_Click()
    {
        //mainMenuPanel.SetActive(false);
        //testConfigPanel.SetActive(true);


        
        string filePath = Application.persistentDataPath + "/zlayers.txt";
        string[] lines;

        if (File.Exists(filePath))
        {
            Debug.Log("sample file found!  " + filePath);
            Debug.Log("contents:");

            lines = File.ReadAllLines(filePath);

            foreach (string s in lines)
                Debug.Log(s);
        }
        else
        {
            string[] sampleFile = { "test file", "from unity" };
            File.WriteAllLines(filePath, sampleFile);
            Debug.Log("writing sample file " + filePath + " ...");
        }
        
    }
    */

    /*
    public void BeginTestButton_Click()
    {
        // build a test info object to hand off to the testing routine.
        Patient patient = new Patient("Joe Bob", 42);

        //patient.age = 42;
        //patient.name = "Joe Bob";

        Toggle leftEye = GameObject.Find("LeftEyeToggle").GetComponent<Toggle>();
        TestType tt = leftEye.isOn ? TestType.LeftEye : TestType.RightEye;
        int stimulusSize = GameObject.Find("StimulusSizeDropdown").GetComponent<Dropdown>().value;

        testInfo = new TestInfo(tt, patient, camOrthoSize, stimulusSize);
        
        testConfigPanel.SetActive(false);

        Debug.Log("starting coroutine...");
        // make the crosshair visible
        crosshair.GetComponent<Transform>().SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        // start the test routine
        StartCoroutine(fieldTest2(testInfo));
    }*/

    /*public void CancelNewTestButton_Click()
    {
        testConfigPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }*/

    /*public void ExitButton_Click()
    {
        Application.Quit();
    }*/

    /*public void SaveTestResultsButton_Click()
    {
        testSave();
    }*/

    /*public void BackButton_Click()
    {
        testResultsPreviewBackdrop.SetActive(false);
        testResultsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }*/

    /*public void PanelTest()
    {
        GameObject input = GameObject.Find("InputField");
        GameObject label = GameObject.Find("TextLabel");

        if (input != null && label != null)
        {
            Text inputText = input.GetComponentInChildren<Text>();
            Text labelText = label.GetComponent<Text>();

            if (inputText != null && labelText != null)
            {
                Debug.Log("inputText.text: " + inputText.text);
                Debug.Log("textText.text: " + labelText.text);
                labelText.text = inputText.text;
                Debug.Log("... textText.text: " + labelText.text);
            }
            else
                Debug.Log("something's null!  inputText: " + (inputText == null) + ", textText: " + (labelText == null));
        }
    }*/


    


    IEnumerator fieldTest2(TestInfo testInfo)
    {
        bool inRampDown;

        // reset all stimuli to full brightness, start as hidden, and move to Z = 0
        

        Debug.Log("Starting test...");
        //Debug.Log("Test info: " + testInfo.type + ", stimulus size: " + testInfo.stimulusSize + ", datetime: " + testInfo.dateTime.ToString("yyyyMMdd-HH-mm-ss"));

        // wait for 1 second before beginning test
        yield return new WaitForSeconds(1);

        // begin test (this will allow the timeout timer to update)
        inTest = true;

        // iterate through stimuli
        //foreach (Stimulus s in shuffledField)
        //Stimulus s;
        for (int i = 0; i < 5; ++i)
        {
            //s = shuffledField[i];
            // the ramp down is the steady decrease in brightness for a given stimulus until it's no longer visible
            inRampDown = true;
            while (inRampDown)
            {
                // reset this each loop
                stimulusSeen = false;

                // show the stimulus at its current brightness
                //s.show();

                // clear the input status in the Java BT library
                //btLib.Call("ClearInput");

                // wait for 200ms
                yield return new WaitForSeconds(0.2f);

                // hide it
                //s.hide();

                // start the timeout timer for 3 seconds
                tot.start(1.0f);

                // wait until the user indicates stimulus was seen, the timer times out, or the user aborts the test
                yield return new WaitUntil(() => (stimulusSeen || tot.timeout || abortTest));

                if (stimulusSeen)
                {
                    // decrease by 10%
                    //s.dimBy(0.1f);
                    //s.brightness = 0.0f;
                    inRampDown = false;
                    // brief delay before next round
                    yield return new WaitForSeconds(0.4f);
                    
                }
                else if (tot.timeout)
                {
                    // we've hit the brightness threshold for this stimulus, so end this loop and start on next stimulus
                    inRampDown = false;
                    
                }
                else if (abortTest)
                {
                    Debug.Log("Aborting test...");

                    stimulusSeen = false;
                    inTest = false;
                    abortTest = false;


                    crosshair.GetComponent<Transform>().SetPositionAndRotation(new Vector3(0, 0, -5.0f), Quaternion.identity);
                    //testConfigPanel.SetActive(true);
                    yield break;
                }
            }
        }

        // at this point, the stimulus objects contain the brighness values at the threshold of visibility
        /*
        foreach(Stimulus s in stimulusField)
        {
            Debug.Log("stimulus at (" + s.position.x + ", " + s.position.y + ") visible down to brightness " + s.brightness);
        }
        */

        /*
        testInfo.duration = (int)Time.time - testInfo.duration;
        lastTestInfo = testInfo;
        Debug.Log("Test complete");

        stimulusSeen = false;
        inTest = false;
        abortTest = false;

        // move crosshair behind the camera
        crosshair.GetComponent<Transform>().SetPositionAndRotation(new Vector3(0, 0, -5.0f), Quaternion.identity);

        Debug.Log("Hiding stimulus field...");
        hideStimulusField();

        Debug.Log("Generating eyemap...");
        testResultEyeMap = generateEyeMap();
        testResultEyeMap.filterMode = FilterMode.Point;

        Debug.Log("Moving in preview backdrop...");
        testResultsPreviewBackdrop.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        Debug.Log("Updating texture...");
        Material m = testResultsPreviewBackdrop.GetComponent<Renderer>().material;
        if (m != null)
            m.SetTexture("_MainTex", (Texture)testResultEyeMap);
        else
            Debug.Log("failed to acquire test results preview backdrop material");

        Debug.Log("Switching to test results panel...");
        // transition to test results panel
        testResultsPanel.SetActive(true);        
        */
    }

    public void testSave()
    {
        // make all stimuli visible and move them behind main camera to z = -15.0f.
        // this is in between the "backdrop" quad (z = -10) and the results camera (z = -20)
        //hideStimulusField();

        // coroutine finishes the job
        StartCoroutine(CoTestSave());
        //generateEyeMap();
        //sampleStimulusField(Vector3.zero);
    }

    

    

    private IEnumerator CoTestSave()
    {
        // wait until end of frame so we know all stimuli are on and have been moved
        yield return new WaitForEndOfFrame();

        Debug.Log("Saving results to SmartHVF gallery...");

        Debug.Log("position of crosshair: " + crosshair.transform.position);
        Debug.Log("position of results backdrop: " + GameObject.Find("Results Backdrop").transform.position);
        crosshair.SetActive(false);
        testResultsPreviewBackdrop.SetActive(false);
        yield return new WaitForEndOfFrame();
        // set the currently active render texture to the one used by the results camera
        RenderTexture.active = resultsTexture;
        // create a temporary texture2d to copy this data into
        Texture2D temp = new Texture2D(resultsTexture.width, resultsTexture.height);

        temp.ReadPixels(new Rect(0, 0, resultsTexture.width, resultsTexture.height), 0, 0);
        temp.Apply();
        RenderTexture.active = null;
        crosshair.SetActive(true);
        testResultsPreviewBackdrop.SetActive(true);

        string nowString = System.DateTime.Now.ToString("yyyyMMdd-HH-mm-ss");
        // this plugin takes a texture2D, encodes to a .png image, and saves it to the gallery
        NativeGallery.SaveImageToGallery(temp, "SmartHVF", nowString + "-field.png");
        

        /*
        // simple blocky eyemap generation.
        // blockSize is computed as the ratio between stepSize and the total vertical extent in world space, but mapped to screen/pixel space
        int blockSize = (int)(stepSize / (camOrthoSize * 2.0f) * Screen.height);
        Color[] cols = new Color[blockSize * blockSize];

        foreach (Stimulus s in stimulusField)
        {
            // map the world coords to screen space
            Vector3 screenPos = mainCamera.WorldToScreenPoint(s.position);
            
            // really need some kind of memset equivalent here
            for (int i = 0; i < cols.Length; ++i)
                cols[i] = new Color(1.0f - s.brightness, 1.0f - s.brightness, 1.0f - s.brightness);

            // draw a single colored block centered at the stimulus location
            temp.SetPixels((int)screenPos.x - (blockSize - 1) / 2, (int)screenPos.y - (blockSize - 1) / 2, blockSize, blockSize, cols, 0);
        }

        temp.Apply();
        NativeGallery.SaveImageToGallery(temp, "SmartHVF", nowString + "-map1.png");
        */

        // v2 of eyemap sampling
        Texture2D map2 = testInfo.generateEyeMap();
        NativeGallery.SaveImageToGallery(map2, "SmartHVF", nowString + "-map2.png");

        // get rid of textures
        Destroy(temp);
        Destroy(map2);

        //hideStimulusField();
        
    }
}
