using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions.Must;

// this object represents one stimulus point in the stimulus field, and it's
// represented as a quad in world space, textured with a white circle.
// the _Color shader parameter is varied to change "brightness" and the scale
// is manipulated to adjust the size, corresponding to Goldmann sizes I-V (needs to be calibrated)

public enum GoldmannSize
{
    I = 0, II, III, IV, V
}

// attributes for serializing to XML
[DataContract(Name = "Stimulus")]
public class Stimulus
{
    [DataMember(Name = "Pos")]
    public Vector3 position;
    [DataMember(Name = "Brightness")]
    public float brightness;
    [DataMember(Name = "GoldmannSize")]
    public GoldmannSize size;

    private GameObject instance;
    private Material material;

    // defines the Z planes of the stimuli when part of an inactive test, and when hidden/inactive
    public const float inTestZPlane = 0.0f;
    public const float inactiveZPlane = -15.0f;
    
    public Stimulus(GameObject prefab, Vector3 startPosition, GoldmannSize size = GoldmannSize.III)
    {
        // initial position in worldspace
        this.position = startPosition;
        // create instance from prefab
        //this.instance = (GameObject)GameObject.Instantiate(prefab, startPosition, Quaternion.identity);
        this.instance = (GameObject)MonoBehaviour.Instantiate(prefab, startPosition, Quaternion.identity);
        // grab a reference to the material for setting the color
        this.material = this.instance.GetComponent<Renderer>().material;
        // set to full brightness by default
        this.brightness = 1.0f;
        this.material.SetColor("_Color", new Color(this.brightness, this.brightness, this.brightness));
        // set scale based on Goldmann size
        this.size = size;
        computeScale(this.size);
        // set hidden/inactive by default
        setZ(Stimulus.inactiveZPlane);
    }

    ~Stimulus()
    {
        //Debug.Log("~Stimulus() invoked!");
        destroy();
    }

    // during testing, a stimulus is made visible by moving it to the active "in test" Z plane
    public void show()
    {
        setZ(Stimulus.inTestZPlane);
    }

    // and hidden by moving it back to the inactive Z plane behind the camera
    public void hide()
    {
        setZ(Stimulus.inactiveZPlane);
    }

    public void dimBy(float d)
    {
        this.brightness -= d;
        if (this.brightness < 0)
            this.brightness = 0;

        this.material.SetColor("_Color", new Color(this.brightness, this.brightness, this.brightness));
    }

    // this is not working.....?
    public void destroy()
    {
        //Debug.Log("Stimulus.destroy() invoked!");
        if (instance != null)
        {
            Debug.Log("destroying stimulus instance: " + instance.name + ", object: " + instance);
            //UnityEngine.Object.Destroy(instance);
            //MonoBehaviour.Destroy(instance);
            UnityEngine.Object.DestroyImmediate(instance);
            //Destroy(instance);
            instance = null;
        }
        else
            Debug.Log("can't destroy, stimulus instance null!");
    }

    // currently, there's no direct correspondence between the sizes here and actual Goldmann sizes
    // in a real HVF test.  the lowest size is arbitrary for now, and scaled by a factor of 4 in
    // area as you go up in size, like the real Goldmann sizes do.
    // later, this can hopefully be calibrated to correspond to a real angular size based on the
    // optics of the headset, the specs of the screen of the phone in usage, etc.
    private void computeScale(GoldmannSize size)
    {
        // size I to V maps to 0 to 4.  each size is 4x the area as the previous, so x/y scale is doubled
        float newScale = 0.025f * (float)Math.Pow(2.0, (double)size);
        this.instance.transform.localScale = new Vector3(newScale, newScale, 1.0f); // scale only X and Y
    }

    // set both the prefab instance's transform, and the internal position
    private void setZ(float newZ)
    {
        this.instance.transform.SetPositionAndRotation(new Vector3(this.position.x, this.position.y, newZ), Quaternion.identity);
        this.position.z = newZ;
    }
}
