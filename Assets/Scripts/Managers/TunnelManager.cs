using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelManager : MonoBehaviour {

	public bool _tunnel = false;
	public bool _StepBackOnTunnelHit = false;
	public float _tunnelWidth;

	public GameObject _tunnelCirclePrefab;
	public GameObject _tunnelSquarePrefab;

	public static TunnelManager instance = null;

	private static GameObject[] allTunnelCircleObjects;
	private static GameObject[] allTunnelSquareObjects;
	private static Vector3 tunnelVector;
	private static Color tunnelColor;
	private static GameObject tunnelCirclePrefab;
	private static GameObject tunnelSquarePrefab;
	private static bool tunnel;
	private static bool tunnelBackTracking;
	private static float tunnelWidth;

	private static Vector4 currentTargetAttributes;
	private static Vector4 nextTargetAttributes;

	private static Vector2 worldPosition;
	private static Collider2D hit;
	private static bool inTunnel = true;

	void Awake() {

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

	public static void CheckTunnel() {

		if (SystemInfo.deviceType == DeviceType.Handheld) {

			foreach (Touch touch in Input.touches) {

				worldPosition = Camera.main.ScreenToWorldPoint (touch.position);
			}
		}
		else if(SystemInfo.deviceType == DeviceType.Desktop) {

			worldPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		}

		CheckCollision (worldPosition);
	}

	private static void CheckCollision(Vector2 _worldPosition) {

		hit = Physics2D.OverlapPoint(_worldPosition);

		if (hit == null) {

			if (inTunnel) {
				GameManager.SetHitPosition (worldPosition);
				GameManager.OutOfTunnel ();
				inTunnel = false;

				if (tunnelBackTracking) {

					GameManager.BackTrack ();
				}
			}
		} 
		else
			inTunnel = true;
	}

	public static void PrepareTunnel() {
		
		tunnelColor = Camera.main.backgroundColor;
		Camera.main.backgroundColor = Color.black;

		allTunnelCircleObjects = new GameObject[GameManager.GetTotalTargets()];

		if(GameManager.GetTotalTargets() > 2)
			allTunnelSquareObjects = new GameObject[GameManager.GetTotalTargets()];
		else
			allTunnelSquareObjects = new GameObject[1];

		tunnelVector = new Vector3 (tunnelWidth, tunnelWidth, 1);

		for (int i = 0; i < GameManager.GetTotalTargets(); i++) {

			currentTargetAttributes = GameManager.GetTargetAttributes (i);

			allTunnelCircleObjects[i] = Instantiate(tunnelCirclePrefab, new Vector3(currentTargetAttributes.x, currentTargetAttributes.y, 1), Quaternion.identity) as GameObject;

			allTunnelCircleObjects [i].transform.localScale = tunnelVector;
			allTunnelCircleObjects [i].transform.GetChild(0).GetComponent<SpriteRenderer> ().color = tunnelColor;

			if (i < allTunnelSquareObjects.Length) {

				allTunnelSquareObjects[i] = Instantiate(tunnelSquarePrefab) as GameObject;

				allTunnelSquareObjects [i].transform.GetChild(0).GetComponent<SpriteRenderer> ().color = tunnelColor;

				if (i == allTunnelSquareObjects.Length - 1 && allTunnelSquareObjects.Length > 1)
					nextTargetAttributes = GameManager.GetTargetAttributes (0);
				else
					nextTargetAttributes = GameManager.GetTargetAttributes (i+1);

				allTunnelSquareObjects [i].transform.position = new Vector3((currentTargetAttributes.x + nextTargetAttributes.x) / 2, (currentTargetAttributes.y + nextTargetAttributes.y) / 2, 1);

				allTunnelSquareObjects [i].transform.localScale = new Vector3(Vector2.Distance(currentTargetAttributes, nextTargetAttributes), tunnelWidth, 1);
				allTunnelSquareObjects [i].transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(nextTargetAttributes.y-currentTargetAttributes.y, nextTargetAttributes.x-currentTargetAttributes.x)*180/Mathf.PI);
			}
		}
	}

	public static bool GetTunnelOn() {

		return tunnel;
	}
}
