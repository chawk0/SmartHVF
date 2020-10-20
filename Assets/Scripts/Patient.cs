using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
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

    [DataMember(Name = "TestHistory")]
    public List<TestInfo> testHistory;

    public string dataPath;

    public Patient()
    {
        //
    }

    public Patient(string name, int age, string guid)
    {
        this.name = name;
        this.age = age;
        this.guid = guid;
        this.testHistory = null;

        this.dataPath = null;
    }

    public void saveToFile()
    {
        // if patient name is "Joe Bob" and GUID is "09a669c5-....", then all
        // relevant information is stored in a directory named "Joe Bob-09a669c5",
        // inside of which is "Joe Bob-09a669c5.xml" as well as various test results
        // in separate .xml files.

        // retrieve the first chunk of the GUID
        string guidChunk = this.guid.Substring(0, this.guid.IndexOf('-'));
        // append to the end of patient name as the directory name
        this.dataPath = this.name + "-" + guidChunk;

        try
        {
            // first check if the directory already exists (theoretically this should never happen)
            DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/Patients/" + this.dataPath);

            if (di.Exists)
                throw new DuplicateNameException("patient data directory already exists!");
            else
                di.Create();

            // now create the .xml file.  dataPath sets the name of the directory and the .xml file
            DataContractSerializer s = new DataContractSerializer(this.GetType());
            FileStream f = File.Create(Application.persistentDataPath + "/Patients/" + this.dataPath + "/" + this.dataPath + ".xml");

            s.WriteObject(f, this);
            f.Close();

            Debug.Log("Wrote Patient object as serialized XML!");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to create patient data directory and/or xml file!  reason: " + e.Message);
        }
    }

    public static Patient readFromDirectory(string path)
    {
        // path comes in as a full absolute path, i.e.:
        // /emulated/storage/..../com.USF...../Patients/Joe Bob-12345678
        try
        {
            // first, check to see if the data directory exists
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                // it's stored as "first last-guidchunk".  extract from the full path
                string dataPath = path.Substring(path.LastIndexOf('/'));
                Debug.Log("attempting to read from directory '" + path + "' and file '" + dataPath + ".xml'");

                // now construct the path to the .xml file inside that directory
                string xmlPath = path + dataPath + ".xml";
                return Patient.readFromFile(xmlPath);
            }
            else
                throw new DirectoryNotFoundException("'" + path + "' doesn't exist!");
        }
        catch (Exception e)
        {
            Debug.Log("failed to read from directory!  reason: " + e.Message);
        }

        return null;
    }

    public static Patient readFromFile(string path)
    {
        try
        {
            FileStream f = File.Open(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(f);
            DataContractSerializer s = new DataContractSerializer(typeof(Patient));
            Patient p = (Patient)s.ReadObject(reader, false);

            // well this is convoluted
            p.dataPath = path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - path.LastIndexOf('/') - 1);

            f.Close();
            reader.Close();

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
