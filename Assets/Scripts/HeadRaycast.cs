using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRaycast : MonoBehaviour {

	public bool canCollide;
	public float waitTimer;
	public Transform latestCollision;
	public Transform previousCollision;
	public Transform tunnelMid;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(!canCollide){
			waitTimer = waitTimer + Time.deltaTime;	
		}

		if(waitTimer > 0.5){
			canCollide = true;
		}

		RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

            if(hit.transform.tag == "StartButtonCollider"){
            	previousCollision.position = latestCollision.position;
            	latestCollision.position = hit.point;
            	tunnelMid.position = new Vector3(hit.point.x,0,hit.point.z);
            	hit.transform.parent.GetComponent<StartButton>().Hit();
            }

            if(hit.transform.tag == "TunnelTargetCollider"){
            	if(canCollide){
            		previousCollision.position = latestCollision.position;
	            	latestCollision.position = hit.point;
	            	tunnelMid.position = new Vector3(hit.point.x,0,hit.point.z);
	            	hit.transform.parent.GetComponent<TunnelTarget>().Cross();

	            	canCollide = false;
	            	waitTimer = 0;
            	}
            }

            if(hit.transform.tag == "TunnelBarCollider"){
            	if(canCollide){
            		previousCollision.position = latestCollision.position;
	            	latestCollision.position = hit.point;
	            	tunnelMid.position = new Vector3(hit.point.x,0,hit.point.z);
	            	hit.transform.parent.GetComponent<TunnelBar>().Cross();

	            	canCollide = false;
	            	waitTimer = 0;
            	}
            }
        }


        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
        }
	}
}
