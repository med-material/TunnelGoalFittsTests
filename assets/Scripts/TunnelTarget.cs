using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TunnelTarget : Target {

    private float animationTime = 0.2f;

    private bool feedback = false;
    private float hitTime;

    private SpriteRenderer sprite;
    private bool newHit = false;

	[SerializeField]
	private Color activeColor;

	[SerializeField]
	private Color inactiveColor;

	[SerializeField]
	private Color feedbackColor;

    private GameManager gameManager;

    private Dropdown inputDropdown;

    void Awake()
    {
        gameManager = GameObject.Find("Managers").GetComponent<GameManager>();

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
    /*private void OnMouseEnter()
    {
        newHit = true;
    }

    private void OnMouseExit()
    {
        newHit = false;
    }*/

    public void DeactivateHit()
    {
        newHit = false;
    }

    public bool IsNewHit()
    {
        return newHit;
    }


    public new void SetActiveTarget()
    {
        sprite.color = activeColor;
    }


    public void Cross()
    {
        if (gameManager.GetCurrentTarget() == targetID)
        {
            gameManager.SuccesfulHit();
            PlayFeedback();
        }
        else
        {
            // Do Nothing
            // gameManager.ErrorHit(targetID);
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