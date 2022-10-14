using System.Collections;
using UnityEngine;

public class Intersection : MonoBehaviour {
	private static Intersection Instance;
	public static bool CanTurn => Instance != null;

	private void OnTriggerEnter(Collider other) {
		Instance = this;
	}

	private void OnTriggerExit(Collider other) {
		Instance = null;
	}

	public static void Turn(Drone drone, bool left) {
		if (!CanTurn)
			return;
		Quaternion rotation = left ? Quaternion.AngleAxis(-3, Vector3.up) : Quaternion.AngleAxis(3, Vector3.up);
		Instance.StartCoroutine(TurnAnimation(drone, rotation));
		Instance = null;
	}

	public static IEnumerator TurnAnimation(Drone drone, Quaternion rotation) {
		drone.CanMove = false;
		drone.TargetLocked = true;
		LevelManager.Instance.Move = false;
		Vector3 position = Instance.transform.parent.position + 3 * Vector3.up;
		for (int i = 0; i < 30; i++) {
			LevelManager.Instance.Rotate(rotation, position);
			yield return null;
		}
		drone.CanMove = true;
		drone.TargetLocked = false;
		LevelManager.Instance.Move = true;
	}
}
