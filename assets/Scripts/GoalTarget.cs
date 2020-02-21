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

	private Vector3 feedbackVector;
    private bool newHit = false;

	[SerializeField]
	private Color activeColor;

	[SerializeField]
	private Color inactiveColor;

	[SerializeField]
	private Color feedbackColor;



    void Awake() {

		midLine = transform.Find ("MidLine");

		midSprite = midLine.GetComponent<SpriteRenderer> ();
		midSprite.color = inactiveColor;
	}

    void Update() {
        if (feedback) {

			if(Time.time - hitTime > animationTime) {

				midSprite.color = inactiveColor;

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

    }

    public void SetSize(float width, float height) {

		midLine.localScale = new Vector3 (width, height, 1);
	}

	private void PlayFeedback() {

		feedback = true;
		hitTime = Time.time;

		midSprite.color = feedbackColor;
	}
}