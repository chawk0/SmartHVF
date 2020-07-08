using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject stimulusPreFab;
    public GameObject exitButton;
    public GameObject startButton;
    private GameObject visualField;

    private bool inTest;
    private bool stimulusSeen;
    private bool inputTimeout;

    private TimeoutTimer tot;

    void Start()
    {
        Debug.Log("Starting app...");
        Debug.Log("Screen size: " + Screen.width + ", " + Screen.height);

        GameObject camera = GameObject.Find("Main Camera");
        float os = camera.GetComponent<Camera>().orthographicSize;
        Debug.Log("Camera ortho size: " + os);
        Debug.Log("World size: " + ((float)Screen.width / Screen.height * os * 2.0f) + ", " + (os * 2.0f));

        Button b = exitButton.GetComponent<Button>();
        b.onClick.AddListener(exitButtonClick);
        
        b = startButton.GetComponent<Button>();
        b.onClick.AddListener(startButtonClick);

        inTest = false;
        stimulusSeen = false;

        tot = new TimeoutTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // only track the start of a touch event
                if (inTest && (touch.phase == TouchPhase.Began))
                {
                    Debug.Log("Touch input received...");
                    stimulusSeen = true;
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
        //StartCoroutine(spawnTestStimulus());
        StartCoroutine(spawnTestStimulus3());
    }

    void exitButtonClick()
    {
        Application.Quit();
    }

    /*
    IEnumerator waitForTimeout(float timeout)
    {
        inputTimeout = false;

        yield return new WaitForSeconds(timeout);

        if (!stimulusSeen)
            inputTimeout = true;
    }*/

    IEnumerator spawnTestStimulus3()
    {
        // spawn a test stimulus behind the camera
        GameObject s = (GameObject)Instantiate(stimulusPreFab, new Vector3(0, 0, -100), Quaternion.identity);
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
    }

    IEnumerator spawnTestStimulus2()
    {
        GameObject s = (GameObject)Instantiate(stimulusPreFab, new Vector3(0, 0, -100), Quaternion.identity);

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

    IEnumerator spawnTestStimulus()
    {
        GameObject[] s = new GameObject[10];

        Vector3 spawnPos = new Vector3(-1.5f, -4.5f, 0);

        for (int i = 0; i < s.Length; i++)
        {
            float c = (float)i / 9.0f;

            s[i] = (GameObject)Instantiate(stimulusPreFab, spawnPos, Quaternion.identity);
            s[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(c, c, c));

            spawnPos.y += 1.0f;
        }

        yield return new WaitForSeconds(5);

        foreach (GameObject o in s)
            Destroy(o);

        yield return new WaitForSeconds(1);

        startButton.SetActive(true);
    }
}
