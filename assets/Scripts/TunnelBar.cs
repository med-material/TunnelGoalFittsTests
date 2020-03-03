using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TunnelBar : Target {

    private float animationTime = 0.2f;

    private bool feedback = false;
    private float hitTime;

    private SpriteRenderer sprite;
    private bool newHit = false;

    private GameManager gameManager;

    private Dropdown inputDropdown;
    

    void Awake()
    {
        gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
        inputDropdown = GameObject.Find("InputTypeDropdown").GetComponent<Dropdown>();
        sprite = this.GetComponent<SpriteRenderer>();
        sprite.color = Color.gray;
    }

    void Update()
    {
        if (feedback)
        {
            if (Time.time - hitTime > animationTime)
            {
                sprite.color = Color.gray;

                feedback = false;
            }
        }
    }

    private void OnMouseEnter()
    {
        if (inputDropdown.value == (int) InputType.pressuresensor) {
            return;
        }
        newHit = true;
    }

    private void OnMouseExit()
    {
        if (inputDropdown.value == (int) InputType.pressuresensor) {
            return;
        }
        newHit = false;
    }

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
        sprite.color = Color.red;
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
            gameManager.ErrorHit(targetID);
        }
    }

    public void SetPosition(int newpos)
    {
        this.transform.position = new Vector3(this.transform.position.x, newpos, this.transform.position.z);
    }

    public void PlayFeedback()
    {
        feedback = true;
        hitTime = Time.time;

        sprite.color = Color.red;
    }

}