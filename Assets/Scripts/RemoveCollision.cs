using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCollision : MonoBehaviour {

	private Color visible;
	private Color invisible;

	// Use this for initialization
	void Awake () {
		visible = new Color (1, 1, 1, 1);
		invisible = new Color (1, 1, 1, 0);	
	}
	
	// Update is called once per frame
	void Update () {
		if(this.transform.parent.GetComponent<SpriteRenderer>().color == invisible){
			this.GetComponent<BoxCollider>().enabled = false;
		}

		if(this.transform.parent.GetComponent<SpriteRenderer>().color == visible){
			this.GetComponent<BoxCollider>().enabled = true;
		}
	}
}
