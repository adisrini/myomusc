using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreDisplay : MonoBehaviour {

	public Text winText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		winText.text = winText.text + PlayerController.count.ToString();
	}
}
