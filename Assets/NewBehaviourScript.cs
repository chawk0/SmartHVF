using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject quad;
    private bool moveQuad;

    // Start is called before the first frame update
    void Start()
    {
        moveQuad = false;
        //quad = GameObject.Find("Quad2");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            moveQuad = true;
        else if (Input.GetKeyUp(KeyCode.Space))
            moveQuad = false;

        if (moveQuad)
            quad.transform.Translate(0.01f, 0, 0);
    }
}
