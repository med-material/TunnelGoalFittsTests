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

    public float _cdgain;
    public float _targetWidthCm;
    public float _targetDistanceCm;
    public float _screenWidthCm;
    public float _screenHeightCm;
    public float _objectWidthCm;
    public float _objectHeightCm;
    public float _objectDistanceCm;

    public bool ArduinoGSR = false;
    public bool ArduinoPulse = false;

    public AudioClip _successClip;
    public AudioClip _errorClip;
    public AudioClip _missClip;

    private int D_fitts, S_fitts;
    private int D_tunnel, S_tunnel;
    private int D_goal, S_goal;
    private GameType gameType;

    private GameObject fittsTargetPrefab;
    private GameObject tunnelTargetPrefab;
    private GameObject goalTargetPrefab;
    private GameObject tunnelBarPrefab;

    private int rounds;
    private float cdgain;
    private float targetWidthCm;
    private float targetDistanceCm;
    private float screenWidthCm;
    private float screenHeightCm;
    private float objectWidthCm;
    private float objectHeightCm;
    private float objectDistanceCm;

    private Vector4[] targetAttributes;
    private AudioClip successClip;
    private AudioClip errorClip;
    private AudioClip missClip;

    private bool inGame = false;

    private FittsTarget[] allFittsTarget;
    private GoalTarget[] allGoalTarget;
    private TunnelTarget[] allTunnelTarget;
    private TunnelBar[] allTunnelBars;
    private GameObject[] allTargetObjects;
    private GameObject[] allBarObjects;

    private int totalTargets;
    private int currentTarget;
    private int currentRound;
    private int errorTargetID;

    private AudioSource missSound;
    private AudioSource successSound;
    private AudioSource errorSound;

    private float startTime;
    private float hitTime;
    private Vector2 hitPos;
    private Vector2 outsetTarget;
    private Vector2 outsetHit;
    private Vector2 lastSuccessfulHit;
    private int targetNumber;
    private Vector3 diameterVector;

    private bool backtracking = false;
    
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
    private InputField screenWidthinput;
    [SerializeField]
    private InputField screenHeightInput;

    [SerializeField]
    private InputManager inputManager;
    
    [SerializeField]
    private GameObject _uicanvas;
    private GameObject uicanvas;

   [SerializeField]
    private Text customGameInstructions;

    private string GameInstructionsTemplate;

    private string dateId = "";

    public LoggingManager _loggingManager;
    private LoggingManager loggingManager;

    [SerializeField]
    private Cursor cursor;

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
        loggingManager = _loggingManager;

        uicanvas = _uicanvas;

        rounds = _rounds;
        targetWidthCm = _targetWidthCm;
        targetDistanceCm = _targetDistanceCm;
        cdgain = _cdgain;
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

        screenWidthCm = (Screen.width / Screen.dpi) * 2.54f;
		Debug.Log("width: " + Screen.width + ", dpi:" + Screen.dpi + ", widthCm: " +screenWidthCm);
		screenHeightCm = (Screen.height / Screen.dpi) * 2.54f;
		Debug.Log("height: " + Screen.height + ", dpi:" + Screen.dpi + ", heightCm: " +screenHeightCm);
        screenWidthinput.text = screenWidthCm.ToString("0.00");
        screenHeightInput.text = screenHeightCm.ToString("0.00"); 
        size_onSliderChanged();
        distance_onSliderChanged();

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
        screenWidthCm = float.Parse(text);
    }

    public void screenHeight_onInputChanged(string text) {
        screenHeightCm = float.Parse(text);
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

    public void rounds_onInputChanged(string text) {
        rounds = int.Parse(text);
    }

    public bool GetInGame()
    {

        return inGame;
    }

    public GameType GetGameType()
    {

        return gameType;
    }

    public void StartGame()
    {
        cursor.gameObject.SetActive(true);
        cursor.ResetPosition();
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
            loggingManager.NewLog();
            //LoggingManager.NewLog();
            startTime = Time.time;
            hitTime = Time.time;

            currentTarget = 0;
            currentRound = 0;
            targetNumber = 0;
        }
    }

    private void LoadAutomateJSON()
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

    public void EndGame()
    {
        cursor.gameObject.SetActive(false);
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

        loggingManager.sendLogs();
    }

    public void ReturnMainMenu() {
        uicanvas.SetActive(true);

        if (gameType == GameType.Fitts)
            PrepareFittsGame();
        else if (gameType == GameType.Tunnel)
            PrepareTunnelGame();
        else if (gameType == GameType.Goal)
            PrepareGoalGame();
    }

    public int GetCurrentTarget()
    {

        return currentTarget;
    }

    public void SuccesfulHit()
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

    public void ErrorHit(int _errorHitID)
    {

        //errorSound.Play();

        errorTargetID = _errorHitID;

        MakeLogEntry("Error");
    }

    public void Miss()
    {

        missSound.Play();

        if (inGame)
        {
            MakeLogEntry("Miss");
        }
    }

    public void OutOfTunnel()
    {

        errorSound.Play();
        MakeLogEntry("LeftTunnel");
    }

    public void BackTrack()
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

    public void SetHitPosition(Vector2 _hitPos)
    {

        outsetHit = hitPos;
        hitPos = _hitPos;
    }

    public Vector4 GetTargetAttributes(int _index)
    {

        return targetAttributes[_index];
    }

    public FittsTarget GetFittsTarget(int _targetNumber)
    {

        return allFittsTarget[_targetNumber];
    }

    public GoalTarget GetGoalTarget(int _targetNumber)
    {
        return allGoalTarget[_targetNumber];
    }

    public TunnelTarget GetTunnelTarget(int _targetNumber)
    {
        return allTunnelTarget[_targetNumber];
    }

    public int GetTotalTargets()
    {

        return totalTargets;
    }

    public TunnelBar GetTunnelBars(int _barNum)
    {
        return allTunnelBars[_barNum];
    }

    private void MakeLogEntry(string _hitType)
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

        //Debug.Log("gametype: " + gameType);
        //Debug.Log("hittype: " + _hitType);
        //Debug.Log("targetnumber: " + targetNumber);
        //Debug.Log("currenttarget: " + currentTarget);
        //Debug.Log("time minus start: " + (Time.time - startTime).ToString());
        //Debug.Log("time minus hit: " + (Time.time - hitTime).ToString());
        //Debug.Log("alltargetobjects transform pos: " + allTargetObjects[currentTarget].transform.position);
        //Debug.Log("hit pos: " + hitPos);
        //Debug.Log("targetatt current target z: " + targetAttributes[currentTarget].z);
        //Debug.Log("targetatt current target w: " + targetAttributes[currentTarget].w);
        //Debug.Log("outsettarget: " + outsetTarget);
        //Debug.Log("outsethit: " + outsetHit);
        //Debug.Log("backtracking: " + backtracking);
        //Debug.Log("errortargetid: " + errorTargetID);
        //Debug.Log("dist: " + dist);
        //Debug.Log("objwidthcm: " + objectWidthCm);
        //Debug.Log("objheightcm: " + objectHeightCm);
        //Debug.Log("objdistcm: " + objectDistanceCm);
        //Debug.Log("LoggingManager: " + loggingManager);


        loggingManager.NewEntry(gameType, _hitType,
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

    private void PrepareFittsGame()
    {
        // For Fitts game we use a power function to manipulate size of objects.
        // 0.1 x ^ 1.3
        // This is to allow us to go to very small sizes for human device resolutions purposes.
        targetAttributes = new Vector4[2];
        targetAttributes[0].x = ((D_fitts / 2) + (0.1f * Mathf.Pow(S_fitts,1.3f) / 2));
        targetAttributes[0].y = 0;
        targetAttributes[0].z = (0.1f * Mathf.Pow(S_fitts,1.3f));
        targetAttributes[0].w = 100;

        targetAttributes[1].x = -((D_fitts / 2) + (0.1f * Mathf.Pow(S_fitts,1.3f) / 2));
        targetAttributes[1].y = 0;
        targetAttributes[1].z = (0.1f * Mathf.Pow(S_fitts,1.3f));
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

    private Bounds CameraBounds()
    {
        float ar = (float)Screen.width / (float)Screen.height;
        float camHeight = Camera.main.orthographicSize * 2;
        Bounds bounds = new Bounds(Camera.main.transform.position, new Vector3(camHeight * ar, camHeight, 0));
        return bounds;
    }

    private void CalculateWidthCm(Bounds bounds)
    {
        //Debug.Log("minBounds: " + bounds.min.x);
        //Debug.Log("maxBounds: " + bounds.max.x);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        //Debug.Log("origin: " + origin.ToString());
        //Debug.Log("extent: " + extent.ToString());
        float objectWidthScreen = extent.x - origin.x;
        if (screenWidthCm != -1f) {
            float centimetersPerPixel = screenWidthCm / Screen.width;
            objectWidthCm = objectWidthScreen * centimetersPerPixel;
        } else {
            objectWidthCm = -1f;
        }
        //Debug.Log("objectWidthScreen: " + objectWidthScreen + ", Screen.width: " + Screen.width + ", objectWidthCm: " + objectWidthCm);
    }

    private void CalculateHeightCm(Bounds bounds)
    {
        //Debug.Log("minBounds: " + bounds.min.y);
        //Debug.Log("maxBounds: " + bounds.max.y);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        //Debug.Log("origin: " + origin.ToString());
        //Debug.Log("extent: " + extent.ToString());
        float objectHeightScreen = extent.y - origin.y;
        if (screenHeightCm != -1f) {
            float centimetersPerPixel = screenHeightCm / Screen.height;
            objectHeightCm = objectHeightScreen * centimetersPerPixel;
        } else {
            objectHeightCm = -1f;
        }
        //Debug.Log("objectHeightScreen: " + objectHeightScreen + ", screen.height: " + Screen.height + ", objectHeightCm: " + objectHeightCm);
    }

    private void CalculateDistanceCm(Bounds bounds2, Bounds bounds1)
    {
        //Debug.Log("minBounds: " + bounds1.max.x);
        //Debug.Log("maxBounds: " + bounds2.min.x);
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds1.max.x, bounds1.min.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds2.min.x, bounds2.max.y, 0f));
		//Vector3 minBoundsScreen = Camera.main.WorldToScreenPoint(bounds.min);
        //Vector3 maxBoundsScreen = Camera.main.WorldToScreenPoint(bounds.max);
        //Debug.Log("origin: " + origin.ToString());
        //Debug.Log("extent: " + extent.ToString());
        float objectDistanceScreen = extent.x - origin.x;
        if (screenWidthCm != -1f) {
            float centimetersPerPixel = screenWidthCm / Screen.width;
            objectDistanceCm = objectDistanceScreen * centimetersPerPixel;
        } else {
            objectDistanceCm = -1f;
        }
        //Debug.Log("objectDistanceScreen: " + objectDistanceScreen + ", screen.width: " + Screen.width + ", objectDistanceCm: " + objectDistanceCm);
    }


    private void PrepareTunnelGame()
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

    private void PrepareGoalGame()
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

    //    private void PrepareBullseyeGame() {
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
    //	private void PrepareLineGame() {
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