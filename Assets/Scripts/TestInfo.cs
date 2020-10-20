using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

// the goal for this class is to encapsulate all relevant data for a given test.
// which eye was tested, the color/background settings, type of test (24-2, for example)
// the parameters of the stimulus presentation algorithm (TBD), the associated Patient
// object, the actual stimulus field values, etc.

public enum TestType
{
    LeftEye,
    RightEye
}

[DataContract(Name = "TestInfo")]
public class TestInfo
{
    [DataMember(Name = "TestType")]
    public TestType type;
    [DataMember(Name = "GoldmannSize")]
    public GoldmannSize stimulusSize;
    [DataMember(Name = "DateTime")]
    public DateTime dateTime;
    [DataMember(Name = "Duration")]
    public int duration;
    [DataMember(Name = "PatientID")]
    public string patientID;
    public Patient patient;
    [DataMember(Name = "CamOrthoSize")]
    public float camOrthoSize;
    public float stepSize;

    [DataMember(Name = "StimulusField")]
    public List<Stimulus> stimulusField;
    public List<Stimulus> shuffledField;
    public GameObject stimulusPrefab;
    public Vector3 stimulusFieldBoundsMin, stimulusFieldBoundsMax;
    public Texture2D eyeMap;


    public TestInfo(TestType type, Patient patient, float camOrthoSize, GoldmannSize stimulusSize = GoldmannSize.III)
    {
        this.type = type;
        this.patient = patient;
        this.patientID = patient.dataPath;
        this.camOrthoSize = camOrthoSize;
        this.stimulusSize = stimulusSize;

        buildStimulusField();
    }

    ~TestInfo()
    {
        Debug.Log("~TestInfo() called");
        if (stimulusField != null)
        {
            Debug.Log("Destroying stimulus field, count: " + stimulusField.Count);
            //foreach (Stimulus s in stimulusField)
            //    s.destroy();
            stimulusField.Clear();
        }
    }

    private void buildStimulusField()
    {
        this.stimulusField = new List<Stimulus>();
        this.stimulusPrefab = GameObject.Find("AppControl").GetComponent<Main>().stimulusPrefab;

        // generates the approximate stimulus pattern from an actual HVF 24-2 test.
        int[] rowLengths = { 4, 6, 8, 10, 10, 8, 6, 4 };

        // used to adjust the overall scale of the field.  so far 1.0 is fine
        float fieldScale = 1.0f;
        // stepSize represents the worldspace distance between neighboring stimuli
        this.stepSize = this.camOrthoSize * 2.0f / (rowLengths.Length + 1) * fieldScale;

        // x/y loop to generate the roundish/diamond pattern
        //                  x   x   x   x
        //
        //              x   x   x   x   x   x
        //
        //          x   x   x   x   x   x   x   x
        //
        //      R   x   x   x   x   x   x   x   x   L
        //
        //      R   x   x   x   x   x   x   x   x   L
        //
        //          x   x   x   x   x   x   x   x
        //
        //              x   x   x   x   x   x
        //
        //                  x   x   x   x
        //
        // the stimuli marked R are skipped for a left visual field, and vice versa for L

        Vector3 pos = Vector3.zero;

        for (int y = 0; y < rowLengths.Length; ++y)
        {
            for (int x = 0; x < rowLengths[y]; ++x)
            {
                pos.x = -(float)(rowLengths[y] - 1) / 2.0f * this.stepSize + (float)x * this.stepSize;
                pos.y = -(float)(rowLengths.Length - 1) / 2.0f * this.stepSize + (float)y * this.stepSize;
                // default z plane for the stimulus field is -15.  during the test, they're
                // moved to z = 0
                pos.z = Stimulus.inactiveZPlane;

                // add the two extra stimuli to the central 2 rows based on which eye the field is built for
                if (y == 3 || y == 4)
                {
                    // left eye test has extra stimuli on the right, so skip the left ones
                    if (this.type == TestType.LeftEye && x == 0)
                        continue;
                    // and vice versa
                    else if (this.type == TestType.RightEye && x == 9)
                        continue;
                }

                this.stimulusField.Add(new Stimulus(this.stimulusPrefab, pos, this.stimulusSize));
            }
        }

        // find the extents of the stimulus field in world space.
        // these bounds are used by the sampling algorithm to generate a finer-grained eyemap
        this.stimulusFieldBoundsMin = new Vector3(float.MaxValue, float.MaxValue, 0);
        this.stimulusFieldBoundsMax = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Stimulus s in this.stimulusField)
        {
            if (s.position.x < this.stimulusFieldBoundsMin.x)
                this.stimulusFieldBoundsMin.x = s.position.x;
            if (s.position.y < this.stimulusFieldBoundsMin.y)
                this.stimulusFieldBoundsMin.y = s.position.y;

            if (s.position.x > this.stimulusFieldBoundsMax.x)
                this.stimulusFieldBoundsMax.x = s.position.x;
            if (s.position.y > this.stimulusFieldBoundsMax.y)
                this.stimulusFieldBoundsMax.y = s.position.y;
        }

