//#define PRINT_FUNC_CALL

using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public static InputManager instance = null; 

	private static Collider2D hit;
	private static Vector2 worldPosition;
	private static Vector2 prevWorldPosition;

	private static bool dragging;
	private static float crossingY;

	private static Vector4 target;

	private static bool cross = false;

	void Awake() {

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(this);  
	}
	
	public static void CheckInput() {

		if (SystemInfo.deviceType == DeviceType.Handheld) {

			CheckTouchInput();
		}
		else if(SystemInfo.deviceType == DeviceType.Desktop) {

			CheckMouseInput();
		}
	}

	private static void CheckMouseInput() {

	
        if (Input.GetMouseButtonDown(0) && GameManager.GetGameType() == GameType.Fitts)
            CheckHit(Input.mousePosition);
        else if (GameManager.GetGameType() == GameType.Goal)
        {
                CheckCrossing(Input.mousePosition);
        }
		else if (GameManager.GetGameType() == GameType.Tunnel){
			CheckTunnelCrossing(Input.mousePosition);
		}
    }

	private static void CheckTouchInput() {

//		foreach (Touch touch in Input.touches) {
//			
//			if (touch.phase == TouchPhase.Began && GameManager.GetGameType () == GameType.Bullseye)
//				CheckHit (touch.position);
//			else if (GameManager.GetGameType () == GameType.Line) {
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

	private static void CheckHit(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckHit");
#endif

        worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		GameManager.SetHitPosition(worldPosition);



		hit = Physics2D.OverlapPoint(worldPosition);

		ConfirmHit(hit);
	}

	private static void CheckTunnelCrossing(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckTunnelCrossing");
#endif

        prevWorldPosition = worldPosition;
		worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		crossingY = (prevWorldPosition.y + worldPosition.y) / 2;
        float crossingX = (prevWorldPosition.x + worldPosition.x) / 2;

        for (int i = 0; i < 2; i++)
        {
            if (GameManager.GetTunnelBars(i).GetComponent<TunnelBar>().IsNewHit())
            {
                GameManager.GetTunnelBars(i).GetComponent<TunnelBar>().DeactivateHit();

                GameManager.SetHitPosition(new Vector2(crossingX, crossingY));
                GameManager.Miss();
            }
        }

		for (int i = 0; i < GameManager.GetTotalTargets(); i++) {

			target = GameManager.GetTargetAttributes (i);

            if (GameManager.GetTunnelTarget(i).GetComponent<TunnelTarget>().IsNewHit())
            {
                GameManager.GetTunnelTarget(i).GetComponent<TunnelTarget>().DeactivateHit();

                GameManager.SetHitPosition(new Vector2(target.x, crossingY));
                GameManager.GetTunnelTarget(i).Cross();

                cross = true;
            }
		}
	}

	private static void CheckCrossing(Vector2 _screenPosition) {

		prevWorldPosition = worldPosition;
		worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);

		crossingY = (prevWorldPosition.y + worldPosition.y) / 2;

		for (int i = 0; i < GameManager.GetTotalTargets(); i++) {

			target = GameManager.GetTargetAttributes (i);
		
		
		var targetXPos = GameManager.GetGoalTarget(i).gameObject.transform.position.x;
		bool hasCrossed = false;
		if (targetXPos > prevWorldPosition.x && targetXPos < worldPosition.x) {
			hasCrossed = true;
		} else if (targetXPos < prevWorldPosition.x && targetXPos > worldPosition.x) {
			hasCrossed = true;
		}
		
		if (hasCrossed)
		{
			GameManager.SetHitPosition(new Vector2(target.x, crossingY));
			GameManager.GetGoalTarget(i).Cross();
			cross = true;
		}
		}
	}

	private static void ConfirmHit(Collider2D _collider) {
#if (PRINT_FUNC_CALL)
        Debug.Log("ConfirmHit");
#endif
        if (_collider == null) {

			GameManager.Miss();
		}
		else if(_collider.tag == "Target") {

			if(GameManager.GetInGame()) {

				_collider.GetComponent<FittsTarget>().Hit();
			}
		}
	}

	public static void CheckStart() {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckStart");
#endif
        if (Input.GetMouseButtonDown (0)) {

			worldPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);

			hit = Physics2D.OverlapPoint(worldPosition);
			
			if(hit != null && hit.tag == "StartButton") {

				hit.GetComponent<StartButton>().Hit();
			}

			cross = true;
		}
	}

	private static void CheckMiss(Vector2 _screenPosition) {
#if (PRINT_FUNC_CALL)
        Debug.Log("CheckMiss");
#endif
        if (cross)
			cross = false;
		else {
			worldPosition = Camera.main.ScreenToWorldPoint (_screenPosition);
			GameManager.SetHitPosition (worldPosition);
			GameManager.Miss ();
		}
	}
}

