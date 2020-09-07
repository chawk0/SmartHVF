using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // linked in the inspector
    public GameObject stimulusPrefab;
    public GameObject crosshair;
    public RenderTexture resultsTexture;

    public GameObject mainMenuPanel;
    public GameObject testConfigPanel;
    public GameObject patientDataPanel;
    public GameObject testResultsPanel;

    public Camera mainCamera;
    public GameObject testResultsPreviewBackdrop;

    // list of Stimulus objects making up the field test
    private List<Stimulus> stimulusField;
    private List<Stimulus> shuffledField;
    private Vector3 stimulusFieldBoundsMin, stimulusFieldBoundsMax;

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

    // holds the most recent test result's generated eyemap
    private Texture2D testResultEyeMap;
    public TestInfo lastTestInfo;

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

        //mainMenuPanel = GameObject.Find("MainMenuPanel");
        //testConfigPanel = GameObject.Find("TestConfigPanel");

        mainMenuPanel.SetActive(true);
        testConfigPanel.SetActive(false);
        patientDataPanel.SetActive(false);
        testResultsPanel.SetActive(false);

        

        // create the timeout timer
        tot = new TimeoutTimer();

        // create the stimulus field objects
        buildStimulusField();

        // setup the Android Java objects that let us communicate to the SmartHVF-Input project and
        // receive bluetooth comms
        /*
        AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityContext = player.GetStatic<AndroidJavaObject>("currentActivity");

        btLib = new AndroidJavaObject("com.example.testlibrary.TestClass");
        //btLib.Call("InitBluetooth", new object[] { unityContext });
        */
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

    public void BeginTestButton_Click()
    {
        // build a test info object to hand off to the testing routine.
        TestInfo newTestInfo = new TestInfo();
        Patient patient = new Patient();

        patient.age = 42;
        patient.name = "Joe Bob";

        // check the left/right toggles
        Toggle leftEye = GameObject.Find("LeftEyeToggle").GetComponent<Toggle>();
        if (leftEye.isOn)
            newTestInfo.type = TestType.LeftEye;
        else
            newTestInfo.type = TestType.RightEye;

        
        newTestInfo.stimulusSize = GameObject.Find("StimulusSizeDropdown").GetComponent<Dropdown>().value;
        setStimulusFieldSize(newTestInfo.stimulusSize);
        newTestInfo.dateTime = DateTime.Now;
        newTestInfo.duration = (int)Time.time;
        newTestInfo.patient = patient;
        //newTestInfo.patientAge = 25;
        //newTestInfo.patientName = "Joe Bob";

        testConfigPanel.SetActive(false);

        Debug.Log("starting coroutine...");
        // make the crosshair visible
        crosshair.GetComponent<Transform>().SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        // start the test routine
        StartCoroutine(fieldTest2(newTestInfo));
    }

    public void CancelNewTestButton_Click()
    {
        testConfigPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitButton_Click()
    {
        Application.Quit();
    }

    public void SaveTestResultsButton_Click()
    {
        testSave();
    }

    public void BackButton_Click()
    {
        testResultsPreviewBackdrop.SetActive(false);
        testResultsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void PanelTest()
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
    }


    void buildStimulusField()
    {
        // build the list of stimulus objects
        stimulusField = new List<Stimulus>();

        // generates the approximate stimulus pattern from an actual HVF 24-2 test.
        int[] rowLengths = { 4, 6, 8, 8, 8, 8, 6, 4 };

        float fieldScale = 1.0f;
        stepSize = camOrthoSize * 2.0f / (rowLengths.Length + 1) * fieldScale;
        Vector3 pos = Vector3.zero;

        for (int y = 0; y < rowLengths.Length; ++y)
        {
            for (int x = 0; x < rowLengths[y]; ++x)
            {
                pos.x = -(float)(rowLengths[y] - 1) / 2.0f * stepSize + (float)x * stepSize;
                pos.y = -(float)(rowLengths.Length - 1) / 2.0f * stepSize + (float)y * stepSize;
                pos.z = -15.0f;

                /*
                // bias the locations closer to the axes
                if (pos.x < 0)
                    pos.x += (0.125f * stepSize);
                else if (pos.x > 0)
                    pos.x -= (0.125f * stepSize);

                if (pos.y < 0)
                    pos.y += (0.125f * stepSize);
                else if (pos.y > 0)
                    pos.y -= (0.125f * stepSize);
                    */
                stimulusField.Add(new Stimulus(stimulusPrefab, pos));
            }

            // generates the 2 extra stimuli at the left/right edge
            if (y == 3 || y == 4)
            {
                pos.x += stepSize;
                stimulusField.Add(new Stimulus(stimulusPrefab, pos));
            }
        }
        
        // find the extents of the stimulus field in world space
        stimulusFieldBoundsMin = new Vector3(float.MaxValue, float.MaxValue, 0);
        stimulusFieldBoundsMax = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Stimulus s in stimulusField)
        {
            if (s.position.x < stimulusFieldBoundsMin.x)
                stimulusFieldBoundsMin.x = s.position.x;
            if (s.position.y < stimulusFieldBoundsMin.y)
                stimulusFieldBoundsMin.y = s.position.y;

            if (s.position.x > stimulusFieldBoundsMax.x)
                stimulusFieldBoundsMax.x = s.position.x;
            if (s.position.y > stimulusFieldBoundsMax.y)
                stimulusFieldBoundsMax.y = s.position.y;
        }

        // expand the bounds by half the step size in each direction
        stimulusFieldBoundsMin.x -= (stepSize / 2.0f);
        stimulusFieldBoundsMin.y -= (stepSize / 2.0f);
        stimulusFieldBoundsMax.x += (stepSize / 2.0f);
        stimulusFieldBoundsMax.y += (stepSize / 2.0f);

        // build a second list that is a shuffled version of the first
        shuffledField = new List<Stimulus>();
        List<Stimulus> temp = new List<Stimulus>(stimulusField);
        System.Random rng = new System.Random();

        while (temp.Count > 0)
        {
            int index = rng.Next(0, temp.Count);
            shuffledField.Add(stimulusField[index]);
            temp.RemoveAt(index);
        }

        //Debug.Log("temp count: " + temp.Count);
        Debug.Log("shuffled field count: " + shuffledField.Count);
    }

    void setStimulusFieldSize(int size)
    {
        // size 0 to 4 maps to Goldmann sizes I to IV.  each size is 4x the area as the previous, so x/y scale is doubled
        float newScale = 0.025f * (float)Math.Pow(2.0, (double)size);

        foreach (Stimulus s in stimulusField)
            s.setScale(newScale);
    }
            

    void stageStimulusField()
    {
        // stage the stimuli at z = 0 so they're in view of the camera and ready for the test
        foreach (Stimulus s in stimulusField)
        {
            s.hide();
            s.setZ(0);
            //s.brightness = 1.0f;
        }
    }

    void hideStimulusField()
    {
        foreach (Stimulus s in stimulusField)
        {
            s.show();
            s.setZ(-15.0f);
        }
    }

    IEnumerator fieldTest2(TestInfo testInfo)
    {
        bool inRampDown;

        // reset all stimuli to full brightness, start as hidden, and move to Z = 0
        stageStimulusField();

        Debug.Log("Starting test...");
        Debug.Log("Test info: " + testInfo.type + ", stimulus size: " + testInfo.stimulusSize + ", datetime: " + testInfo.dateTime.ToString("yyyyMMdd-HH-mm-ss"));

        // wait for 1 second before beginning test
        yield return new WaitForSeconds(1);

        // begin test (this will allow the timeout timer to update)
        inTest = true;

        // iterate through stimuli
        //foreach (Stimulus s in shuffledField)
        Stimulus s;
        for (int i = 0; i < 5; ++i)
        {
            s = shuffledField[i];
            // the ramp down is the steady decrease in brightness for a given stimulus until it's no longer visible
            inRampDown = true;
            while (inRampDown)
            {
                // reset this each loop
                stimulusSeen = false;

                // show the stimulus at its current brightness
                s.show();

                // clear the input status in the Java BT library
                //btLib.Call("ClearInput");

                // wait for 200ms
                yield return new WaitForSeconds(0.2f);

                // hide it
                s.hide();

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
                    testConfigPanel.SetActive(true);
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
    }

    public void testSave()
    {
        // make all stimuli visible and move them behind main camera to z = -15.0f.
        // this is in between the "backdrop" quad (z = -10) and the results camera (z = -20)
        hideStimulusField();

        // coroutine finishes the job
        StartCoroutine(CoTestSave());
        //generateEyeMap();
        //sampleStimulusField(Vector3.zero);
    }

    private float sampleStimulusField(Vector3 pos)
    {
        // construct a list of tuples, storing a reference to a stimulus and the distance from pos
        List<Tuple<Stimulus, float>> distanceList = new List<Tuple<Stimulus, float>>();

        foreach (Stimulus s in stimulusField)
        {
            float d = Vector3.Distance(pos, s.position);
            distanceList.Add(new Tuple<Stimulus, float>(s, d));
        }

        // ascending sort based on distance (item2 of the tuple)
        distanceList.Sort((x, y) => x.Item2.CompareTo(y.Item2));

        /*
        // sample the nearest 4
        float sample = 0;
        for (int i = 0; i < 4; ++i)
            sample += ((1.0f - distanceList[i].Item1.brightness) / 4.0f);
            */

        // sample only stimuli within a certain radius based on stepsize
        float sample = 0;
        int sampleCount = 0;
        foreach (var t in distanceList)
        {
            if (t.Item2 <= (stepSize * 0.7778f))
            {
                sample += (1.0f - t.Item1.brightness);
                sampleCount++;
            }
        }

        if (sampleCount > 0)
            return sample / sampleCount;
        else
            return 1.0f - distanceList[0].Item1.brightness;
    }

    private Texture2D generateEyeMap()
    {
        Texture2D eyeMap;// = new Texture2D(2, 2);

        string filePath = "eyemap_left";
        Color[] pixelData;

        eyeMap = Resources.Load<Texture2D>(filePath);
        if (eyeMap != null)
        {
            Debug.Log("texture size: " + eyeMap.width + " w x " + eyeMap.height + " h");
            pixelData = eyeMap.GetPixels();

            // the step size to move for each sample, in world coords
            float mapStepX = (float)(stimulusFieldBoundsMax.x - stimulusFieldBoundsMin.x) / eyeMap.width;
            float mapStepY = (float)(stimulusFieldBoundsMax.y - stimulusFieldBoundsMin.y) / eyeMap.height;

            // start at the bottom left corner of the field bounds (biased to the center of the sample)
            Vector3 p = new Vector3();
            p.x = stimulusFieldBoundsMin.x + mapStepX / 2.0f;
            p.y = stimulusFieldBoundsMin.y + mapStepY / 2.0f;
            p.z = -15.0f;

            // iterate through the eyemap, using the alpha channel to tell where to sample (a == 1)
            for (int y = 0; y < eyeMap.height; ++y)
            {
                // at the start of each row, reset x coord to the left
                p.x = stimulusFieldBoundsMin.x + mapStepX / 2.0f;

                for (int x = 0; x < eyeMap.width; ++x)
                {
                    int index = x + y * eyeMap.width;
                    // only sample where alpha == 1
                    if (pixelData[index].a == 1.0f)
                        pixelData[index].r = pixelData[index].g = pixelData[index].b = sampleStimulusField(p);

                    // move x coord over
                    p.x += mapStepX;
                }

                // move y coord up
                p.y += mapStepY;
            }

            eyeMap.SetPixels(pixelData);
            eyeMap.Apply();

            return eyeMap;
        }
        else
        {
            Debug.Log("failed to load eyemap_left :(");
            return null;
        }
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

        // v2 of eyemap sampling
        Texture2D map2 = generateEyeMap();
        NativeGallery.SaveImageToGallery(map2, "SmartHVF", nowString + "-map2.png");

        // get rid of textures
        Destroy(temp);
        Destroy(map2);

        hideStimulusField();
        
    }
}
