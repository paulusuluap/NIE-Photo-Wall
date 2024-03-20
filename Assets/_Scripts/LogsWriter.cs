using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogsWriter : MonoBehaviour
{
    private void Start()
    {
        WriteLog("makan aysssam enak rasanya\n" +
                 System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss"));
    }
    public static void WriteLog(string log)
    {
        string path = Application.streamingAssetsPath + "/logs.txt";
        //Write some text to the logs.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(log);
        writer.Close();
        StreamReader reader = new StreamReader(path);
        //Print the text from the file
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
    public static void ReadString()
    {
        string path = Application.persistentDataPath + "/logs.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
