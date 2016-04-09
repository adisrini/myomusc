using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;


public class WallController : MonoBehaviour {

	public static float currEastPos;
	public static float currWestPos;

	void Start() {
		if(this.gameObject.CompareTag("East Wall")) {
			currEastPos = this.transform.position.x;
			Debug.Log ("EAST: " + currEastPos);
		}
			
		if (this.gameObject.CompareTag ("West Wall")) {
			currWestPos = this.transform.position.x;
			Debug.Log ("WEST: " + currWestPos);
		}
			
	}

	void Update() {
		if (GroundController.shrink) {
			if(this.gameObject.CompareTag("East Wall")){
				Vector3 pos = this.transform.position;
				pos.x = pos.x - 0.05f;
				this.transform.position = pos;
			}
			if(this.gameObject.CompareTag("West Wall")){
				Vector3 pos = this.transform.position;
				pos.x = pos.x + 0.05f;
				this.transform.position = pos;
			}
		}

		if (GroundController.grow) {
			if(this.gameObject.CompareTag("East Wall")){
				Vector3 pos = this.transform.position;
				pos.x = pos.x + 0.05f;
				this.transform.position = pos;
			}
			if(this.gameObject.CompareTag("West Wall")){
				Vector3 pos = this.transform.position;
				pos.x = pos.x - 0.05f;
				this.transform.position = pos;
			}
		}
	
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.CompareTag("East Wall") || other.gameObject.CompareTag("West Wall")  ) {
			other.gameObject.SetActive(false); 
			GameOver ();
		}
	}

	void GameOver(){

	}

}