        // expand the bounds by half the step size in each direction
        this.stimulusFieldBoundsMin.x -= (this.stepSize / 2.0f);
        this.stimulusFieldBoundsMin.y -= (this.stepSize / 2.0f);
        this.stimulusFieldBoundsMax.x += (this.stepSize / 2.0f);
        this.stimulusFieldBoundsMax.y += (this.stepSize / 2.0f);

        

        // build a second list that is a shuffled version of the first
        this.shuffledField = new List<Stimulus>(this.stimulusField.Count);

        // list of indices to randomly pick from
        List<int> indexList = new List<int>(this.stimulusField.Count);
        for (int i = 0; i < this.stimulusField.Count; i++)
            indexList.Add(i);
        
        // rng
        System.Random rng = new System.Random();

        // randomly pick and remove an index to from the list and add
        // that corresponding stimulus to the shuffled list
        while (indexList.Count > 0)
        {
            int index = rng.Next(0, indexList.Count);
            this.shuffledField.Add(this.stimulusField[indexList[index]]);
            indexList.RemoveAt(index);
        }

        //Debug.Log("stimulus field size: " + this.stimulusField.Count);
        //Debug.Log("shuffled field size: " + this.shuffledField.Count);
    }

    public void hideStimulusField()
    {
        foreach (Stimulus s in this.stimulusField)
            s.hide();
    }

    private float sampleStimulusField(Vector3 pos)
    {
        // construct a list of tuples, storing a reference to a stimulus and the distance from pos
        List<Tuple<Stimulus, float>> distanceList = new List<Tuple<Stimulus, float>>();

        foreach (Stimulus s in this.stimulusField)
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
            if (t.Item2 <= (this.stepSize * 0.7778f))
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

    public void generateEyeMap()
    {
        string filePath = (this.type == TestType.LeftEye) ? "eyemap_left" : "eyemap_right";
        Color[] pixelData;

        this.eyeMap = Resources.Load<Texture2D>(filePath);
        if (this.eyeMap != null)
        {
            this.eyeMap.filterMode = FilterMode.Point;

            Debug.Log("texture size: " + this.eyeMap.width + " w x " + this.eyeMap.height + " h");
            pixelData = this.eyeMap.GetPixels();

            // the step size to move for each sample, in world coords
            float mapStepX = (float)(this.stimulusFieldBoundsMax.x - this.stimulusFieldBoundsMin.x) / this.eyeMap.width;
            float mapStepY = (float)(this.stimulusFieldBoundsMax.y - this.stimulusFieldBoundsMin.y) / this.eyeMap.height;

            // start at the bottom left corner of the field bounds (biased to the center of the sample)
            Vector3 p = new Vector3();
            p.x = this.stimulusFieldBoundsMin.x + mapStepX / 2.0f;
            p.y = this.stimulusFieldBoundsMin.y + mapStepY / 2.0f;
            p.z = -15.0f;

            // iterate through the eyemap, using the alpha channel to tell where to sample (a == 1)
            for (int y = 0; y < this.eyeMap.height; ++y)
            {
                // at the start of each row, reset x coord to the left
                p.x = this.stimulusFieldBoundsMin.x + mapStepX / 2.0f;

                for (int x = 0; x < this.eyeMap.width; ++x)
                {
                    int index = x + y * this.eyeMap.width;
                    // only sample where alpha == 1
                    if (pixelData[index].a == 1.0f)
                        pixelData[index].r = pixelData[index].g = pixelData[index].b = sampleStimulusField(p);

                    // move x coord over
                    p.x += mapStepX;
                }

                // move y coord up
                p.y += mapStepY;
            }

            this.eyeMap.SetPixels(pixelData);
            this.eyeMap.Apply();
        }
        else
            Debug.Log("failed to load eyemap!");
    }

    public void testSave()
    {
        string path = Application.persistentDataPath + "/Patients/" + this.patient.dataPath + "/" + this.dateTime.ToString("yyyyMMdd-HH-mm-ss") + ".xml";
        Debug.Log("test save to " + path);

        DataContractSerializer s = new DataContractSerializer(this.GetType());
        FileStream f = File.Create(path);
        s.WriteObject(f, this);
        f.Close();

        Debug.Log("Wrote TestInfo object as serialized XML!");
    }
}
