using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusItems : MonoBehaviour
{

    [SerializeField]
    private Text EDAText;
    private string EDATextTemplate;
    [SerializeField]
    private Text IBIText;
    private string IBITextTemplate;
    [SerializeField]
    private Text RawPulseText;
    private string RawPulseTextTemplate;
    [SerializeField]
    private Text PressureText;
    private string pressureTextTemplate;
    // Start is called before the first frame update
    void Start()
    {
        EDATextTemplate = EDAText.text;
        IBITextTemplate = IBIText.text;
        RawPulseTextTemplate = RawPulseText.text;
        pressureTextTemplate = PressureText.text;
        Arduino.NewDataEvent += UpdateStatus; 
    }

    void UpdateStatus(Dictionary<string, List<string>> data) {
        EDAText.text = string.Format(EDATextTemplate,data["EDA"][data["EDA"].Count-1].ToString());
        IBIText.text = data["IBI"][data["IBI"].Count-1] != "0" ? string.Format(IBITextTemplate, data["IBI"][data["IBI"].Count-1].ToString()) : IBIText.text;
        RawPulseText.text = string.Format(RawPulseTextTemplate, data["RawPulse"][data["RawPulse"].Count-1].ToString());
        PressureText.text = string.Format(pressureTextTemplate, data["Pressure"][data["Pressure"].Count-1].ToString());  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
