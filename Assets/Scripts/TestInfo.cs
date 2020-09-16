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
    private TestType _type;
    private int _stimulusSize;
    private DateTime _dateTime;
    private int _duration;
    private Patient _patient;
    private float _camOrthoSize, _stepSize;

    private List<Stimulus> _stimulusField, _shuffledField;
    private GameObject _stimulusPrefab;
    private Vector3 _stimulusFieldBoundsMin, _stimulusFieldBoundsMax;


    public TestInfo(TestType type, Patient patient, float camOrthoSize)
    {
        _type = type;
        _patient = patient;
        _camOrthoSize = camOrthoSize;

        buildStimulusField();
    }

    private void buildStimulusField()
    {
        _stimulusField = new List<Stimulus>();
        _stimulusPrefab = GameObject.Find("AppControl").GetComponent<Main>().stimulusPrefab;

        // generates the approximate stimulus pattern from an actual HVF 24-2 test.
        int[] rowLengths = { 4, 6, 8, 10, 10, 8, 6, 4 };

        float fieldScale = 1.0f;
        _stepSize = _camOrthoSize * 2.0f / (rowLengths.Length + 1) * fieldScale;
        Vector3 pos = Vector3.zero;

        for (int y = 0; y < rowLengths.Length; ++y)
        {
            for (int x = 0; x < rowLengths[y]; ++x)
            {
                pos.x = -(float)(rowLengths[y] - 1) / 2.0f * _stepSize + (float)x * _stepSize;
                pos.y = -(float)(rowLengths.Length - 1) / 2.0f * _stepSize + (float)y * _stepSize;
                // default z plane for the stimulus field is -15.  during the test, they're
                // moved to z = 0
                pos.z = -15.0f;

                // the central 2 rows are 10 stimuli wide, but depending on whether the
                // test is left or right eye, those extra 2 stimuli on the ends are either
                // skipped or included
                if (y == 3 || y == 4)
                {
                    // left eye test has extra stimuli on the right, so skip the left ones
                    if (_type == TestType.LeftEye && x == 0)
                        continue;
                    // and vice versa
                    else if (_type == TestType.RightEye && x == 9)
                        continue;
                }

                _stimulusField.Add(new Stimulus(_stimulusPrefab, pos));
            }
        }

        // find the extents of the stimulus field in world space
        _stimulusFieldBoundsMin = new Vector3(float.MaxValue, float.MaxValue, 0);
        _stimulusFieldBoundsMax = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Stimulus s in _stimulusField)
        {
            if (s.position.x < _stimulusFieldBoundsMin.x)
                _stimulusFieldBoundsMin.x = s.position.x;
            if (s.position.y < _stimulusFieldBoundsMin.y)
                _stimulusFieldBoundsMin.y = s.position.y;

            if (s.position.x > _stimulusFieldBoundsMax.x)
                _stimulusFieldBoundsMax.x = s.position.x;
            if (s.position.y > _stimulusFieldBoundsMax.y)
                _stimulusFieldBoundsMax.y = s.position.y;
        }

        // expand the bounds by half the step size in each direction
        _stimulusFieldBoundsMin.x -= (_stepSize / 2.0f);
        _stimulusFieldBoundsMin.y -= (_stepSize / 2.0f);
        _stimulusFieldBoundsMax.x += (_stepSize / 2.0f);
        _stimulusFieldBoundsMax.y += (_stepSize / 2.0f);

        // build a second list that is a shuffled version of the first
        _shuffledField = new List<Stimulus>();
        List<Stimulus> temp = new List<Stimulus>(_stimulusField);
        System.Random rng = new System.Random();

        while (temp.Count > 0)
        {
            int index = rng.Next(0, temp.Count);
            _shuffledField.Add(_stimulusField[index]);
            temp.RemoveAt(index);
        }

        //Debug.Log("temp count: " + temp.Count);
        //Debug.Log("shuffled field count: " + shuffledField.Count);
    }
}
