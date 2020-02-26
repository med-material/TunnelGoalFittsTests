using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
public class Cursor : MonoBehaviour
{

    [SerializeField]
    private Camera sceneCamera;

    private float minPressure = 100.0f;
    private float maxPressure = 600.0f;

    private Vector2 MinPos;
    private Vector2 MaxPos;

    private float pressure;
    public float xPos;
    
    // Start is called before the first frame update
    void Start()
    {
        Arduino.NewDataEvent += NewData;
    }

    void NewData(Dictionary<string, List<string>> data) {
        float.TryParse(data["Pressure"][data["Pressure"].Count-1], NumberStyles.Any, CultureInfo.InvariantCulture, out pressure);
    }
    // Update is called once per frame
    void Update()
    {
        if (pressure < minPressure) {
            this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(0.0f, 0.5f));
        } else if (pressure > maxPressure) {
            this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(1.0f, 0.5f));
        } else {
            xPos = (pressure - minPressure) / (maxPressure - minPressure);
            this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(xPos, 0.5f));
        }

    }

    public Vector2 GetScreenPosition() {
        return sceneCamera.ViewportToScreenPoint(new Vector2(xPos, 0.5f));
    }
}
