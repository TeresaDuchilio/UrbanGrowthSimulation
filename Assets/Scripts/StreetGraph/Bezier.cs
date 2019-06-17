using UnityEngine;
using System.Collections;

public static class Bezier{

	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float t2 = t * t;
		float omt = 1f - t;
		float omt2 = omt * omt; 
		return p0 * (omt2 * omt) +
			   p1 * (3f * omt2 * t) +
			   p2 * (3f * omt * t2) +
			   p3 * (t2 * t);
	}
	
	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return	3f * oneMinusT * oneMinusT * (p1 - p0) + 6f * oneMinusT * t * (p2 - p1) + 3f * t * t * (p3 - p2);
	}

	public static Vector3 GetTanget (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t){
		t = Mathf.Clamp01 (t);
		float t2 = t * t;
		float omt = 1f - t;
		float omt2 = omt * omt; 

		Vector3 tangent = p0 * (-omt2) +
						  p1 * (3 * omt2 - 2 * omt) +
						  p2 * (-3 * t2 + 2 * t) +
						  p3 * t2;

		return tangent.normalized;
	}

	public static Vector3 GetNormal (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t){
		Vector3 tangent = GetTanget(p0,p1, p2, p3, t);
		Vector3 binormal = Vector3.Cross(Vector3.up, tangent).normalized;
		return Vector3.Cross(tangent, binormal);
	}
	
}
