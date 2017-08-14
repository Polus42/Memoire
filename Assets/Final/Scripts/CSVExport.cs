using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVExport : MonoBehaviour {
    public static bool logging = true;
    public string fileName = "Assets/Data.csv";
    public static string fileNamestatic;
    public static string allLog = "";
    // Use this for initialization
    void Start() {
        fileNamestatic = fileName;
    }
    public static void LogCSV(string[] content)
    {
        if(logging)
        {
            string log = "";
            for (int i = 0; i < content.Length; i++)
            {
                if(i!=content.Length-1)
                {
                    log += content[i]+ ";";
                }
                else
                {
                    log += content[i];
                }
            }
            allLog += log+'\n';
        }
    }
    private void OnDestroy()
    {
        System.IO.File.WriteAllText(fileNamestatic, allLog.Replace('.', ','));
    }
}
