using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FittsTarget : Target
{

    private float animationTime = 0.2f;

    private bool feedback = false;
    private float hitTime;

    private SpriteRenderer sprite;

	[SerializeField]
	private Color activeColor;

	[SerializeField]
	private Color inactiveColor;

	[SerializeField]
	private Color feedbackColor;


    void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
        sprite.color = inactiveColor;
    }

    void Update()
    {
        if (feedback)
        {
            if (Time.time - hitTime > animationTime)
            {
                sprite.color = inactiveColor;

                feedback = false;
            }
        }
    }
    public new void SetActiveTarget()
    {
        sprite.color = activeColor;
    }

    public void Hit()
    {
        if (GameManager.GetCurrentTarget() == targetID)
        {
            GameManager.SuccesfulHit();
            PlayFeedback();
        }
        else
        {
            GameManager.ErrorHit(targetID);
        }
    }

    public void SetSize(int width, int height)
    {
        this.transform.localScale = new Vector3(width, height, 1);
    }

    private void PlayFeedback()
    {
        feedback = true;
        hitTime = Time.time;

        sprite.color = feedbackColor;
    }

}
