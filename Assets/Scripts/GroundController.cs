using UnityEngine;
using System.Collections;

public class GroundController : MonoBehaviour {

	public int frameCount;

	public static float currentScale;
	public static bool shrink;
	public static bool grow;

	// Use this for initialization
	void Start () {
		frameCount = 100;
		currentScale = transform.localScale.x;
		shrink = false;
		grow = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (PlayerController.jitter) {
			shrink = true;
			grow = false;
			currentScale = transform.localScale.x;
		}
		if (!PlayerController.jitter) {
			shrink = false;
			grow = true;
			currentScale = transform.localScale.x;
		}
		if(shrink) {
			float intendedScale = 0.2F;
			if (transform.localScale.x <= intendedScale) {
				shrink = false;
			} else {
				Vector3 scale = transform.localScale;
				scale.x = scale.x + (intendedScale - currentScale)/frameCount; // your new value
				transform.localScale = scale;
			}
		}
		if(grow) {
			float intendedScale = 2F;
			if (transform.localScale.x >= intendedScale) {
				grow = false;
			} else {
				Vector3 scale = transform.localScale;
				scale.x = scale.x + (intendedScale - currentScale)/frameCount; // your new value
				transform.localScale = scale;
			}

		}
	}
}
