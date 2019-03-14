using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;
using System.Linq;

public enum InputResponders
{
    rightHand,
    leftHand,
    dominantHand,
    nonDominantHand,
    bothHands,
    head,
    indexFinger,
    thumb,
    tongue,
    eyes,
    body,
    foot,
    knee,
    wrist
}
;

public enum InputType
{
    mouse,
	pressuresensor,
    keyboard,
    touchpad,
    eyeTracking,
    headMountedDisplay,
    VRController,
    touchScreen,
    graphicTablet,
    isotonicJoystick,
    isometricJoystick,
    kinect,
    leapMotion,
    novintFalcon,
    gamePad
}
;

public class LoggingManager : MonoBehaviour {

	public static LoggingManager instance = null; 

	public string userID;
	public InputResponders inputResponders;
	public InputType inputType;

	public static string _userID;
	public static InputResponders _inputResponders;
	public static InputType _inputType;

	private static string headers = "Date;Time;UserID;GameType;InputType;InputResponders;HitType;TargetNumber;TargetID;SessionTime;DeltaTime;TargetX;TargetY;HitX;HitY;HitOffsetX;HitOffsetY;OutsetTargetX;OutsetTargetY;TargetDeltaX;TargetDeltaY;OutsetHitX;OutsetHitY;DeltaHitX;DeltaHitY;TargetDiameter;ColliderDiameter;Backtracking;ErrorTargetID";

	private static StreamWriter writer;
	private static string directory;
	private static string fileName;
	private static string sep = ";";
	private static string currentEntry;

	private static string date;
	private static string time;

	// UI
	[SerializeField]
	private Dropdown inputResponderDropdown;

	[SerializeField]
	private Dropdown inputTypeDropdown;

	[SerializeField]
	private InputField emailField;
	public void Awake() {
		var optionsList = Enum.GetNames(typeof(InputResponders)).ToList();
		inputResponderDropdown.AddOptions(optionsList);

		var inputTypeList = Enum.GetNames(typeof(InputType)).ToList();
		inputTypeDropdown.AddOptions(inputTypeList);

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);

		// Don't run if UserID isn't set
		if(userID == ""){
			Debug.LogError("Please set User ID");
			EditorApplication.ExecuteMenuItem("Edit/Play");
		}

        // directory = Application.persistentDataPath + "/Data/";

		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			directory = "C:\\rtii\\" + "tunnelgoalfitts" + "\\";
			print ("Windows");
		}
		else if(Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor) {
			directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/" + "tunnelgoalfitts" + "/";
			print("Linux");
		} else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
			directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/" + "tunnelgoalfitts" + "/";
			print("Mac OSX");
		} else {
            directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/" + "tunnelgoalfitts" + "/";
            print("Unknown");
		}
/*
        directory = Application.dataPath + "/../Data/";
 */

        if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		_userID = emailField.text;
		_inputType = (InputType) inputTypeDropdown.value;
		_inputResponders = (InputResponders) inputResponderDropdown.value;
	}

	public void emailField_Changed() {
		_userID = emailField.text;
	}

	public void onInputType_Changed() {
		_inputType = (InputType) inputTypeDropdown.value;
	}

	public void onInputResponder_Changed() {
		_inputResponders = (InputResponders) inputResponderDropdown.value;
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
						_targetPos.x + sep +
						_targetPos.y + sep +
						_hitPos.x + sep +
						_hitPos.y + sep +
						(_hitPos.x - _targetPos.x) + sep +
						(_hitPos.y - _targetPos.y) + sep +
						_outsetTarget.x + sep +
						_outsetTarget.y + sep +
						(_targetPos.x - _outsetTarget.x) + sep +
						(_targetPos.y - _outsetTarget.y) + sep +
						_outsetHit.x + sep +
						_outsetHit.y + sep +
						(_hitPos.x - _outsetHit.x) + sep +
						(_hitPos.x - _outsetHit.y) + sep +
						_diameter + sep +
						_collider + sep +
						_backtracking + sep +
						_errorTargetID;

		using (StreamWriter writer = File.AppendText(directory + fileName))
		{
			writer.WriteLine(currentEntry);
		}
	}

	public static void NewLog() {
		/*
		fileName = System.DateTime.Now.ToString() + ".csv";
		fileName = fileName.Replace ('/', '-');
		fileName = fileName.Replace (':', '-');
		*/
		fileName = "rtii_output.csv";
		using (StreamWriter writer = File.AppendText(directory + fileName))
		{
			writer.WriteLine(headers);
		}
	}
}