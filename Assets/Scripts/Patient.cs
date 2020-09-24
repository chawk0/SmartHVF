using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class will hold some basic patient data, and also a list
// of tests already performed.

public class Patient
{
    private string _name;
    public int age;

    public string name
    {
        get
        {
            return _name;
        }
    }

    public List<TestInfo> testHistory;
}
