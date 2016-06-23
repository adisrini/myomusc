using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class PlayerController : MonoBehaviour {

	// Myo game object to connect with.
	// This object must have a ThalmicMyo script attached.
	public GameObject myo = null;
	public static bool jitter;

	public float threshold;

	public float speed;
	public Text countText;

	public Vector3[] movementCache;
	private float[] variances;

	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;

	// A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
	// Once set, the direction the Myo armband is facing becomes "forward" within the program.
	// Set by making the fingers spread pose or pressing "r".
	private Quaternion _antiYaw = Quaternion.identity;

	// A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
	// Set by making the fingers spread pose or pressing "r".
	private float _referenceRoll = 0.0f;

	private Rigidbody rb;
	public static int count;
	private int cacheSize;

	// Use this for initialization
	void Start () {
		threshold = 10;
		cacheSize = 20;
		movementCache = new Vector3[cacheSize];
		variances = new float[cacheSize];
		rb = GetComponent<Rigidbody> ();
		count = 0;
		setCountText ();
	}

	void FixedUpdate() {
		if (myo == null) {
			myo = new GameObject ();
		}
		// Access the ThalmicMyo component attached to the Myo game object.
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		// Update references when the pose becomes fingers spread or the q key is pressed.
		bool updateReference = false;

		// Check if the pose has changed since last update.
		// The ThalmicMyo component of a Myo game object has a pose property that is set to the
		// currently detected pose (e.g. Pose.Fist for the user making a fist). If no pose is currently
		// detected, pose will be set to Pose.Rest. If pose detection is unavailable, e.g. because Myo
		// is not on a user's arm, pose will be set to Pose.Unknown.
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;

			// Vibrate the Myo armband when a fist is made.
			if (thalmicMyo.pose == Pose.Fist) {
				thalmicMyo.Vibrate (VibrationType.Medium);
				//rb.AddForce (new Vector3(0, 0, 10) * speed);
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.FingersSpread || Input.GetKeyDown("r")) {
				updateReference = true;

				ExtendUnlockAndNotifyUserAction(thalmicMyo);
			}

		}

		// Update references. This anchors the joint on-screen such that it faces forward away
		// from the viewer when the Myo armband is oriented the way it is when these references are taken.
		if (updateReference) {
			// _antiYaw represents a rotation of the Myo armband about the Y axis (up) which aligns the forward
			// vector of the rotation with Z = 1 when the wearer's arm is pointing in the reference direction.
			_antiYaw = Quaternion.FromToRotation (
				new Vector3 (myo.transform.forward.x, 0, myo.transform.forward.z),
				new Vector3 (0, 0, 1)
			);

			// _referenceRoll represents how many degrees the Myo armband is rotated clockwise
			// about its forward axis (when looking down the wearer's arm towards their hand) from the reference zero
			// roll direction. This direction is calculated and explained below. When this reference is
			// taken, the joint will be rotated about its forward axis such that it faces upwards when
			// the roll value matches the reference.
			Vector3 referenceZeroRoll = computeZeroRollVector (myo.transform.forward);
			_referenceRoll = rollFromZero (referenceZeroRoll, myo.transform.forward, myo.transform.up);
		}
			
		// Current zero roll vector and roll value.
		Vector3 zeroRoll = computeZeroRollVector (myo.transform.forward);
		float roll = rollFromZero (zeroRoll, myo.transform.forward, myo.transform.up);

		// The relative roll is simply how much the current roll has changed relative to the reference roll.
		// adjustAngle simply keeps the resultant value within -180 to 180 degrees.
		float relativeRoll = normalizeAngle (roll - _referenceRoll);

		// antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
		Quaternion antiRoll = Quaternion.AngleAxis (relativeRoll, myo.transform.forward);

		// Here the anti-roll and yaw rotations are applied to the myo Armband's forward direction to yield
		// the orientation of the joint.
		transform.rotation = _antiYaw * antiRoll * Quaternion.LookRotation (myo.transform.forward);

		// The above calculations were done assuming the Myo armbands's +x direction, in its own coordinate system,
		// was facing toward the wearer's elbow. If the Myo armband is worn with its +x direction facing the other way,
		// the rotation needs to be updated to compensate.
		if (thalmicMyo.xDirection == Thalmic.Myo.XDirection.TowardWrist) {
			// Mirror the rotation around the XZ plane in Unity's coordinate system (XY plane in Myo's coordinate
			// system). This makes the rotation reflect the arm's orientation, rather than that of the Myo armband.
			transform.rotation = new Quaternion(transform.localRotation.x,
											   -transform.localRotation.y,
												transform.localRotation.z,
											   -transform.localRotation.w);
		}

		Vector3 rotation = FromQ2 (transform.rotation);
		Vector3 movement = getMovementFromRotation (rotation);

		rb.AddForce (movement * speed);
		addToCache (rotation);

		float v = variance (movementCache);

		if (v < threshold) {
			addToVariances (v);
		}

		interpretVariances ();

	}

	private void interpretVariances() {
		Debug.Log (Stats.average (variances));
		jitter = Stats.average (variances) > 1;
	}

	private float variance(Vector3[] array) {
		if (array [0].x == 0f && array[0].y == 0f && array[0].z == 0f) {
			return 0;
		}
		return Stats.variance3 (array);
	}

	private Vector3 getMovementFromRotation(Vector3 rotation) {
		float playerX, playerZ;

		if (rotation.x < 180) {
			playerZ = -rotation.x;
		} else {
			playerZ = (360 - rotation.x);
		}
		if (rotation.y < 180) {
			playerX = rotation.y;
		} else {
			playerX = -(360 - rotation.y);
		}

		return new Vector3 (playerX, 0, playerZ);
	}

	private Vector3 FromQ2 (Quaternion q1) {
		float sqw = q1.w * q1.w;
		float sqx = q1.x * q1.x;
		float sqy = q1.y * q1.y;
		float sqz = q1.z * q1.z;
		float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
		float test = q1.x * q1.w - q1.y * q1.z;
		Vector3 v;

		if (test>0.4995f*unit) { // singularity at north pole
			v.y = 2f * Mathf.Atan2 (q1.y, q1.x);
			v.x = Mathf.PI / 2;
			v.z = 0;
			return NormalizeAngles (v * Mathf.Rad2Deg);
		}
		if (test<-0.4995f*unit) { // singularity at south pole
			v.y = -2f * Mathf.Atan2 (q1.y, q1.x);
			v.x = -Mathf.PI / 2;
			v.z = 0;
			return NormalizeAngles (v * Mathf.Rad2Deg);
		}
		Quaternion q = new Quaternion (q1.w, q1.z, q1.x, q1.y);
		v.y = (float)Math.Atan2 (2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));     // Yaw
		v.x = (float)Math.Asin (2f * (q.x * q.z - q.w * q.y));                             // Pitch
		v.z = (float)Math.Atan2 (2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));      // Roll
		return NormalizeAngles (v * Mathf.Rad2Deg);
	}

	private Vector3 NormalizeAngles (Vector3 angles) {
		angles.x = NormalizeAngle (angles.x);
		angles.y = NormalizeAngle (angles.y);
		angles.z = NormalizeAngle (angles.z);
		return angles;
	}

	private float NormalizeAngle (float angle) {
		while (angle>360)
			angle -= 360;
		while (angle<0)
			angle += 360;
		return angle;
	}

	// Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
	// As the armband is rotated about the forward axis this value will change, regardless of which way the
	// forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
	private float rollFromZero (Vector3 zeroRoll, Vector3 forward, Vector3 up) {
		// The cosine of the angle between the up vector and the zero roll vector. Since both are
		// orthogonal to the forward vector, this tells us how far the Myo has been turned around the
		// forward axis relative to the zero roll vector, but we need to determine separately whether the
		// Myo has been rolled clockwise or counterclockwise.
		float cosine = Vector3.Dot (up, zeroRoll);

		// To determine the sign of the roll, we take the cross product of the up vector and the zero
		// roll vector. This cross product will either be the same or opposite direction as the forward
		// vector depending on whether up is clockwise or counter-clockwise from zero roll.
		// Thus the sign of the dot product of forward and it yields the sign of our roll value.
		Vector3 cp = Vector3.Cross (up, zeroRoll);
		float directionCosine = Vector3.Dot (forward, cp);
		float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

		// Return the angle of roll (in degrees) from the cosine and the sign.
		return sign * Mathf.Rad2Deg * Mathf.Acos (cosine);
	}

	// Compute a vector that points perpendicular to the forward direction,
	// minimizing angular distance from world up (positive Y axis).
	// This represents the direction of no rotation about its forward axis.
	private Vector3 computeZeroRollVector (Vector3 forward) {
		Vector3 antigravity = Vector3.up;
		Vector3 m = Vector3.Cross (myo.transform.forward, antigravity);
		Vector3 roll = Vector3.Cross (m, myo.transform.forward);

		return roll.normalized;
	}

	// Adjust the provided angle to be within a -180 to 180.
	private float normalizeAngle (float angle) {
		if (angle > 180.0f) {
			return angle - 360.0f;
		}
		if (angle < -180.0f) {
			return angle + 360.0f;
		}
		return angle;
	}


	// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
	// recognized.
	private void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo) {
		ThalmicHub hub = ThalmicHub.instance;

		if (hub.lockingPolicy == LockingPolicy.Standard) {
			myo.Unlock (UnlockType.Timed);
		}

		myo.NotifyUserAction ();
	}

	private void addToCache(Vector3 vec) {
		for(int i = 1; i < this.movementCache.Length; i++) {
			this.movementCache [i - 1] = this.movementCache [i];
		}
		this.movementCache[this.movementCache.Length - 1] = vec;
	}

	private void addToVariances(float v) {
		for(int i = 1; i < this.variances.Length; i++) {
			this.variances [i - 1] = this.variances [i];
		}
		this.variances[this.variances.Length - 1] = v;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag ("Pick Up")) {
			other.gameObject.SetActive (false); 
			count = count + 1;
			setCountText ();
		}	
		if (other.gameObject.CompareTag ("East Wall") || other.gameObject.CompareTag ("West Wall")) {
<<<<<<< HEAD
			//end game
			Debug.Log("collide");
=======
			count = 0;
			setCountText ();
			Vector3 position = transform.position;
			position.x = 0f;
			position.y = 0.5f;
			position.z = -140f;
			transform.position = position;
		}
		if (other.gameObject.CompareTag ("North Wall")) {
			Application.LoadLevel (2);
>>>>>>> 91690045f77a717c283fd6873ebdfcade1179d44
		}

	}

	void onCollisionEnter(Collider other) {
		
	}

	void setCountText() {
		countText.text = "Count: " + count.ToString ();
	}

}