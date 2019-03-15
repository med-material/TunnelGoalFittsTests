using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class StatusBarMonitor : MonoBehaviour
{

    [SerializeField]
    private Arduino arduino;

    // UI

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
    }

    // Update is called once per frame
    void Update()
    {
        EDAText.text = string.Format(EDATextTemplate,arduino.RawEDA.ToString());
        IBIText.text = string.Format(IBITextTemplate, arduino.IBI.ToString());
        RawPulseText.text = string.Format(RawPulseTextTemplate, arduino.RawPulse.ToString());
        PressureText.text = string.Format(pressureTextTemplate, arduino.rawPressure.ToString());
    }
}
