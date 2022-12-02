using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataTracker : MonoBehaviour
{
    static public Dictionary<string, float> dataNumber = new Dictionary<string, float>();
    static public Dictionary<string, TimeSpan> TimeData = new Dictionary<string, TimeSpan>();
    
    public string fileName = "DataLog";
    
    private string m_pathFile;

    // Start is called before the first frame update
    void Start()
    {
        CreateDataFile();
        dataNumber.Add("Test", 1.0f);
        WriteData();
        ReadData();
    }

    static void RegisterDataScalar(string key,float data)
    {
        if(dataNumber.ContainsKey(key))
        {
            dataNumber[key] = data;    
        }
        else
        {
            dataNumber.Add(key,data);
            Debug.Log("Add new data " + key + ":" + data + "in data tracker");
        }
    }

    void CreateDataFile()
    {
        m_pathFile = Application.dataPath + "/" + fileName + ".csv";
        if (!File.Exists(m_pathFile))
        {
            File.WriteAllText(m_pathFile, "Hello;World");
        }
    }

    void WriteData()
    {
        string content = "Data\n";
        foreach (var kvp in dataNumber)
        {
            content += kvp.Key + ";" + kvp.Value + "\n";
        }
        File.AppendAllText(m_pathFile, content);
    }

    void ReadData()
    {
        string content = File.ReadAllText(m_pathFile);
        int index = content.IndexOf("Data\n");
        string test = content.Substring(index + 5);
        string[] data = test.Split(';');
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = data[i].Trim('\n');
            Debug.Log(data[i]);
            if (i % 2 == 1)
            {
                Debug.Log(float.Parse(data[i]));
            }
        }
    }
}
