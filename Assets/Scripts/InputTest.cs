using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputTest : MonoBehaviour
{
    public GameObject stimulus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                StartCoroutine(ShiftStimulus());
            }
            else if (Input.touchCount == 2)
            {
                Application.Quit();
            }
        }
    }

    IEnumerator ShiftStimulus()
    {
        Debug.Log("Entering coroutine...");
        yield return new WaitForSeconds(2);
        Debug.Log("Translating...");
        stimulus.transform.Translate(0, 0.5f, 0);
        Debug.Log("Done.");
    }
}
