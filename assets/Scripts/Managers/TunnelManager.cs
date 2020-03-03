using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TunnelManager : MonoBehaviour {

	public bool _tunnel = false;
	public bool _StepBackOnTunnelHit = false;
	public float _tunnelWidth;

	public GameObject _tunnelCirclePrefab;
	public GameObject _tunnelSquarePrefab;

	public static TunnelManager instance = null;

	private GameObject[] allTunnelCircleObjects;
	private GameObject[] allTunnelSquareObjects;
	private Vector3 tunnelVector;
	private Color tunnelColor;
	private GameObject tunnelCirclePrefab;
	private GameObject tunnelSquarePrefab;
	private bool tunnel;
	private bool tunnelBackTracking;
	private float tunnelWidth;

	private Vector4 currentTargetAttributes;
	private Vector4 nextTargetAttributes;

	private Vector2 worldPosition;
	private Collider2D hit;
	private bool inTunnel = true;

	private GameManager gameManager;

	private Dropdown inputDropdown;

	[SerializeField]
	private Cursor cursor;

	void Awake() {
		gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
		inputDropdown = GameObject.Find("InputTypeDropdown").GetComponent<Dropdown>();
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);

		tunnel = _tunnel;
		tunnelCirclePrefab = _tunnelCirclePrefab;
		tunnelSquarePrefab = _tunnelSquarePrefab;
		tunnelBackTracking = _StepBackOnTunnelHit;
		tunnelWidth = _tunnelWidth;
	}

	public void CheckTunnel() {
		Debug.Log(inputDropdown.value);
		Debug.Log((int) InputType.pressuresensor);
		if (inputDropdown.value == (int) InputType.pressuresensor) {
			worldPosition = Camera.main.ScreenToWorldPoint (cursor.GetScreenPosition());
		}
		else if (SystemInfo.deviceType == DeviceType.Handheld) {

			foreach (Touch touch in Input.touches) {

				worldPosition = Camera.main.ScreenToWorldPoint (touch.position);
			}
		}
		else if(SystemInfo.deviceType == DeviceType.Desktop) {

			worldPosition = Camera.main.ScreenToWorldPoint (cursor.GetScreenPosition());
		}

		CheckCollision (worldPosition);
	}

	private void CheckCollision(Vector2 _worldPosition) {

		hit = Physics2D.OverlapPoint(_worldPosition);

		if (hit == null) {

			if (inTunnel) {
				gameManager.SetHitPosition (worldPosition);
				gameManager.OutOfTunnel ();
				inTunnel = false;

				if (tunnelBackTracking) {

					gameManager.BackTrack ();
				}
			}
		} 
		else
			inTunnel = true;
	}

	public void PrepareTunnel() {
		
		tunnelColor = Camera.main.backgroundColor;
		Camera.main.backgroundColor = Color.black;

		allTunnelCircleObjects = new GameObject[gameManager.GetTotalTargets()];

		if(gameManager.GetTotalTargets() > 2)
			allTunnelSquareObjects = new GameObject[gameManager.GetTotalTargets()];
		else
			allTunnelSquareObjects = new GameObject[1];

		tunnelVector = new Vector3 (tunnelWidth, tunnelWidth, 1);

		for (int i = 0; i < gameManager.GetTotalTargets(); i++) {

			currentTargetAttributes = gameManager.GetTargetAttributes (i);

			allTunnelCircleObjects[i] = Instantiate(tunnelCirclePrefab, new Vector3(currentTargetAttributes.x, currentTargetAttributes.y, 1), Quaternion.identity) as GameObject;

			allTunnelCircleObjects [i].transform.localScale = tunnelVector;
			allTunnelCircleObjects [i].transform.GetChild(0).GetComponent<SpriteRenderer> ().color = tunnelColor;

			if (i < allTunnelSquareObjects.Length) {

				allTunnelSquareObjects[i] = Instantiate(tunnelSquarePrefab) as GameObject;

				allTunnelSquareObjects [i].transform.GetChild(0).GetComponent<SpriteRenderer> ().color = tunnelColor;

				if (i == allTunnelSquareObjects.Length - 1 && allTunnelSquareObjects.Length > 1)
					nextTargetAttributes = gameManager.GetTargetAttributes (0);
				else
					nextTargetAttributes = gameManager.GetTargetAttributes (i+1);

				allTunnelSquareObjects [i].transform.position = new Vector3((currentTargetAttributes.x + nextTargetAttributes.x) / 2, (currentTargetAttributes.y + nextTargetAttributes.y) / 2, 1);

				allTunnelSquareObjects [i].transform.localScale = new Vector3(Vector2.Distance(currentTargetAttributes, nextTargetAttributes), tunnelWidth, 1);
				allTunnelSquareObjects [i].transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(nextTargetAttributes.y-currentTargetAttributes.y, nextTargetAttributes.x-currentTargetAttributes.x)*180/Mathf.PI);
			}
		}
	}

	public bool GetTunnelOn() {

		return tunnel;
	}
}
