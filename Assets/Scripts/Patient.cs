using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

// this class will hold some basic patient data, and also a list
// of tests already performed.

[DataContract(Name = "Patient")]  
public class Patient
{
    [DataMember(Name = "Name")]
    public string name;
    [DataMember(Name = "Age")]
    public int age;
    [DataMember(Name = "GUID")]
    public Guid guid;

    public List<TestInfo> testHistory;

    [DataMember(Name = "StimulusList")]
    public List<Stimulus> testList;

    public Patient()
    {
        //
    }

    public Patient(string name, int age)
    {
        this.name = name;
        this.age = age;
        this.testList = null;// new List<Stimulus>();
        this.guid = Guid.NewGuid();

        //testList.Add(new Stimulus(null, new Vector3(1.1f, 1.2f, 1.3f)));
        //testList.Add(new Stimulus(null, new Vector3(3.1f, 2.1f, 1.1f)));
    }

    public void testSerialize()
    {
        try
        {
            DataContractSerializer s = new DataContractSerializer(this.GetType());
            FileStream f = File.Create(Application.persistentDataPath + "/patient.xml");

            s.WriteObject(f, this);
            f.Close();

            Debug.Log("Wrote test Patient object as serialized XML!");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to write serialized test object!  reason: " + e.Message);
        }
    }

    public void testRead()
    {
        try
        {
            FileStream f = File.Open(Application.persistentDataPath + "/patient.xml", FileMode.Open);
            XmlReader reader = XmlReader.Create(f);
            DataContractSerializer s = new DataContractSerializer(typeof(Patient));
            Patient p = (Patient)s.ReadObject(reader, false);

            this.age = p.age;
            this.name = p.name;
            this.testList = p.testList;
            this.guid = p.guid;

            Debug.Log("Read test Patient object as serialized XML!");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to read serialized object!  reason: " + e.Message);
        }


    }
}
