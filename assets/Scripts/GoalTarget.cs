using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GoalTarget : Target {

	private float animationTime = 0.2f;

	private bool feedback = false;
	private float hitTime;

	private SpriteRenderer midSprite;
	private SpriteRenderer topSprite;
	private SpriteRenderer bottomSprite;

	private Transform midLine;
	private Transform topLine;
	private Transform bottomLine;

	private Vector3 feedbackVector;
    private bool newHit = false;

	[SerializeField]
	private Color activeColor;
	[SerializeField]
	private Color feedbackColor;



    void Awake() {

		midLine = transform.Find ("MidLine");
		topLine = transform.Find ("TopLine");
		bottomLine = transform.Find ("BottomLine");

		midSprite = midLine.GetComponent<SpriteRenderer> ();
		topSprite = topLine.GetComponent<SpriteRenderer> ();
		bottomSprite = bottomLine.GetComponent<SpriteRenderer> ();
	}

    void Update() {
        if (feedback) {

			if(Time.time - hitTime > animationTime) {

				midSprite.color = Color.white;
				topSprite.color = Color.white;
				bottomSprite.color = Color.white;

				feedback = false;
			}
		}
	}

	public void Cross() {

		if (GameManager.GetCurrentTarget () == targetID) {

			GameManager.SuccesfulHit();
			PlayFeedback();
		}
		else {
			// Do Nothing 
			// GameManager.ErrorHit(targetID);
		}
	}

    public new void SetActiveTarget()
    {
        midSprite.color = activeColor;
        topSprite.color = activeColor;
        bottomSprite.color = activeColor;

    }

    public void SetSize(float width, float height) {

		midLine.localScale = new Vector3 (width*0.1f, height, 1);
        //topLine.localScale = new Vector3(7, 1, 1);
        topLine.localScale = new Vector3(0, 0, 0);
		topLine.localPosition = new Vector3 (0, height / 2, 1);
        //bottomLine.localScale = new Vector3(7, 1, 1);
        bottomLine.localScale = new Vector3(0, 0, 0);
        bottomLine.localPosition = new Vector3 (0, -height / 2, 1);
	}

	private void PlayFeedback() {

		feedback = true;
		hitTime = Time.time;

		midSprite.color = feedbackColor;
		topSprite.color = feedbackColor;
		bottomSprite.color = feedbackColor;
	}
}