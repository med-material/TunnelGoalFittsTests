using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

	[SerializeField]
	private Dropdown inputDropdown;

	[SerializeField]
	private InputField inputSpeed;

    public float speed = 0.01f;

    private Vector2 defaultPosition;
    private bool enable = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Arduino.NewDataEvent += NewData;
        defaultPosition = this.transform.position;
        inputSpeed.text = speed.ToString();
    }

    void NewData(Dictionary<string, List<string>> data) {
        float.TryParse(data["Pressure"][data["Pressure"].Count-1], NumberStyles.Any, CultureInfo.InvariantCulture, out pressure);
    }

    public void Enable() {
        UnityEngine.Cursor.visible = false;
        this.transform.position = defaultPosition;
        enable = true;
    }

    public void SetCursorSpeed(string newSpeed) {
        if (float.Parse(newSpeed) != speed) {
            speed = float.Parse(newSpeed);
        }
    }

    public void Disable() {
        UnityEngine.Cursor.visible = true;
        enable = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (enable) {
            if (inputDropdown.value == (int) InputType.pressuresensor) {
                if (pressure < minPressure) {
                    this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(0.02f, 0.5f));
                } else if (pressure > maxPressure) {
                    this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(1.0f, 0.5f));
                } else {
                    xPos = (pressure - minPressure) / (maxPressure - minPressure);
                    this.transform.position = sceneCamera.ViewportToWorldPoint(new Vector2(xPos, 0.5f));
                }
            } else {
                this.transform.position = new Vector2(this.transform.position.x + Input.GetAxis("Mouse X") * speed, this.transform.position.y + Input.GetAxis("Mouse Y") * speed);
            }
        }

    }

    public void SetCursorPosition(Vector2 screenPosition) {
        this.transform.position = screenPosition;
    }

    public void ResetPosition() {
        this.transform.position = defaultPosition;
    }


    public Vector2 GetScreenPosition() {
        return sceneCamera.WorldToScreenPoint(this.transform.position);
    }
}
