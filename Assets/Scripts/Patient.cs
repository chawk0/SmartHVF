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
    public string guid;

    public List<TestInfo> testHistory;

    [DataMember(Name = "StimulusList")]
    public List<Stimulus> testList;

    public string dataFileName, dataFileFullPath;

    public Patient()
    {
        //
    }

    public Patient(string name, int age, string guid)
    {
        this.name = name;
        this.age = age;
        this.guid = guid;
        this.testList = null;

        this.dataFileName = null;
        this.dataFileFullPath = null;
    }

    public void saveToFile()
    {
        string guidChunk = this.guid.Substring(0, this.guid.IndexOf('-'));
        string fileName = this.name + "-" + guidChunk + ".xml";
        //FileStream f = File.Create(Application.persistentDataPath + "/Patients/" + fileName);
        this.dataFileName = fileName;
        this.dataFileFullPath = Application.persistentDataPath + "/Patients/" + this.dataFileName;
        //Debug.Log("Patient.saveToFile would write to " + this.dataFileFullPath);

        try
        {
            DataContractSerializer s = new DataContractSerializer(this.GetType());
            FileStream f = File.Create(this.dataFileFullPath);

            s.WriteObject(f, this);
            f.Close();

            Debug.Log("Wrote Patient object as serialized XML!");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to write serialized Patient object!  reason: " + e.Message);
        }
    }

    public static Patient readFromFile(string path)
    {
        try
        {
            FileStream f = File.Open(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(f);
            DataContractSerializer s = new DataContractSerializer(typeof(Patient));
            Patient p = (Patient)s.ReadObject(reader, false);

            f.Close();
            reader.Close();

            /*
            this.age = p.age;
            this.name = p.name;
            this.testList = p.testList;
            this.guid = p.guid;
            */

            Debug.Log("Read Patient object as serialized XML!");

            return p;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to read serialized Patient object!  reason: " + e.Message);

            return null;
        }
    }
}
