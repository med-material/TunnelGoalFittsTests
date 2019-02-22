using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ERMManager : MonoBehaviour {

	public bool isStarted;

	public float timer;

	public AnimationCurve ERM1Pattern;
	public int ERM1Debug;
	public AnimationCurve ERM2Pattern;
	public int ERM2Debug;
	public AnimationCurve ERM3Pattern;
	public int ERM3Debug;

	public GameObject Arduino;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Arduino.GetComponent<Arduino>().testStart == 1){
			isStarted = true;
		}

		if(isStarted){
			timer = timer + Time.deltaTime;	
		}

		ERM1Debug = Mathf.RoundToInt(ERM1Pattern.Evaluate(timer));
		ERM2Debug = Mathf.RoundToInt(ERM2Pattern.Evaluate(timer));
		ERM3Debug = Mathf.RoundToInt(ERM3Pattern.Evaluate(timer));

		// ERM1
		if(ERM1Debug > 255){
			Arduino.GetComponent<Arduino>().ERM1Power = 255;
		}
		else{
			if(ERM1Debug < 0){
				Arduino.GetComponent<Arduino>().ERM1Power = 0;
			}
			else{
				Arduino.GetComponent<Arduino>().ERM1Power = ERM1Debug;
			}
		}

		// ERM22
		if(ERM2Debug > 255){
			Arduino.GetComponent<Arduino>().ERM2Power = 255;
		}
		else{
			if(ERM2Debug < 0){
				Arduino.GetComponent<Arduino>().ERM2Power = 0;
			}
			else{
				Arduino.GetComponent<Arduino>().ERM2Power = ERM2Debug;
			}
		}

		// ERM3
		if(ERM3Debug > 255){
			Arduino.GetComponent<Arduino>().ERM3Power = 255;
		}
		else{
			if(ERM3Debug < 0){
				Arduino.GetComponent<Arduino>().ERM3Power = 0;
			}
			else{
				Arduino.GetComponent<Arduino>().ERM3Power = ERM3Debug;
			}
		}
	}
}
