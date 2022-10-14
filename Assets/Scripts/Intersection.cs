using System.Collections;
using Common;
using Events;
using UnityEngine;
using Random = UnityEngine.Random;

public class Intersection : MonoBehaviour {
	[SerializeField] private Collider turn;
	private int used;
	private int direction;

	private static Intersection Instance;

	private void Awake() {
		this.direction = Random.Range(-1, 2);
		EventManager.Instance.AddListener<DeliverStartEvent>(this.OnDeliverStart);
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<DeliverStartEvent>(this.OnDeliverStart);
	}

	private void OnTriggerEnter(Collider other) {
		Instance = this;
		if (LevelManager.Instance.FollowGPS)
			EventManager.Instance.Raise(new GPSUpdatedEvent(this.direction));
	}

	private void OnTriggerExit(Collider other) {
		Instance = null;
		EventManager.Instance.Raise(new GPSUpdatedEvent());
		if (LevelManager.Instance.FollowGPS && this.used != this.direction) {
			EventManager.Instance.Raise(new DeliverEndEvent(false));
			SfxManager.Instance.PlaySfx2D("DeliverFailed");
		}
	}

	private void OnDeliverStart(DeliverStartEvent e) {
		Drone drone = FindObjectOfType<Drone>();
		Collider col = this.GetComponent<Collider>();
		if (col.bounds.Contains(drone.transform.position))
			EventManager.Instance.Raise(new GPSUpdatedEvent(this.direction));
	}

	public static bool CanTurn(Drone drone) {
		return Instance != null && Instance.used == 0 && Instance.turn.bounds.Contains(drone.transform.position);
	}

	public static void Turn(Drone drone, bool left) {
		if (!CanTurn(drone))
			return;
		Quaternion rotation = left ? Quaternion.AngleAxis(-3, Vector3.up) : Quaternion.AngleAxis(3, Vector3.up);
		Instance.StartCoroutine(TurnAnimation(drone, rotation));
		Instance.used = left ? -1 : 1;
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
