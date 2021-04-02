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
    dominantHand,
	nonDominantHand,
	rightHand,
    leftHand,
    bothHands,
    head,
    indexFinger,
    thumb,
    tongue,
    eyes,
    body,
    foot,
    knee,
    wrist,
	custom
}
;

public enum InputType
{
    mouse,
	mouseWin,
	mouseWinPrecis,
	mouseOSX,
	mouseOSXPrecis,
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
    gamePad,
	custom
}
;

public class LoggingManager : MonoBehaviour {

	public static LoggingManager instance = null; 

	public string userID;
	public string PID;
	public string cdgain;
	public string TrialNo;
	public string inputResponders;
	public string inputType;

	private string headers = "UserID;GameType;InputType;InputResponders;HitType;TargetNumber;TargetID;SessionTime;DeltaTime;TargetX;TargetY;HitX;HitY;HitOffsetX;HitOffsetY;OutsetTargetX;OutsetTargetY;TargetDeltaX;TargetDeltaY;OutsetHitX;OutsetHitY;DeltaHitX;DeltaHitY;TargetDiameter;ColliderDiameter;Backtracking;ErrorTargetID;TargetsDistance;Timestamp;PID;ObjectWidthCm;ObjectHeightCm;ObjectDistanceCm";

	private StreamWriter writer;
	private string directory;
	private string fileName;
	private string sep = ";";
	private string currentEntry;

	private string date;
	private string time;

	private Dictionary <string, List<string>> logs;

	private ConnectToMySQL connectToMySQL;
	// UI
	[SerializeField]
	private Dropdown inputResponderDropdown;

	[SerializeField]
	private Dropdown inputTypeDropdown;

	[SerializeField]
	private InputField emailField;

	[SerializeField]
	private InputField PIDField;

	[SerializeField]
	private InputField cdgainField;

	[SerializeField]
	private InputField TrialNoField;

	[SerializeField]
	private InputField inputTypeField;

	[SerializeField]
	private InputField inputResponderField;

	public void Awake() {

		connectToMySQL = FindObjectOfType<ConnectToMySQL>();
		PID = "1";
		TrialNo = "1";
		cdgain = "NULL";
		logs = new Dictionary<string, List<string>>() //create a new dictionary
		
		{
			{"Email", new List<string>()},
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
			{"TargetsDistance",new List<string>()},
			{"Timestamp", new List<string>()},
			{"PID", new List<string>()},
			{"ObjectWidthCm", new List<string>()},
			{"ObjectHeightCm", new List<string>()},
			{"ObjectDistanceCm", new List<string>()},
			{"TrialNo", new List<string>()},
			{"CDGain", new List<string>()}
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
			directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/tunnelgoalfitts/";
			print("Linux");
		} else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
			directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/tunnelgoalfitts/";
			print("Mac OSX");
		} else {
            directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/rtii/tunnelgoalfitts/";
            print("Unknown");
		}
/*
        directory = Application.dataPath + "/../Data/";
 */

