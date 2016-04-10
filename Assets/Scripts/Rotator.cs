using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	public GameObject ground;
	private Vector3 position;

	private float rand;

	void Start(){
		position = transform.position;
		rand = Random.Range (-ground.transform.localScale.x * 3.0f, ground.transform.localScale.x * 3.0f);
		position.x = rand;
		transform.position = position;
	}
	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
		position = transform.position;
		position.x = (rand*ground.transform.localScale.x)/2.0f;
		transform.position = position;
	}
}
