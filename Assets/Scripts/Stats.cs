using System;
using UnityEngine;

public class Stats {
	
	private static float average(float[] array) {
		int length = array.Length;
		float sum = 0;
		for (int i = 0; i < array.Length; i++) {
			sum += array [i] / length;
		}
		return sum;
	}

	public static float variance3(Vector3[] array) {
		float[] xVals, yVals, zVals;
		xVals = new float[array.Length];
		yVals = new float[array.Length];
		zVals = new float[array.Length];
		for (int i = 0; i < array.Length; i++) {
			xVals [i] = normalizeAngle(array [i].x);
			yVals [i] = normalizeAngle(array [i].y);
			zVals [i] = normalizeAngle(array [i].z);
		}
		return 0;

	}

	private float variance(float[] array) {
		float avg = average (array);
		float sum = 0;
		for (int i = 0; i < array.Length; i++) {
			sum += Math.Pow (array.Length - avg, 2);
		}
	}

	private static float normalizeAngle (float angle) {
		if (angle > 180.0f) {
			return angle - 360.0f;
		}
		if (angle < -180.0f) {
			return angle + 360.0f;
		}
		return angle;
	}

}