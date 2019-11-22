using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
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

	private static Dictionary <string, List<string>> logs;

	private static ConnectToMySQL connectToMySQL;
	// UI
	[SerializeField]
	private Dropdown inputResponderDropdown;

	[SerializeField]
	private Dropdown inputTypeDropdown;

	[SerializeField]
	private InputField emailField;
	public void Awake() {

		connectToMySQL = FindObjectOfType<ConnectToMySQL>();

		logs = new Dictionary<string, List<string>>() //create a new dictionary
		
		{
			{"Email", new List<string>()},
			{"Date", new List<string>()},
			{"Time", new List<string>()},
			{"UserID", new List<string>()},
			{"GameType", new List<string>()},
			{"InputType", new List<string>()},
			{"InputResponders", new List<string>()},
			{"HitType", new List<string>()},
			{"TargetNumber", new List<string>()},
			{"TargetID", new List<string>()},
			{"SessionTime", new List<string>()},
			{"DeltaTime", new List<string>()},
			{"TargetX", new List<string>()},
			{"TargetY", new List<string>()},
			{"HitX", new List<string>()},
			{"HitY", new List<string>()},
			{"HitOffsetX", new List<string>()},
			{"HitOffsetY", new List<string>()},
			{"OutsetTargetX", new List<string>()},
			{"OutsetTargetY", new List<string>()},
			{"TargetDeltaX", new List<string>()},
			{"TargetDeltaY", new List<string>()},
			{"OutsetHitX", new List<string>()},
			{"OutsetHitY", new List<string>()},
			{"DeltaHitX", new List<string>()},
			{"DeltaHitY", new List<string>()},
			{"TargetDiameter", new List<string>()},
			{"ColliderDiameter", new List<string>()},
			{"Backtracking", new List<string>()},
			{"ErrorTargetID", new List<string>()},
			


		}; 

		var optionsList = Enum.GetNames(typeof(InputResponders)).ToList();
		inputResponderDropdown.AddOptions(optionsList);

		var inputTypeList = Enum.GetNames(typeof(InputType)).ToList();
		inputTypeDropdown.AddOptions(inputTypeList);

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);

		// Don't run if UserID isn't set
		/*		if(userID == ""){
			Debug.LogError("Please set User ID");
			EditorApplication.ExecuteMenuItem("Edit/Play");
		}
		 */


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

		_userID = GameObject.Find("ConnectToArduino").GetComponent<ConnectToArduino>().email;
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
	public static void NewEntry(GameType _gameType, 

						 string _hitType,
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

		string b = System.Enum.GetName(typeof(GameType), _gameType);         //To Get the name of the enumerator

		logs["Email"].Add("hello@email.test");
		logs["Date"].Add(date);
		logs["Time"].Add(time);
		logs["UserID"].Add(_userID.ToString());
		logs["GameType"].Add(System.Enum.GetName(typeof(GameType), _gameType));
		logs["InputType"].Add(System.Enum.GetName(typeof(InputType), _inputType));
		logs["InputResponders"].Add(System.Enum.GetName(typeof(InputResponders), _inputResponders));
		logs["HitType"].Add(_hitType);
		logs["TargetNumber"].Add(_targetNumber.ToString());
		logs["TargetID"].Add(_targetID.ToString());
		logs["SessionTime"].Add(_sessionTime.ToString());
		logs["DeltaTime"].Add(_deltaTime.ToString());
		logs["TargetX"].Add(_targetPos.x.ToString());
		logs["TargetY"].Add(_targetPos.y.ToString());
		logs["HitX"].Add(_hitPos.x.ToString());
		logs["HitY"].Add(_hitPos.y.ToString());
		logs["HitOffsetX"].Add(_hitPos.x.ToString());
		logs["HitOffsetY"].Add(_hitPos.y.ToString());
		logs["OutsetTargetX"].Add(_outsetTarget.x.ToString());
		logs["OutsetTargetY"].Add(_outsetTarget.y.ToString());
		logs["TargetDeltaX"].Add(_targetPos.x.ToString());
		logs["TargetDeltaY"].Add(_targetPos.y.ToString());
		logs["OutsetHitX"].Add(_outsetHit.x.ToString());
		logs["OutsetHitY"].Add(_outsetHit.y.ToString());
		logs["DeltaHitX"].Add(_hitPos.x.ToString());
		logs["DeltaHitY"].Add(_hitPos.y.ToString());
		logs["TargetDiameter"].Add(_diameter.ToString());
		logs["ColliderDiameter"].Add(_diameter.ToString());
		logs["Backtracking"].Add(_backtracking.ToString());
		logs["ErrorTargetID"].Add(_errorTargetID.ToString());
		

		
		
		}

		public void sendLogs(){              //Send the logs

		connectToMySQL.AddToUploadQueue(logs);  //
		connectToMySQL.UploadNow();			//

		resetLogs();
		}
		
		private void resetLogs(){     //reset the logs

		foreach (List<string> Entry in logs.Values ) {   //for each value in the dictionary 
			Entry.Clear();           //clear the value
		}
		}

	public static void NewLog() {
		/*
		fileName = System.DateTime.Now.ToString() + ".csv";
		fileName = fileName.Replace ('/', '-');
		fileName = fileName.Replace (':', '-');
		*/
		fileName = "rtii_output.csv";
		if (!File.Exists(directory + fileName)) {
			using (StreamWriter writer = File.AppendText(directory + fileName))
			{
				writer.WriteLine(headers);
			}
		}
	}
}