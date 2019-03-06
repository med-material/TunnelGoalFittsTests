using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

	protected int targetID;

	public void SetTargetID(int _id) {

		targetID = _id;
	}

	public int GetTargetID() {

		return targetID;
	}

    public void SetActiveTarget() { }
}
