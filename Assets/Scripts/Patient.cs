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

    //[DataMember(Name = "TestHistory")]
    public List<TestInfo> testHistory;

    // this is combination of the first/last name and the first 8 digits of the GUID:
    // 'Joe Bob-12345678'
    public string patientID;

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

        this.patientID = null;
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
        this.patientID = this.name + "-" + guidChunk;

        try
        {
            // first check if the directory already exists (theoretically this should never happen)
            DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/Patients/" + this.patientID);

            if (di.Exists)
                throw new DuplicateNameException("patient data directory already exists!");
            else
                di.Create();

            // now create the .xml file.  dataPath sets the name of the directory and the .xml file
            DataContractSerializer s = new DataContractSerializer(this.GetType());
            FileStream f = File.Create(Application.persistentDataPath + "/Patients/" + this.patientID + "/" + this.patientID + ".xml");

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
        Debug.Log("loading patient data from directory " + path);
        // path comes in as a full absolute path, i.e.:
        // /emulated/storage/..../com.USF...../Patients/Joe Bob-12345678
        try
        {
            // extract the patient id from the directory path
            string pid;

            // handle paths with both types of slashes
            char[] slashes = { '\\', '/' };
            int pos = path.LastIndexOfAny(slashes);
            
            if (pos != -1)
                pid = path.Substring(pos + 1);
            else
                throw new FileNotFoundException("malformed path");

            Debug.Log("patient ID: " + pid);

            FileStream f = File.Open(path + "/" + pid + ".xml", FileMode.Open);
            XmlReader reader = XmlReader.Create(f);
            DataContractSerializer s = new DataContractSerializer(typeof(Patient));
            Patient p = (Patient)s.ReadObject(reader, false);
            p.patientID = pid;
            p.loadTestHistory();

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

    private void loadTestHistory()
    {
        Debug.Log("loading test history...");

        if (patientID.Length > 0)
        {
            if (testHistory != null)
                testHistory.Clear();

            string dataPath = Application.persistentDataPath + "/Patients/" + patientID;
            
            Debug.Log("reading directory for test files");
            //string[] files = Directory.GetFiles(Application.persistentDataPath + "/Patients/" + patientID, "*.xml");
            string[] files = Directory.GetFiles(dataPath, "*.xml");
            Debug.Log("found " + files.Length + " files");

            testHistory = new List<TestInfo>();
            foreach (string filePath in files)
            {
                string fileName;
                char[] slashes = { '\\', '/' };
                int pos = filePath.LastIndexOfAny(slashes);

                if (pos != -1)
                    fileName = filePath.Substring(pos + 1);
                else
                    throw new FileNotFoundException("malformed path");

                Debug.Log("checking file: " + fileName);
                // skip the actual patient .xml file
                if (!fileName.Contains(patientID))
                {
                    TestInfo ti = TestInfo.loadFromFile(filePath);
                    if (ti != null)
                    {
                        testHistory.Add(ti);
                        Debug.Log("testinfo loaded!  type: " + (ti.type == TestType.LeftEye ? "left" : "right") + ", size: " + (int)ti.stimulusSize + ", datetime: " + ti.dateTime + ", duration: " + ti.duration + ", patientid: " + ti.patientID);
                    }
                }
            }
        }
    }
}
