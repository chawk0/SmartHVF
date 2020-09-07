using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class will hold some basic patient data, and also a list
// of tests already performed.

public class Patient
{
    public string name;
    public int age;

    public List<TestInfo> testHistory;
}
