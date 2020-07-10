using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject stimulusPrefab;
    public GameObject exitButton;
    public GameObject startButton;
    public GameObject testSaveButton;
    public RenderTexture resultsTexture;


    private List<Stimulus> stimulusField;

    // state variables for testing and input
    private bool inTest, abortTest;
    private bool stimulusSeen;
    private bool inputTimeout;

    private float lastTouchStartTime;

    // simple timer to trigger on user timeout
    private TimeoutTimer tot;

    void Start()
    {
        Debug.Log("Starting app...");
        Debug.Log("Screen size: " + Screen.width + ", " + Screen.height);

        GameObject camera = GameObject.Find("Main Camera");
        float os = camera.GetComponent<Camera>().orthographicSize;
        Debug.Log("Camera ortho size: " + os);
        Debug.Log("World size: " + ((float)Screen.width / Screen.height * os * 2.0f) + ", " + (os * 2.0f));

        // setup button handlers
        Button b = exitButton.GetComponent<Button>();
        b.onClick.AddListener(exitButtonClick);
        
        b = startButton.GetComponent<Button>();
        b.onClick.AddListener(startButtonClick);

        // set initial states
        inTest = false;
        abortTest = false;
        stimulusSeen = false;
        lastTouchStartTime = 0;

        // create the timeout timer
        tot = new TimeoutTimer();

        buildStimulusField();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
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
                        Debug.Log("Touch input received...");
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

        // keep the timeout timer updated if in testing
        if (inTest)
            tot.update();
    }

    void startButtonClick()
    {
        startButton.SetActive(false);
        exitButton.SetActive(false);
        testSaveButton.SetActive(false);
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
        stimulusField = new List<Stimulus>();

        for (float y = 1.5f; y >= -1.5f; y -= 1.0f)
        {
            for (float x = -1.5f; x <= 1.5f; x += 1.0f)
            {
                stimulusField.Add(new Stimulus(stimulusPrefab, new Vector3(x, y, 0)));
            }
        }
    }

    void resetStimulusField()
    {
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

        foreach (Stimulus s in stimulusField)
        {
            // the ramp down is the steady decrease in brightness for a given stimulus until it's no longer visible
            inRampDown = true;
            while (inRampDown)
            {
                // reset this each loops
                stimulusSeen = false;

                // show the stimulus at its current brightness
                s.show();

                // wait for 200ms
                yield return new WaitForSeconds(0.2f);

                // hide it
                s.hide();

                // wait until timeout or user input indicates stimulus was seen
                tot.start(3.0f);
                yield return new WaitUntil(() => (stimulusSeen || tot.timeout || abortTest));

                if (stimulusSeen)
                {
                    // decrease by 10%
                    s.dimBy(0.1f);
                    // brief delay before next round
                    yield return new WaitForSeconds(1.0f);
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

        foreach(Stimulus s in stimulusField)
        {
            Debug.Log("stimulus at (" + s.position.x + ", " + s.position.y + ") visible down to brightness " + s.brightness);
        }

        
        Debug.Log("Test complete");
        stimulusSeen = false;
        inTest = false;
        abortTest = false;
        startButton.SetActive(true);
        exitButton.SetActive(true);
        testSaveButton.SetActive(true);
    }

    public void testSave()
    {
        foreach(Stimulus s in stimulusField)
        {
            s.show();
            s.position.z = -15.0f;
        }

        StartCoroutine(CoTestSave());
    }

    private IEnumerator CoTestSave()
    {
        yield return new WaitForEndOfFrame();



        Debug.Log("Saving results to " + Application.dataPath + "/results.png");

        RenderTexture.active = resultsTexture;
        Texture2D temp = new Texture2D(resultsTexture.width, resultsTexture.height);

        temp.ReadPixels(new Rect(0, 0, resultsTexture.width, resultsTexture.height), 0, 0);
        temp.Apply();

        NativeGallery.SaveImageToGallery(temp, "SmartHVF", "results.png");

        //byte[] data = temp.EncodeToPNG();
        Destroy(temp);

        //File.WriteAllBytes(Application.dataPath + "/results.png", data);

        


        foreach (Stimulus s in stimulusField)
        {
            s.hide();
            s.position.z = 0;
        }
    }


    IEnumerator fieldTest1()
    {
        foreach (Stimulus s in stimulusField)
            s.show();

        yield return new WaitForSeconds(5);

        startButton.SetActive(true);
        exitButton.SetActive(true);
    }

   
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
