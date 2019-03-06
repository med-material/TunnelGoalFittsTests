using UnityEngine;
using System.Collections;

public class StartButton : MonoBehaviour {

	private SpriteRenderer sprite;
	private Color visible;
	private Color invisible;
	private Collider2D coll;
    
    void Awake() {
		sprite = this.GetComponent<SpriteRenderer> ();
        coll = this.GetComponent<Collider2D> ();
		visible = new Color (1, 1, 1, 1);
		invisible = new Color (1, 1, 1, 0);
    }

    public void Hit() {
		
		GameManager.StartGame ();
	}

	public void Disappear() {

		sprite.color = invisible;
		coll.enabled = false;
	}

	public void Appear() {
		
		sprite.color = visible;
		coll.enabled = true;
	}
}