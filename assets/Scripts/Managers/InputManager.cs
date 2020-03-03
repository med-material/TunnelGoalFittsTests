//#define PRINT_FUNC_CALL

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {

	public static InputManager instance = null;

	private  Collider2D hit;
	private Vector2 worldPosition;
	private Vector2 prevWorldPosition;

	private bool dragging;
	private float crossingY;

	private Vector4 target;

	private bool cross = false;

	[SerializeField]
	private Dropdown inputDropdown;

	[SerializeField]
	private Cursor cursor;

	private GameManager gameManager;
	private bool errorRecorded = false;

	void Awake() {
		gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);  
	}

	public void CheckPressure() {

        if (Input.GetKeyDown(KeyCode.Space) && gameManager.GetGameType() == GameType.Fitts)
            CheckHit(cursor.GetScreenPosition());
        else if (gameManager.GetGameType() == GameType.Goal)
        {
                CheckCrossing(cursor.GetScreenPosition());
        }
		else if (gameManager.GetGameType() == GameType.Tunnel){
			CheckTunnelCrossing(cursor.GetScreenPosition());
		}

	}

	public void inputType_onValueChanged() {
		// if (inputDropdown.value == (int) InputType.pressuresensor) {
		// 	cursor.gameObject.SetActive(true);
		// } else {
		// 	cursor.gameObject.SetActive(false);
		// }
	}

	public void CheckInput() {

		if (inputDropdown.value == (int) InputType.pressuresensor) {
			CheckPressure();
		} else if (SystemInfo.deviceType == DeviceType.Handheld) {

			CheckTouchInput();
		}
		else if(SystemInfo.deviceType == DeviceType.Desktop) {

			CheckPressure();
		}


	}

	private void CheckMouseInput() {

	
        if (Input.GetMouseButtonDown(0) && gameManager.GetGameType() == GameType.Fitts)
            CheckHit(cursor.GetScreenPosition());
        else if (gameManager.GetGameType() == GameType.Goal)
        {
                CheckCrossing(cursor.GetScreenPosition());
        }
		else if (gameManager.GetGameType() == GameType.Tunnel){
			CheckTunnelCrossing(cursor.GetScreenPosition());
		}
    }

	private void CheckPressureInput() {


	}

	private void CheckTouchInput() {

//		foreach (Touch touch in Input.touches) {
//			
//			if (touch.phase == TouchPhase.Began && gameManager.GetGameType () == GameType.Bullseye)
//				CheckHit (touch.position);
//			else if (gameManager.GetGameType () == GameType.Line) {
//				if (touch.phase == TouchPhase.Ended) {
//					CheckMiss (touch.position);
//				} 
//				else {
//					dragging = touch.phase != TouchPhase.Began;
//					CheckCrossing (touch.position);
//				}
//			}
//		}
	}

	private void CheckHit(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckHit");
#endif

        worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		gameManager.SetHitPosition(worldPosition);



		hit = Physics2D.OverlapPoint(worldPosition);

		ConfirmHit(hit);
	}

	private void CheckTunnelCrossing(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckTunnelCrossing");
#endif

        prevWorldPosition = worldPosition;
		worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		crossingY = (prevWorldPosition.y + worldPosition.y) / 2;
        float crossingX = (prevWorldPosition.x + worldPosition.x) / 2;

		for (int i = 0; i < gameManager.GetTotalTargets(); i++) {

			target = gameManager.GetTargetAttributes (i);
		
		
		var targetXPos = gameManager.GetTunnelTarget(i).gameObject.transform.position.x;
        Bounds upper_bound = gameManager.GetTunnelBars(0).GetComponentsInChildren<Renderer>()[0].bounds;
        Bounds lower_bound = gameManager.GetTunnelBars(1).GetComponentsInChildren<Renderer>()[0].bounds;
        Vector3 upper_origin = Camera.main.WorldToScreenPoint(new Vector3(upper_bound.max.x, upper_bound.min.y, 0f));
        Vector3 lower_extent = Camera.main.WorldToScreenPoint(new Vector3(lower_bound.min.x, lower_bound.max.y, 0f));
		// Debug.Log("upper_origin_y: " + upper_origin.y);
		// Debug.Log("lower_extent_y: " + lower_extent.y);
		// Debug.Log("screenPosition.y: " + _screenPosition.y);
		// Debug.Log("worldPosition.y: " + worldPosition.y);
		bool hasCrossed = false;
		bool hasError = false;
		int errorHit = -1;
		if (_screenPosition.y > lower_extent.y &&
			_screenPosition.y < upper_origin.y &&
			targetXPos > prevWorldPosition.x && 
			targetXPos < worldPosition.x) {
			hasCrossed = true;
			errorRecorded = false;
		} else if (_screenPosition.y > lower_extent.y &&
			_screenPosition.y < upper_origin.y &&
			targetXPos < prevWorldPosition.x && 
			targetXPos > worldPosition.x) {
			hasCrossed = true;
			errorRecorded = false;
		} else if (_screenPosition.y < lower_extent.y) {
			hasError = true;
			errorHit = 1;
		} else if (_screenPosition.y > upper_origin.y) {
			hasError = true;
			errorHit = 0;
		}
		
		if (hasCrossed)
		{
			gameManager.SetHitPosition(new Vector2(target.x, crossingY));
			gameManager.GetTunnelTarget(i).Cross();
			cross = true;
		} else if (hasError && !errorRecorded) {
			errorRecorded = true;
			gameManager.ErrorHit(errorHit);
			gameManager.GetTunnelBars(0).PlayFeedback();
			gameManager.GetTunnelBars(1).PlayFeedback();
		}
		}


        // for (int i = 0; i < 2; i++)
        // {
        //     if (gameManager.GetTunnelBars(i).GetComponent<TunnelBar>().IsNewHit())
        //     {
        //         gameManager.GetTunnelBars(i).GetComponent<TunnelBar>().DeactivateHit();

        //         gameManager.SetHitPosition(new Vector2(crossingX, crossingY));
        //         gameManager.Miss();
        //     }
        // }

		// for (int i = 0; i < gameManager.GetTotalTargets(); i++) {

		// 	target = gameManager.GetTargetAttributes (i);

        //     if (gameManager.GetTunnelTarget(i).GetComponent<TunnelTarget>().IsNewHit())
        //     {
        //         gameManager.GetTunnelTarget(i).GetComponent<TunnelTarget>().DeactivateHit();

        //         gameManager.SetHitPosition(new Vector2(target.x, crossingY));
        //         gameManager.GetTunnelTarget(i).Cross();

        //         cross = true;
        //     }
		// }
	}

	private void CheckCrossing(Vector2 _screenPosition) {

		prevWorldPosition = worldPosition;
		worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		crossingY = (prevWorldPosition.y + worldPosition.y) / 2;

		for (int i = 0; i < gameManager.GetTotalTargets(); i++) {

			target = gameManager.GetTargetAttributes (i);
		
		
		var targetXPos = gameManager.GetGoalTarget(i).gameObject.transform.position.x;
		var targetBounds = gameManager.GetGoalTarget(i).gameObject.GetComponentsInChildren<Renderer>()[0].bounds;
		
		bool hasCrossed = false;
		if (worldPosition.y > targetBounds.min.y &&
			worldPosition.y < targetBounds.max.y &&
			targetXPos > prevWorldPosition.x && 
			targetXPos < worldPosition.x) {
			hasCrossed = true;
		}	 else if (worldPosition.y > targetBounds.min.y &&
				worldPosition.y < targetBounds.max.y &&
				targetXPos < prevWorldPosition.x &&
				targetXPos > worldPosition.x) {
			hasCrossed = true;
		}
		
		if (hasCrossed)
		{
			gameManager.SetHitPosition(new Vector2(target.x, crossingY));
			gameManager.GetGoalTarget(i).Cross();
			cross = true;
		}
		}
	}

	private void ConfirmHit(Collider2D _collider) {
#if (PRINT_FUNC_CALL)
        Debug.Log("ConfirmHit");
#endif
        if (_collider == null) {

			gameManager.Miss();
		}
		else if(_collider.tag == "Target") {

			if(gameManager.GetInGame()) {

				_collider.GetComponent<FittsTarget>().Hit();
			}
		}
	}

	private void CheckMiss(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckMiss");
#endif
        if (cross)
			cross = false;
		else {
			worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);
			gameManager.SetHitPosition (worldPosition);
			gameManager.Miss ();
		}
	}
}