        if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		userID = GameObject.Find("ConnectToArduino").GetComponent<ConnectToArduino>().email;
		inputType = System.Enum.GetName(typeof(InputType), inputTypeDropdown.value);
		inputResponders = System.Enum.GetName(typeof(InputResponders), inputResponderDropdown.value);
	}

	public void PID_Changed() {
		PID = PIDField.text;
		if (PID == "") {
			PID = "1";
		}
	}

	public void CDGain_Changed() {
		cdgain = cdgainField.text;
		if (cdgain == "") {
			cdgain = "NULL";
		}
	}

	public void Trial_Changed() {
		TrialNo = TrialNoField.text;
		if (TrialNo == "") {
			TrialNo = "NULL";
		}
	}

	public void emailField_Changed() {
		userID = emailField.text;
	}

	public void onInputType_Changed() {
		inputType = System.Enum.GetName(typeof(InputType), inputTypeDropdown.value);
		if ((InputType) inputTypeDropdown.value == InputType.custom) {
			inputTypeDropdown.gameObject.SetActive(false);
			inputTypeField.gameObject.SetActive(true);
		}
	}

	public void inputTypeField_Changed() {
		inputType = inputTypeField.text;
	}

	public void onInputResponder_Changed() {
		inputResponders = System.Enum.GetName(typeof(InputResponders), inputResponderDropdown.value);
		if ((InputResponders) inputResponderDropdown.value == InputResponders.custom) {
			inputResponderDropdown.gameObject.SetActive(false);
			inputResponderField.gameObject.SetActive(true);
		}
	}

	public void inputResponderField_Changed() {
		inputResponders = inputResponderField.text;
	}

	public void NewEntry(GameType gameType, 
						 string hitType,
	                     int targetNumber,
	                     int targetID, 
	                     float sessionTime, 
	                     float deltaTime, 
	                     Vector2 targetPos, 
	                     Vector2 hitPos, 
	                     float diameter,
	                     float collider,
	                     Vector2 outsetTarget, 
	                     Vector2 outsetHit,
						 bool backtracking,
						 int errorTargetID,
						 int dDist,
						 float objectWidthCm,
						 float objectHeightCm,
						 float objectDistanceCm) {

		date = System.DateTime.Now.ToString("yyyy-MM-dd");
		time = System.DateTime.Now.ToString("HH:mm:ss:ffff");
		string dateId = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");


		currentEntry = 	userID + sep +
                        gameType + sep +
                        inputType + sep +
                        inputResponders + sep +
						hitType + sep +
						targetNumber + sep +
						targetID + sep +
						sessionTime + sep +
						deltaTime + sep +
						targetPos.x + sep +
						targetPos.y + sep +
						hitPos.x + sep +
						hitPos.y + sep +
						(hitPos.x - targetPos.x) + sep +
						(hitPos.y - targetPos.y) + sep +
						outsetTarget.x + sep +
						outsetTarget.y + sep +
						(targetPos.x - outsetTarget.x) + sep +
						(targetPos.y - outsetTarget.y) + sep +
						outsetHit.x + sep +
						outsetHit.y + sep +
						(hitPos.x - outsetHit.x) + sep +
						(hitPos.x - outsetHit.y) + sep +
						diameter + sep +
						collider + sep +
						backtracking + sep +
						errorTargetID + sep +
						dDist + sep +
						dateId + sep +
						PID + sep +
						objectWidthCm + sep +
						objectHeightCm + sep +
						objectDistanceCm + sep +
						cdgain;
		Debug.Log(directory + fileName);
		using (StreamWriter writer = File.AppendText(directory + fileName))
		{
			writer.WriteLine(currentEntry);
		}

		string b = System.Enum.GetName(typeof(GameType), gameType);         //To Get the name of the enumerator

		logs["Email"].Add(userID.ToString());
		logs["GameType"].Add(System.Enum.GetName(typeof(GameType), gameType));
		logs["InputType"].Add(inputType);
		logs["InputResponders"].Add(inputResponders);
		logs["HitType"].Add(hitType);
		logs["TargetNumber"].Add(targetNumber.ToString());
		logs["TargetID"].Add(targetID.ToString());
		logs["SessionTime"].Add(sessionTime.ToString().Replace(',', '.'));
		logs["DeltaTime"].Add(deltaTime.ToString().Replace(',', '.'));
		logs["TargetX"].Add(targetPos.x.ToString().Replace(',', '.'));
		logs["TargetY"].Add(targetPos.y.ToString().Replace(',', '.'));
		logs["HitX"].Add(hitPos.x.ToString().Replace(',', '.'));
		logs["HitY"].Add(hitPos.y.ToString().Replace(',', '.'));
		logs["HitOffsetX"].Add(hitPos.x.ToString().Replace(',', '.'));
		logs["HitOffsetY"].Add(hitPos.y.ToString().Replace(',', '.'));
		logs["OutsetTargetX"].Add(outsetTarget.x.ToString().Replace(',', '.'));
		logs["OutsetTargetY"].Add(outsetTarget.y.ToString().Replace(',', '.'));
		logs["TargetDeltaX"].Add(targetPos.x.ToString().Replace(',', '.'));
		logs["TargetDeltaY"].Add(targetPos.y.ToString().Replace(',', '.'));
		logs["OutsetHitX"].Add(outsetHit.x.ToString().Replace(',', '.'));
		logs["OutsetHitY"].Add(outsetHit.y.ToString().Replace(',', '.'));
		logs["DeltaHitX"].Add(hitPos.x.ToString().Replace(',', '.'));
		logs["DeltaHitY"].Add(hitPos.y.ToString().Replace(',', '.'));
		logs["TargetDiameter"].Add(diameter.ToString().Replace(',', '.'));
		logs["ColliderDiameter"].Add(diameter.ToString().Replace(',', '.'));
		logs["Backtracking"].Add(backtracking.ToString());
		logs["ErrorTargetID"].Add(errorTargetID.ToString());
		logs["TargetsDistance"].Add(dDist.ToString().Replace(',', '.'));
		logs["Timestamp"].Add(dateId.ToString().Replace(',', '.'));
		logs["PID"].Add(PID.ToString());
		logs["ObjectWidthCm"].Add(objectWidthCm.ToString().Replace(',', '.'));
		logs["ObjectHeightCm"].Add(objectHeightCm.ToString().Replace(',', '.'));
		logs["ObjectDistanceCm"].Add(objectDistanceCm.ToString().Replace(',', '.'));
		logs["TrialNo"].Add(TrialNo.ToString());
		logs["CDGain"].Add(cdgain.ToString());

		
		}

		public void sendLogs(){              //Send the logs

		if (logs["Email"].Count == 0)
		return;

		connectToMySQL.AddToUploadQueue(logs);  //
		connectToMySQL.UploadNow();			//

		resetLogs();
		}
		
		private void resetLogs(){     //reset the logs

		foreach (List<string> Entry in logs.Values ) {   //for each value in the dictionary 
			Entry.Clear();           //clear the value
		}
		}

	public void NewLog() {
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
