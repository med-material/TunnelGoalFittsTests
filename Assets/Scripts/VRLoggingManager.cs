using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.IO;

public class VRLoggingManager : MonoBehaviour {

	public static VRLoggingManager instance = null; 

	public GameObject managers;
	public Transform latestCollision;
	public Transform previousCollision;
	public Transform tunnelMid;
	public Transform headLocation;
	public GameObject target;

	public static Transform _latestCollision;
	public static Transform _previousCollision;
	public static Transform _tunnelMid;
	public static Vector3 hitPos;
	public static Vector3 missPos;
	public static float targetWidth; 

	public static string _userID;
	public static InputResponders _inputResponders;
	public static InputType _inputType;

	private static string headers = "Date;Time;UserID;GameType;InputType;InputResponders;HitType;TargetNumber;TargetID;SessionTime;DeltaTime;TargetPosX;TargetPosY;HitPosX;HitPosY;xDistanceToExit;yDistanceToExit;distanceToPrevious;TargetWidth;TargetGap;Backtracking;ErrorTargetID";

	private static StreamWriter writer;
	private static string directory;
	private static string fileName;
	private static string sep = ";";
	private static string currentEntry;

	private static string date;
	private static string time;

	public void Awake() {

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);

		// Don't run if UserID isn't set
		if(managers.GetComponent<LoggingManager>().userID == ""){
			Debug.LogError("Please set User ID (In Hierarchy -> Managers -> VRLoggingManager)");
			EditorApplication.ExecuteMenuItem("Edit/Play");
		}

        // directory = Application.persistentDataPath + "/Data/";

        directory = Application.dataPath + "/../Data/VRData/";

        if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		// Get info from the default Logging Manager
		_userID	= managers.GetComponent<LoggingManager>().userID;
		_inputType	= managers.GetComponent<LoggingManager>().inputType;
		_inputResponders = managers.GetComponent<LoggingManager>().inputResponders;
		_latestCollision = latestCollision;
		_previousCollision = previousCollision;
		_tunnelMid = tunnelMid;

	}

	public void Update(){
		if(!target){
			target = GameObject.Find("TunnelTarget(Clone)");
			if(target){
				targetWidth = target.transform.localScale.x; 
			}
		}
	}

	public static void NewEntry(GameType _gameType, string _hitType, 
	                     int _targetNumber,
	                     int _targetID, 
	                     float _sessionTime, 
	                     float _deltaTime, 
	                     Vector2 _targetPos, 
	                     Vector2 _hitPos, 
	                     float _diameter,
	                     float _collider,
	                     Vector2 _outsetTarget, 
	                     Vector2 _outsetHit,
						 bool _backtracking,
						 int _errorTargetID) {

		//
		hitPos = _latestCollision.position;

		/*
		if(_hitType == "Hit"){
			hitPos = _latestCollision.position;
			missPos = new Vector3(0,0,0);
		}
		if(_hitType == "Miss"){
			hitPos = new Vector3(0,0,0);
			missPos = _latestCollision.position;
		}
		*/

		// Calculate distances to target
		float xDist = Vector3.Distance(new Vector3(hitPos.x,0,0),new Vector3(_targetPos.x,0,0));
		float yDist = Vector3.Distance(new Vector3(0,hitPos.y,0),new Vector3(0,_targetPos.y,0));
		// Subtract target width from target distance
		xDist = xDist - (targetWidth / 2);

		// Distance from previous collision
		float distanceToPrev = Vector3.Distance(_previousCollision.position,_latestCollision.position);

		// Time
		date = System.DateTime.Now.ToString("yyyy-MM-dd");
		time = System.DateTime.Now.ToString("HH:mm:ss:ffff");

		currentEntry = 	date + sep +
						time + sep +
						_userID + sep +
                        _gameType + sep +
                        _inputType + sep +
                        _inputResponders + sep +
						_hitType + sep +
						_targetNumber + sep +
						_targetID + sep +
						_sessionTime + sep +
						_deltaTime + sep +

						// Target positions
						_targetPos.x + sep +
						_targetPos.y + sep +

						// Replaced with:
						// Hit Position
						hitPos.x + sep +
						hitPos.y + sep +

						// Distance from hit to target (on each axis)
						xDist + sep +
						yDist + sep +

						// Distance from previous collision
						distanceToPrev + sep +

						// Width of the targets
						targetWidth + sep +

						// Target gap 
						// THIS IS BROKEN FIX IT
						xDist * 2 + sep +

						/*
						_outsetTarget.x + sep +
						_outsetTarget.y + sep +
						(_targetPos.x - _outsetTarget.x) + sep +
						(_targetPos.y - _outsetTarget.y) + sep +
						_outsetHit.x + sep +
						_outsetHit.y + sep +
						(_hitPos.x - _outsetHit.x) + sep +
						(_hitPos.x - _outsetHit.y) + sep +
						*/

						// End of changes
						_backtracking + sep +
						_errorTargetID;

		using (StreamWriter writer = File.AppendText(directory + fileName))
		{
			writer.WriteLine(currentEntry);
		}
	}

	public static void NewLog() {
		
		fileName = System.DateTime.Now.ToString() + ".csv";
		fileName = fileName.Replace ('/', '-');
		fileName = fileName.Replace (':', '-');

		using (StreamWriter writer = File.AppendText(directory + fileName))
		{
			writer.WriteLine(headers);
		}
	}
}