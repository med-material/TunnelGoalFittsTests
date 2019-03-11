using System;
using UnityEngine;
using System.Collections;
using System.IO;


public class ArduinoLogger : MonoBehaviour
{
    [Header("Don't Touch these either")]
    public string FileName = "Arduino Log";
    public string FileFormat = ".tsv";
    public string path = "";
    public string header = "UnityMillis\tArduinoMillis\tEDA\tIBI\tDistance";

    private StreamWriter fileWriter;

    public bool MatchFittsLawLogging = true;
    private bool _MatchFittsLawLogging;
    public string AltHeader = "Date;Time;ArduinoMillis;EDA;IBI;RawPulse;Pressure";
    public string AltFileFormat = ".csv";

    // Use this for initialization
    void Start ()
	{
        _MatchFittsLawLogging = MatchFittsLawLogging;

        if (path == "")
        {

            if (_MatchFittsLawLogging)
                path = Application.dataPath + "/../Data/Arduino/";
            else
                path = Application.dataPath + "/logs/";
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (_MatchFittsLawLogging)
            FileName += string.Format(" {0:HH mm ss yyyy-MM-dd}", DateTime.Now) + AltFileFormat;
        else
            FileName += string.Format(" {0:HH mm ss yyyy-MM-dd}", DateTime.Now) + FileFormat;
        fileWriter = new StreamWriter(path + FileName);
        if(_MatchFittsLawLogging)
	        fileWriter.WriteLine(AltHeader);
        else
            fileWriter.WriteLine(header);

        Arduino.NewDataEvent += NewData;
	}

    void NewData(Arduino arduino)
    {

        if (_MatchFittsLawLogging)
        {
            string changedSeperator = arduino.NewestIncomingData.Replace('\t', ';');
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string time = DateTime.Now.ToString("HH:mm:ss:ffff");
            fileWriter.Write(date + ";" + time + ";" + changedSeperator);
        } else {
            fileWriter.Write((uint)(1000 * Time.realtimeSinceStartup) + "\t" + arduino.NewestIncomingData);
        }

        
        //fileWriter.Write(arduino.NewestIncomingData);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDisable()
    {
        fileWriter.Flush();
        fileWriter.Close();
        
    }
}
