using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
    public Transform target;
    public float targetLerp = 0.8f;

	void Update () {
        Vector3 targetPos = Vector3.Lerp(transform.position, target.position, targetLerp);
        targetPos.Set(targetPos.x, targetPos.y, -5);
        transform.position = targetPos;
	}
}
