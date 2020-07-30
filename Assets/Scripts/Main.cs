using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // linked in the inspector
    public GameObject stimulusPrefab;
    public GameObject exitButton;
    public GameObject startButton;
    public GameObject testSaveButton;
    public GameObject crosshair;
    public RenderTexture resultsTexture;

    // list of Stimulus objects making up the field test
    private List<Stimulus> stimulusField;

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

    // java objects to interface with the SmartHVF-Input library for BT commss
    private AndroidJavaObject unityContext;
    private AndroidJavaObject btLib;

    void Start()
    {
        Debug.Log("Starting app...");
        Debug.Log("Screen size: " + Screen.width + ", " + Screen.height);

        GameObject camera = GameObject.Find("Main Camera");
        camOrthoSize = camera.GetComponent<Camera>().orthographicSize;
        Debug.Log("Camera ortho size: " + camOrthoSize);
        Debug.Log("World size: " + ((float)Screen.width / Screen.height * camOrthoSize * 2.0f) + ", " + (camOrthoSize * 2.0f));

        // setup button handlers
        Button b = exitButton.GetComponent<Button>();
        b.onClick.AddListener(exitButtonClick);
        
        b = startButton.GetComponent<Button>();
        b.onClick.AddListener(startButtonClick);

        // move the crosshairs behind the camera initially
        //crosshair.GetComponent<Transform>().SetPositionAndRotation(new Vector3(0, 0, -10.0f), Quaternion.identity);

        // set initial states
        inTest = false;
        abortTest = false;
        stimulusSeen = false;
        lastTouchStartTime = 0;

        // create the timeout timer
        tot = new TimeoutTimer();

        // create the stimulus field objects
        buildStimulusField();

        // setup the Android Java objects that let us communicate to the SmartHVF-Input project and
        // receive bluetooth comms
        AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityContext = player.GetStatic<AndroidJavaObject>("currentActivity");

        btLib = new AndroidJavaObject("com.example.testlibrary.TestClass");
        btLib.Call("InitBluetooth", new object[] { unityContext });
    }

    // Update is called once per frame
    void Update()
    {
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
        if (btLib.Call<bool>("GetInput") == true)
            stimulusSeen = true;

        // keep the timeout timer updated if in testing
        if (inTest)
            tot.update();
    }

    void startButtonClick()
    {
        // hide the buttons before beginning test
        startButton.SetActive(false);
        exitButton.SetActive(false);
        testSaveButton.SetActive(false);

        crosshair.GetComponent<Transform>().SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        StartCoroutine(fieldTest2());
        //StartCoroutine(spawnTestStimulus());
        //StartCoroutine(spawnTestStimulus3());
    }

    void exitButtonClick()
    {
        Application.Quit();
    }

    void buildStimulusField()
    {
        // build the list of stimulus objects
        stimulusField = new List<Stimulus>();

        // generates the approximate stimulus pattern from an actual HVF test.
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
                pos.z = 0;

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

        /*
        Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        Debug.Log("stimulus[0] is at world pos: " + stimulusField[0].position + " and screen pos: " + cam.WorldToScreenPoint(stimulusField[0].position));
        Debug.Log("stimulus field has " + stimulusField.Count + " elements");
        */

    }

    void resetStimulusField()
    {
        // reset all stimuli to full brightness
        foreach (Stimulus s in stimulusField)
            s.brightness = 1.0f;
    }

    IEnumerator fieldTest2()
    {
        bool inRampDown;

        // reset all stimuli to full brightness
        resetStimulusField();

        Debug.Log("Starting test...");

        // wait for 1 second before beginning test
        yield return new WaitForSeconds(1);

        // begin test (this will allow the timeout timer to update)
        inTest = true;

        // iterate through stimuli
        foreach (Stimulus s in stimulusField)
        {
            // the ramp down is the steady decrease in brightness for a given stimulus until it's no longer visible
            inRampDown = true;
            while (inRampDown)
            {
                // reset this each loop
                stimulusSeen = false;

                // show the stimulus at its current brightness
                s.show();

                // clear the input status in the Java BT library
                btLib.Call("ClearInput");

                // wait for 200ms
                yield return new WaitForSeconds(0.2f);

                // hide it
                s.hide();

                // start the timeout timer for 3 seconds
                tot.start(3.0f);

                // wait until the user indicates stimulus was seen, the timer times out, or the user aborts the test
                yield return new WaitUntil(() => (stimulusSeen || tot.timeout || abortTest));

                if (stimulusSeen)
                {
                    // decrease by 10%
                    s.dimBy(0.1f);
                    //s.brightness = 0.0f;
                    //inRampDown = false;
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

                    startButton.SetActive(true);
                    exitButton.SetActive(true);
                    testSaveButton.SetActive(true);

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

        
        Debug.Log("Test complete");

        stimulusSeen = false;
        inTest = false;
        abortTest = false;

        startButton.SetActive(true);
        exitButton.SetActive(true);
        testSaveButton.SetActive(true);

        crosshair.GetComponent<Transform>().SetPositionAndRotation(new Vector3(0, 0, -5.0f), Quaternion.identity);
    }

    public void testSave()
    {
        // make all stimuli visible and move them behind main camera to z = -15.0f.
        // this is in between the black quad "backdrop" (z = -10) and the results camera (z = -20)
        foreach(Stimulus s in stimulusField)
        {
            s.show();
            s.position.z = -15.0f;
        }

        // coroutine finishes the job
        StartCoroutine(CoTestSave());
    }

    private IEnumerator CoTestSave()
    {
        // wait until end of frame so we know all stimuli are on and have been moved
        yield return new WaitForEndOfFrame();

        Debug.Log("Saving results to SmartHVF gallery...");

        // set the currently active render texture to the one used by the results camera
        RenderTexture.active = resultsTexture;
        // create a temporary texture2d to copy this data into
        Texture2D temp = new Texture2D(resultsTexture.width, resultsTexture.height);

        temp.ReadPixels(new Rect(0, 0, resultsTexture.width, resultsTexture.height), 0, 0);
        temp.Apply();

        // this plugin takes a texture2d. encodes to a .png image, and saves it to the gallery
        NativeGallery.SaveImageToGallery(temp, "SmartHVF", "results-" + System.DateTime.Now.ToString("yyyyMMdd-HH-mm-ss") + ".png");



        // experimental grayscale map generation

        Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        int blockSize = (int)(stepSize / (camOrthoSize * 2.0f) * 1080);
        Color[] cols = new Color[blockSize * blockSize];

        foreach (Stimulus s in stimulusField)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(s.position);
            
            for (int i = 0; i < cols.Length; ++i)
                cols[i] = new Color(1.0f - s.brightness, 1.0f - s.brightness, 1.0f - s.brightness);

            temp.SetPixels((int)screenPos.x - (blockSize - 1) / 2, (int)screenPos.y - (blockSize - 1) / 2, blockSize, blockSize, cols, 0);
        }

        temp.Apply();
        NativeGallery.SaveImageToGallery(temp, "SmartHVF", "test.png");

        // get rid of temp texture
        Destroy(temp);

        // hide each stimulus and return them to the main plane at z = 0
        foreach (Stimulus s in stimulusField)
        {
            s.hide();
            s.position.z = 0;
        }
    }

    /*
    IEnumerator fieldTest1()
    {
        foreach (Stimulus s in stimulusField)
            s.show();

        yield return new WaitForSeconds(5);

        startButton.SetActive(true);
        exitButton.SetActive(true);
    }
    */

   
    /*
    IEnumerator spawnTestStimulus3()
    {
        // spawn a test stimulus behind the camera
        GameObject s = (GameObject)Instantiate(stimulusPrefab, new Vector3(0, 0, -100), Quaternion.identity);
        // initial brightness of stimulus is 1.0 (full)
        float b = 1.0f;

        Debug.Log("Starting test...");

        // wait for 1 second before beginning test
        yield return new WaitForSeconds(1);

        // translate the stimulus into view and grab its material reference to set the color
        s.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Material m = s.GetComponent<Renderer>().material;


        // begin test
        inTest = true;
        while (inTest)
        {
            // reset each loop
            stimulusSeen = false;

            // set the brightness of the stimulus
            m.SetColor("_Color", new Color(b, b, b));

            // wait for 200m while stimulus is visible
            yield return new WaitForSeconds(0.2f);
            // then turn it black
            m.SetColor("_Color", Color.black);

            // start the timeout timer for 5 seconds
            tot.start(5.0f);
            // wait until timeout or user input indicates stimulus was seen
            yield return new WaitUntil(() => (stimulusSeen || tot.timeout));

            // if user saw the stimulus, decrease brightness and repeat test
            if (stimulusSeen)
            {
                b -= 0.1f;
                if (b < 0)
                    b = 0;

                Debug.Log("Input received, decreasing brightness by 10% (current: " + b + ")");

                // brief delay before next round
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                Debug.Log("User input timeout...");
                inTest = false;
            }
                
        }

        Debug.Log("Test complete");

        Destroy(s);

        stimulusSeen = false;
        inTest = false;
        startButton.SetActive(true);
        exitButton.SetActive(true);
    }
    */
    

    /*
    IEnumerator spawnTestStimulus2()
    {
        GameObject s = (GameObject)Instantiate(stimulusPrefab, new Vector3(0, 0, -100), Quaternion.identity);

        yield return new WaitForSeconds(1);

        inTest = true;

        s.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Material m = s.GetComponent<Renderer>().material;

        while (!stimulusSeen)
        {
            m.SetColor("_Color", Color.white);

            yield return new WaitForSeconds(0.2f);
            
            m.SetColor("_Color", Color.black);

            Debug.Log("Stimulus shown... waiting...");

            yield return new WaitForSeconds(5);
        }

        Debug.Log("Test complete");

        Destroy(s);

        stimulusSeen = false;
        inTest = false;
        startButton.SetActive(true);
    }
    */

    /*
    IEnumerator spawnTestStimulus()
    {
        GameObject[] s = new GameObject[10];

        Vector3 spawnPos = new Vector3(-1.5f, -4.5f, 0);

        for (int i = 0; i < s.Length; i++)
        {
            float c = (float)i / 9.0f;

            s[i] = (GameObject)Instantiate(stimulusPrefab, spawnPos, Quaternion.identity);
            s[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(c, c, c));

            spawnPos.y += 1.0f;
        }

        yield return new WaitForSeconds(5);

        foreach (GameObject o in s)
            Destroy(o);

        yield return new WaitForSeconds(1);

        startButton.SetActive(true);
    }
    */
}
