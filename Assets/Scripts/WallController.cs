﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;


public class WallController : MonoBehaviour {

	public static float currEastPos;
	public static float currWestPos;

	public GameObject ground;

	private Vector3 position;

	void Start() {
		if(this.gameObject.CompareTag("East Wall")) {
			position.x = ground.transform.localScale.x * 5;
			transform.position = position;
		}
			
		if (this.gameObject.CompareTag ("West Wall")) {
			position.x = ground.transform.localScale.x * -5;
			transform.position = position;
		}


	}

	void Update() {
		if(this.gameObject.CompareTag("East Wall")) {
			position.x = ground.transform.localScale.x * 5;
			transform.position = position;
		}

		if (this.gameObject.CompareTag ("West Wall")) {
			position.x = ground.transform.localScale.x * -5;
			transform.position = position;
		}

		if (this.gameObject.CompareTag ("North Wall") || this.gameObject.CompareTag ("South Wall")) {
			Vector3 scale = transform.localScale;
			scale.x = 20.5f * (ground.transform.localScale.x / 2.0f);
			transform.localScale = scale;
		}

	}

}

