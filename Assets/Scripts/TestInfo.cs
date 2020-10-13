using System;
using System.Collections;
using System.Collections.Generic;
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
public class TestInfo
{
    public TestType type;
    public GoldmannSize stimulusSize;
    public DateTime dateTime;
    public int duration;
    public Patient patient;
    public float camOrthoSize, stepSize;

    public List<Stimulus> stimulusField, shuffledField;
    public GameObject stimulusPrefab;
    public Vector3 stimulusFieldBoundsMin, stimulusFieldBoundsMax;


    public TestInfo(TestType type, Patient patient, float camOrthoSize, GoldmannSize stimulusSize = GoldmannSize.III)
    {
        this.type = type;
        this.patient = patient;
        this.camOrthoSize = camOrthoSize;
        this.stimulusSize = stimulusSize;

        buildStimulusField();
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
        this.shuffledField = new List<Stimulus>();
        List<Stimulus> temp = new List<Stimulus>(this.stimulusField);
        System.Random rng = new System.Random();

        while (temp.Count > 0)
        {
            int index = rng.Next(0, temp.Count);
            this.shuffledField.Add(this.stimulusField[index]);
            temp.RemoveAt(index);
        }

        Debug.Log("stimulus field size: " + this.stimulusField.Count);
        Debug.Log("shuffled field size: " + this.shuffledField.Count);
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

    public Texture2D generateEyeMap()
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
            float mapStepX = (float)(this.stimulusFieldBoundsMax.x - this.stimulusFieldBoundsMin.x) / eyeMap.width;
            float mapStepY = (float)(this.stimulusFieldBoundsMax.y - this.stimulusFieldBoundsMin.y) / eyeMap.height;

            // start at the bottom left corner of the field bounds (biased to the center of the sample)
            Vector3 p = new Vector3();
            p.x = this.stimulusFieldBoundsMin.x + mapStepX / 2.0f;
            p.y = this.stimulusFieldBoundsMin.y + mapStepY / 2.0f;
            p.z = -15.0f;

            // iterate through the eyemap, using the alpha channel to tell where to sample (a == 1)
            for (int y = 0; y < eyeMap.height; ++y)
            {
                // at the start of each row, reset x coord to the left
                p.x = this.stimulusFieldBoundsMin.x + mapStepX / 2.0f;

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
}
