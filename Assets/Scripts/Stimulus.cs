using System.Runtime.Serialization;
using UnityEngine;

// this object represents one stimulus point in the stimulus field, and it's
// represented as a quad in world space, textured with a white circle.
// the _Color shader parameter is varied to change "brightness" and the scale
// is manipulated to adjust the size, corresponding to Goldmann sizes I-V (needs to be calibrated)

[DataContract(Name = "Stimulus")]
public class Stimulus
{
    [DataMember(Name = "Pos")]
    public Vector3 position;
    [DataMember(Name = "Brightness")]
    public float brightness;
    private float size;

    private GameObject instance;
    private Material material;

    //private static int colorID;

    public Stimulus(GameObject prefab, Vector3 startPosition)
    {
        // save starting position
        this.position = startPosition;
        // create instance from prefab; start behind camera initially so it's not visible
        this.instance = (GameObject)GameObject.Instantiate(prefab, startPosition, Quaternion.identity);
        // grab a reference to the material for setting the color
        this.material = this.instance.GetComponent<Renderer>().material;
        // set to black initially, though the starting brightness is 100%
        //this.material.SetColor("_Color", new Color(this.brightness, this.brightness, this.brightness));
        //this.brightness = Random.Range(0.0f, 1.0f);
        this.brightness = 0.9f;
        // move into place
        //this.instance.transform.SetPositionAndRotation(this.position, Quaternion.identity);
    }

    ~Stimulus()
    {
        destroy();
    }

    public void show()
    {
        this.material.SetColor("_Color", new Color(this.brightness, this.brightness, this.brightness));
    }

    public void hide()
    {
        this.material.SetColor("_Color", Color.black);
    }

    public void dimBy(float d)
    {
        this.brightness -= d;
        if (this.brightness < 0)
            this.brightness = 0;
    }

    public void destroy()
    {
        Object.Destroy(instance);
    }

    public void setScale(float newScale)
    {
        this.instance.transform.localScale = new Vector3(newScale, newScale, 1.0f);
    }

    public void setZ(float newZ)
    {
        this.instance.transform.SetPositionAndRotation(new Vector3(this.position.x, this.position.y, newZ), Quaternion.identity);
        this.position.z = newZ;
    }
}
