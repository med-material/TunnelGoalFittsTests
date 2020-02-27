using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//public enum GameType {Bullseye, Line, Fitts, Tunnel, Goal };
public enum GameType
{
    Fitts,
    Tunnel,
    Goal,
    Custom
}
;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;

    public GameType _gameType;

    [Range(10, 150)]
    public int _D_fitts;
    [Range(1, 100)]
    public int _S_fitts;
    [Range(10, 150)]
    public int _D_tunnel;
    [Range(1, 100)]
    public int _S_tunnel;
    [Range(10, 150)]
    public int _D_goal;
    [Range(1, 100)]
    public int _S_goal;
    public int _rounds;

    public float _targetWidthCm;
    public float _targetDistanceCm;
    public float _screenWidthCm;
    public float _screenHeightCm;
    public float _screenDPICm;
    public float _objectWidthCm;
    public float _objectHeightCm;
    public float _objectDistanceCm;

    public bool ArduinoGSR = false;
    public bool ArduinoPulse = false;

    public AudioClip _successClip;
    public AudioClip _errorClip;
    public AudioClip _missClip;

    private static int D_fitts, S_fitts;
    private static int D_tunnel, S_tunnel;
    private static int D_goal, S_goal;
    private static GameType gameType;

    private static GameObject fittsTargetPrefab;
    private static GameObject tunnelTargetPrefab;
    private static GameObject goalTargetPrefab;
    private static GameObject tunnelBarPrefab;

    private static int rounds;
    private static float targetWidthCm;
    private static float targetDistanceCm;
    private static float screenWidthCm;
    private static float screenHeightCm;
    private static float screenDPI;
    private static float objectWidthCm;
    private static float objectHeightCm;
    private static float objectDistanceCm;

    private static Vector4[] targetAttributes;
    private static AudioClip successClip;
    private static AudioClip errorClip;
    private static AudioClip missClip;

    private static bool inGame = false;

    private static FittsTarget[] allFittsTarget;
    private static GoalTarget[] allGoalTarget;
    private static TunnelTarget[] allTunnelTarget;
    private static TunnelBar[] allTunnelBars;
    private static GameObject[] allTargetObjects;
    private static GameObject[] allBarObjects;

    private static int totalTargets;
    private static int currentTarget;
    private static int currentRound;
    private static int errorTargetID;

    private static AudioSource missSound;
    private static AudioSource successSound;
    private static AudioSource errorSound;

    private static float startTime;
    private static float hitTime;
    private static Vector2 hitPos;
    private static Vector2 outsetTarget;
    private static Vector2 outsetHit;
    private static Vector2 lastSuccessfulHit;
    private static int targetNumber;
    private static Vector3 diameterVector;

    private static bool backtracking = false;
    
    // UI
    [SerializeField]
    private Slider sizeSlider;
    [SerializeField]
    private Text sizeNumber;

    [SerializeField]
    private Slider distanceSlider;
    [SerializeField]
    private Text distanceNumber;
    [SerializeField]
    private Text roundsNumber;
    [SerializeField]
    private Slider roundsSlider;

    [SerializeField]
    private InputField screenWidthinput;
    [SerializeField]
    private InputField screenHeightInput;

    [SerializeField]
    private InputField screenDPIInput;

    [SerializeField]
    private InputManager inputManager;
    
    [SerializeField]
    private GameObject _uicanvas;
    private static GameObject uicanvas;

   [SerializeField]
    private Text customGameInstructions;

    private string GameInstructionsTemplate;

    private static string dateId = "";

    void Awake()
    {
        GameInstructionsTemplate = customGameInstructions.text;
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);

        DontDestroyOnLoad(this);

        gameType = _gameType;


        D_fitts = _D_fitts;
        S_fitts = _S_fitts;
        D_tunnel = _D_tunnel;
        S_tunnel = _S_tunnel;
        D_goal = _D_goal;
        S_goal = _S_goal;

        uicanvas = _uicanvas;

        rounds = _rounds;
        targetWidthCm = _targetWidthCm;
        targetDistanceCm = _targetDistanceCm;
        successClip = _successClip;
        errorClip = _errorClip;
        missClip = _missClip;

        successSound = gameObject.AddComponent<AudioSource>();
        successSound.clip = successClip;

        errorSound = gameObject.AddComponent<AudioSource>();
        errorSound.clip = errorClip;

        missSound = gameObject.AddComponent<AudioSource>();
        missSound.clip = missClip;
    }

    void Start()
    {

        //		if (gameType == GameType.Bullseye)
        //			PrepareBullseyeGame ();
        //		else if (gameType == GameType.Line)
        //			PrepareLineGame ();

        //      if (TunnelManager.GetTunnelOn()) {
        //	TunnelManager.PrepareTunnel ();
        //}

        //startButton = GameObject.Find("StartButton").GetComponent<StartButton>();
        LoadAutomateJSON();
        S_tunnel = (int) sizeSlider.value;
        S_goal = (int) sizeSlider.value;
        S_fitts = (int) sizeSlider.value;
        D_fitts = (int) distanceSlider.value;
        D_goal = (int) distanceSlider.value;
        D_tunnel = (int) distanceSlider.value;
        rounds = (int) roundsSlider.value;

        screenWidthCm = (Screen.width / Screen.dpi) * 2.54f;
		Debug.Log("width: " + Screen.width + ", dpi:" + Screen.dpi + ", widthCm: " +screenWidthCm);
		screenHeightCm = (Screen.height / Screen.dpi) * 2.54f;
		Debug.Log("height: " + Screen.height + ", dpi:" + Screen.dpi + ", heightCm: " +screenHeightCm);
        screenWidthinput.text = screenWidthCm.ToString("0.00");
        screenHeightInput.text = screenHeightCm.ToString("0.00"); 
        screenDPI = Screen.dpi;

        PrepareGoalGame();
    }

    void Update()
    {

        if (inGame) {
            inputManager.CheckInput();
            //if (TunnelManager.GetTunnelOn ())
            //	TunnelManager.CheckTunnel ();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void QuitApplication() {
        Application.Quit();
    }

    public void gameTypeToggleTunnel() {
        if (allTargetObjects != null && allTargetObjects.Length > 0) {
            EndGame();
        }
        customGameInstructions.gameObject.SetActive(false);
        gameType = GameType.Tunnel;
        S_tunnel = (int) sizeSlider.value;
        D_tunnel = (int) distanceSlider.value;
        PrepareTunnelGame();
    }

    public void gameTypeToggleFitts() {
        if (allTargetObjects != null && allTargetObjects.Length > 0) {
            EndGame();
        }
        customGameInstructions.gameObject.SetActive(false);
        gameType = GameType.Fitts;
        S_fitts = (int) sizeSlider.value;
        D_fitts = (int) distanceSlider.value;
        PrepareFittsGame();
    }

    public void gameTypeToggleGoal() {
        if (allTargetObjects != null && allTargetObjects.Length > 0) {
            EndGame();
        }
        customGameInstructions.gameObject.SetActive(false);
        gameType = GameType.Goal;
        S_goal = (int) sizeSlider.value;
        D_goal = (int) distanceSlider.value;
        PrepareGoalGame();
    }

    public void gameTypeToggleCustom() {
        if (allTargetObjects != null && allTargetObjects.Length > 0) {
            EndGame();
        }
        gameType = GameType.Custom;
        PrepareCustomGame();
    }

    public void screenWidth_onInputChanged(string text) {
        if (text != "") {
        Debug.Log(text);
        screenWidthCm = float.Parse(text);
        } else {
            screenWidthinput.text = "-1";
        }
    }

    public void screenDPI_onInputChanged(string text) {
        if (text != "") {
        Debug.Log(text);
        screenDPI = float.Parse(text);
        } else {
            screenDPIInput.text = "-1";
        }
    }    

    public void screenHeight_onInputChanged(string text) {
        if (text != "") {
        Debug.Log(text);
        screenHeightCm = float.Parse(text);
        } else {
            screenHeightInput.text = "-1";
        }
    }

    public void size_onSliderChanged() {
        sizeNumber.text = ((int)sizeSlider.value).ToString();

        if (gameType == GameType.Fitts) {
            S_fitts = (int) sizeSlider.value;
            PrepareFittsGame();
        } else if (gameType == GameType.Goal) {
            S_goal = (int) sizeSlider.value;
            PrepareGoalGame();
        } else if (gameType == GameType.Tunnel) {
            S_tunnel = (int) sizeSlider.value;
            PrepareTunnelGame();
        }
    }

    public void distance_onSliderChanged() {
        distanceNumber.text = ((int)distanceSlider.value).ToString();
        if (gameType == GameType.Fitts) {
            D_fitts = (int) distanceSlider.value;
            PrepareFittsGame();
        } else if (gameType == GameType.Goal) {
            D_goal = (int) distanceSlider.value;
            PrepareGoalGame();
        } else if (gameType == GameType.Tunnel) {
            D_tunnel = (int) distanceSlider.value;
            PrepareTunnelGame();
        }
    }

    public void rounds_onSliderChanged() {
        roundsNumber.text = ((int)roundsSlider.value).ToString();
        rounds = (int)roundsSlider.value;
    }

    public static bool GetInGame()
    {

        return inGame;
    }

    public static GameType GetGameType()
    {

        return gameType;
    }

    public void StartGame()
    {

        if (gameType == GameType.Fitts)
            PrepareFittsGame();
        else if (gameType == GameType.Tunnel)
            PrepareTunnelGame();
        else if (gameType == GameType.Goal)
            PrepareGoalGame();
        else if (gameType == GameType.Custom)
            customGameInstructions.text = "Now logging the Arduino..";
        
        dateId = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (targetAttributes.Length > 0)
        {
            inGame = true;
            //startButton.Disappear();
            LoggingManager.NewLog();
            startTime = Time.time;
            hitTime = Time.time;

            currentTarget = 0;
            currentRound = 0;
            targetNumber = 0;
        }
    }

    private static void LoadAutomateJSON()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        //string filePath = Path.Combine(Application., "Automate.json");
        TextAsset jsonFile = Resources.Load<TextAsset>("Automate");

        if (jsonFile != null)
        {
            Debug.Log(jsonFile);
            //Debug.Log(JsonUtility.FromJson<T>(jsonFile);

        }
        else
        {
            Debug.Log("Did not find Automate script.");
        }
    }

    public static void EndGame()
    {

        inGame = false;
        //startButton.Appear();
        for (int i = 0; i < allTargetObjects.Length; i++)
        {
            Destroy(allTargetObjects[i]);
        }
        if (allBarObjects != null && allBarObjects.Length > 0) {
            for (int i = 0; i < allBarObjects.Length; i++) {
                Destroy(allBarObjects[i]);
            }
        }
        if (allTunnelTarget != null && allTunnelTarget.Length > 0) {
            for (int i = 0; i < allTunnelTarget.Length; i++) {
                Destroy(allTunnelTarget[i]);
            }
        }
        allTunnelTarget = null;
        allBarObjects = null;
        allTargetObjects = null;

        GameObject.Find("Managers").GetComponent<LoggingManager>().sendLogs();
    }

    public static void ReturnMainMenu() {
        uicanvas.SetActive(true);

        if (gameType == GameType.Fitts)
            PrepareFittsGame();
        else if (gameType == GameType.Tunnel)
            PrepareTunnelGame();
        else if (gameType == GameType.Goal)
            PrepareGoalGame();
    }

    public static int GetCurrentTarget()
    {

        return currentTarget;
    }

    public static void SuccesfulHit()
    {
        successSound.Play();

        MakeLogEntry("Hit");
        hitTime = Time.time;

        outsetTarget = allTargetObjects[currentTarget].transform.position;

        currentTarget++;
        targetNumber = currentRound * totalTargets + currentTarget;

        backtracking = false;

        if (currentTarget > totalTargets - 1)
        {

            currentTarget = 0;
            currentRound++;

            if (currentRound > rounds - 1)
            {

                EndGame();
                ReturnMainMenu();
            }
        }
        if (inGame) {
            if (gameType == GameType.Fitts)
            {
                allTargetObjects[currentTarget].GetComponent<FittsTarget>().SetActiveTarget();
            }
            else if (gameType == GameType.Goal)
            {
                allTargetObjects[currentTarget].GetComponent<GoalTarget>().SetActiveTarget();
            }
            else if (gameType == GameType.Tunnel){
                allTargetObjects[currentTarget].GetComponent<TunnelTarget>().SetActiveTarget();
            }
        }
    }

    public static void ErrorHit(int _errorHitID)
    {

        errorSound.Play();

        errorTargetID = _errorHitID;

        MakeLogEntry("Error");
    }

    public static void Miss()
    {

        missSound.Play();

        if (inGame)
        {
            MakeLogEntry("Miss");
        }
    }

    public static void OutOfTunnel()
    {

        errorSound.Play();
        MakeLogEntry("LeftTunnel");
    }

    public static void BackTrack()
    {

        if (!backtracking)
        {

            backtracking = true;

            if (targetNumber > 0)
            {

                currentTarget--;

                if (currentTarget < 0)
                {
                    currentTarget = totalTargets - 1;
                    currentRound--;
                }

                targetNumber = currentRound * totalTargets + currentTarget;
            }
        }
    }

    public static void SetHitPosition(Vector2 _hitPos)
    {

        outsetHit = hitPos;
        hitPos = _hitPos;
    }

    public static Vector4 GetTargetAttributes(int _index)
    {

        return targetAttributes[_index];
    }

    public static FittsTarget GetFittsTarget(int _targetNumber)
    {

        return allFittsTarget[_targetNumber];
    }

    public static GoalTarget GetGoalTarget(int _targetNumber)
    {
        return allGoalTarget[_targetNumber];
    }

    public static TunnelTarget GetTunnelTarget(int _targetNumber)
    {
        return allTunnelTarget[_targetNumber];
    }

    public static int GetTotalTargets()
    {

        return totalTargets;
    }

    public static TunnelBar GetTunnelBars(int _barNum)
    {
        return allTunnelBars[_barNum];
    }

    private static void MakeLogEntry(string _hitType)
    {

        int dist = -1;

       

        if (gameType == GameType.Tunnel)
        {
            dist = D_tunnel;
        }
        else if (gameType == GameType.Fitts)
        {
            dist = D_fitts;
        }
        else if (gameType == GameType.Goal)
        {
            dist = D_goal;
        }

        

        LoggingManager.NewEntry(gameType, _hitType,
            targetNumber,
            currentTarget,
            Time.time - startTime,
            Time.time - hitTime,
            allTargetObjects[currentTarget].transform.position,
            hitPos,
            targetAttributes[currentTarget].z,
            targetAttributes[currentTarget].w,
            outsetTarget,
            outsetHit,
            backtracking,
            errorTargetID,
            dist,
            objectWidthCm,
            objectHeightCm,
            objectDistanceCm);
    }

    private static void PrepareFittsGame()
    {
        targetAttributes = new Vector4[2];
        targetAttributes[0].x = ((D_fitts / 2) + (S_fitts / 2));
        targetAttributes[0].y = 0;
        targetAttributes[0].z = S_fitts;
        targetAttributes[0].w = 100;

        targetAttributes[1].x = -((D_fitts / 2) + (S_fitts / 2));
        targetAttributes[1].y = 0;
        targetAttributes[1].z = S_fitts;
        targetAttributes[1].w = 100;

        totalTargets = 2;
        if (allTargetObjects == null) {
            allFittsTarget = new FittsTarget[totalTargets];
            allTargetObjects = new GameObject[totalTargets];
            Debug.Log("Preparing Fitts Game");

            for (int i = 0; i < totalTargets; i++)
            {
                allTargetObjects[i] = Instantiate(Resources.Load("Prefabs/FittsTarget", typeof(GameObject)), new Vector2(targetAttributes[i].x, targetAttributes[i].y), Quaternion.identity) as GameObject;

                allFittsTarget[i] = allTargetObjects[i].GetComponent<FittsTarget>();
                allFittsTarget[i].SetTargetID(i);
            }
            allTargetObjects[0].GetComponent<FittsTarget>().SetActiveTarget();
        }
        for (int i = 0; i < totalTargets; i++) {
            allTargetObjects[i].transform.position = new Vector2(targetAttributes[i].x, targetAttributes[i].y);
            allFittsTarget[i].SetSize(S_fitts, 100);
        }
        CalculateWidthCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds);
        CalculateHeightCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds);
        CalculateDistanceCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds, allTargetObjects[1].GetComponentsInChildren<Renderer>()[0].bounds);
    }

    private static Bounds CameraBounds()
    {
        float ar = (float)Screen.width / (float)Screen.height;
        float camHeight = Camera.main.orthographicSize * 2;
        Bounds bounds = new Bounds(Camera.main.transform.position, new Vector3(camHeight * ar, camHeight, 0));
        return bounds;
    }

    private static void CalculateWidthCm(Bounds bounds)
    {
        Debug.Log("minBounds: " + bounds.min.x);
        Debug.Log("maxBounds: " + bounds.max.x);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        Debug.Log("origin: " + origin.ToString());
        Debug.Log("extent: " + extent.ToString());
        float objectWidthScreen = extent.x - origin.x;
        if (screenDPI == 0f) {
            objectWidthCm = -1f;
            return;
        }
        float objectWidthInches = objectWidthScreen / screenDPI;
        objectWidthCm = objectWidthInches * 2.54f;
        Debug.Log("objectWidthScreen: " + objectWidthScreen + ", objectWidthInches: " + objectWidthInches + ", objectWidthCm: " + objectWidthCm);
    }

    private static void CalculateHeightCm(Bounds bounds)
    {
        Debug.Log("minBounds: " + bounds.min.y);
        Debug.Log("maxBounds: " + bounds.max.y);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        Debug.Log("origin: " + origin.ToString());
        Debug.Log("extent: " + extent.ToString());
        float objectHeightScreen = extent.y - origin.y;
        if (screenDPI == 0f) {
            objectHeightCm = -1f;
            return;
        }        
        float objectHeightInches = objectHeightScreen / screenDPI;
        objectHeightCm = objectHeightInches * 2.54f;
        Debug.Log("objectHeightScreen: " + objectHeightScreen + ", objectHeightInches: " + objectHeightInches + ", objectHeightCm: " + objectHeightCm);
    }

    private static void CalculateDistanceCm(Bounds bounds2, Bounds bounds1)
    {
        Debug.Log("minBounds: " + bounds1.max.x);
        Debug.Log("maxBounds: " + bounds2.min.x);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds1.max.x, bounds1.min.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds2.min.x, bounds2.max.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        Debug.Log("origin: " + origin.ToString());
        Debug.Log("extent: " + extent.ToString());
        float objectDistanceScreen = extent.x - origin.x;
        if (screenDPI == 0f) {
            objectDistanceCm = -1f;
            return;
        }
        float objectDistanceInches = objectDistanceScreen / screenDPI;
        objectDistanceCm = objectDistanceInches * 2.54f;
        Debug.Log("objectDistanceScreen: " + objectDistanceScreen + ", objectDistanceInches: " + objectDistanceInches + ", objectDistanceCm: " + objectDistanceCm);
    }


    private static void PrepareTunnelGame()
    {

        targetAttributes = new Vector4[2];
        var tunnelgoalSize = 3;
        targetAttributes[0].x = ((D_tunnel / 2) + (tunnelgoalSize / 2));
        targetAttributes[0].y = 0;
        targetAttributes[0].z = 10;
        targetAttributes[0].w = 100;

        targetAttributes[1].x = -((D_tunnel / 2) + (tunnelgoalSize / 2));
        targetAttributes[1].y = 0;
        targetAttributes[1].z = 10;
        targetAttributes[1].w = 100;

        Bounds b = CameraBounds();
        float barPos = ((b.extents.y - (S_tunnel / 2))/2) + (S_tunnel / 2);
        float barHeight = (b.extents.y - (S_tunnel / 2));

        if (allTargetObjects == null) {
            totalTargets = 2;
            allTunnelTarget = new TunnelTarget[totalTargets];
            allTargetObjects = new GameObject[totalTargets];
            allTunnelBars = new TunnelBar[2];
            allBarObjects = new GameObject[2];
            Debug.Log("Preparing Tunnel Game");

            allBarObjects[0] = Instantiate(Resources.Load("Prefabs/TunnelBar", typeof(GameObject)), new Vector2(0, barPos), Quaternion.identity) as GameObject;
            allTunnelBars[0] = allBarObjects[0].GetComponent<TunnelBar>();
            allBarObjects[1] = Instantiate(Resources.Load("Prefabs/TunnelBar", typeof(GameObject)), new Vector2(0, -barPos), Quaternion.identity) as GameObject;
            allTunnelBars[1] = allBarObjects[1].GetComponent<TunnelBar>();
        }

        allTunnelBars[0].SetPosition((int) barPos);
        allTunnelBars[1].SetPosition((int) -barPos);

        Debug.Log("CamRect: " + CameraBounds());
        Debug.Log("allTargetObjects  count: " + allTargetObjects.Length);

        if (allTargetObjects[0] == null) {
            for (int i = 0; i < totalTargets; i++)
            {
                allTargetObjects[i] = Instantiate(Resources.Load("Prefabs/TunnelTarget", typeof(GameObject)), new Vector2(targetAttributes[i].x, targetAttributes[i].y), Quaternion.identity) as GameObject;
                allTunnelTarget[i] = allTargetObjects[i].GetComponent<TunnelTarget>();
                allTunnelTarget[i].SetTargetID(i);
            }
            allTargetObjects[0].GetComponent<TunnelTarget>().SetActiveTarget();
        }
        for (int i = 0; i < totalTargets; i++) {
            allTargetObjects[i].transform.position = new Vector2(targetAttributes[i].x, targetAttributes[i].y);
            allTunnelTarget[i].SetSize(tunnelgoalSize,100);
        }

        CalculateWidthCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds);
        Bounds upper_bound = allTunnelBars[0].GetComponentsInChildren<Renderer>()[0].bounds;
        Bounds lower_bound = allTunnelBars[1].GetComponentsInChildren<Renderer>()[0].bounds;
        Bounds tunnel_bounds = new Bounds(new Vector3(0,0,0), new Vector3(Mathf.Abs(upper_bound.min.x - upper_bound.max.x), Mathf.Abs(upper_bound.min.y - lower_bound.max.y), 0f));
        CalculateHeightCm(tunnel_bounds);
        CalculateDistanceCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds, allTargetObjects[1].GetComponentsInChildren<Renderer>()[0].bounds);

    }

    private static void PrepareGoalGame()
    {
        var tunnelgoalSize = 3;
        targetAttributes = new Vector4[2];
        targetAttributes[0].x = ((D_goal / 2) + (tunnelgoalSize / 2));
        targetAttributes[0].y = 0;
        targetAttributes[0].z = S_goal;
        targetAttributes[0].w = 100;

        targetAttributes[1].x = -((D_goal / 2) + (tunnelgoalSize / 2));
        targetAttributes[1].y = 0;
        targetAttributes[1].z = S_goal;
        targetAttributes[1].w = 100;

        totalTargets = 2;

        if (allTargetObjects == null) {
            allGoalTarget = new GoalTarget[totalTargets];
            allTargetObjects = new GameObject[totalTargets];
            Debug.Log("Preparing Goal Game");

            for (int i = 0; i < totalTargets; i++)
            {
                allTargetObjects[i] = Instantiate(Resources.Load("Prefabs/GoalTarget", typeof(GameObject)), new Vector2(targetAttributes[i].x, targetAttributes[i].y), Quaternion.identity) as GameObject;

                allGoalTarget[i] = allTargetObjects[i].GetComponent<GoalTarget>();
                allGoalTarget[i].SetTargetID(i);
            }
            allTargetObjects[0].GetComponent<GoalTarget>().SetActiveTarget();
        }
        for (int i = 0; i < totalTargets; i++)
        {
            allTargetObjects[i].transform.position = new Vector2(targetAttributes[i].x, targetAttributes[i].y);
            allGoalTarget[i].SetSize(tunnelgoalSize,S_goal);
        }

        CalculateWidthCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds);
        CalculateHeightCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds);
        CalculateDistanceCm(allTargetObjects[0].GetComponentsInChildren<Renderer>()[0].bounds, allTargetObjects[1].GetComponentsInChildren<Renderer>()[0].bounds);


    }

    private void PrepareCustomGame() {
        customGameInstructions.gameObject.SetActive(true);
        customGameInstructions.text = GameInstructionsTemplate;
    }

    //    private static void PrepareBullseyeGame() {
    //
    //		targetAttributes = bullseyeAttributes;
    //
    //		totalTargets = targetAttributes.Length;
    //
    //		allBullseyes = new Bullseye[totalTargets];
    //		allTargetObjects = new GameObject[totalTargets];
    //
    //		for (int i = 0; i < totalTargets; i++) {
    //
    //			allTargetObjects[i] = Instantiate(bullseyePrefab, new Vector2(targetAttributes[i].x, targetAttributes[i].y), Quaternion.identity) as GameObject;
    //
    //			diameterVector = new Vector3 (targetAttributes[i].z, targetAttributes[i].z, 1);
    //			allTargetObjects[i].transform.localScale = diameterVector;
    //
    //			allTargetObjects[i].GetComponent<CircleCollider2D>().radius = targetAttributes[i].w/targetAttributes[i].z/2;
    //
    //			allBullseyes[i] = allTargetObjects[i].GetComponent<Bullseye>();
    //			allBullseyes[i].SetTargetID(i);
    //		}
    //	}
    //
    //	private static void PrepareLineGame() {
    //
    //		targetAttributes = lineAttributes;
    //
    //		totalTargets = targetAttributes.Length;
    //
    //		allLines = new Line[totalTargets];
    //		allTargetObjects = new GameObject[totalTargets];
    //
    //		for (int i = 0; i < totalTargets; i++) {
    //
    //			allTargetObjects[i] = Instantiate(linePrefab, new Vector2(targetAttributes[i].x, targetAttributes[i].y), Quaternion.identity) as GameObject;
    //
    //			allLines[i] = allTargetObjects[i].GetComponent<Line>();
    //			allLines[i].SetTargetID(i);
    //
    //			allLines [i].SetAttributes (targetAttributes [i].z);
    //		}
    //	}
}