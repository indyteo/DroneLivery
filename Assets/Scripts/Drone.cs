using Common;
using Events;
using UnityEngine;

public class Drone : MonoBehaviour {
	[SerializeField] private float moveSpeed = 1.5f;
	[SerializeField] private Transform target;

	public bool TargetLocked { get; set; } = false;
	public bool CanMove { get; set; } = true;

	public new Rigidbody rigidbody { get; private set; }
	//public new CapsuleCollider collider { get; private set; }

	//private static readonly float ROTATION_EPSILON = 0.001f;
	public static float Sensitivity { get; set; } = 1;

	private void Crash() {
		EventManager.Instance.Raise(new DroneCrashedEvent());
		Destroy(this.gameObject);
	}

	private void Awake() {
		this.rigidbody = this.GetComponent<Rigidbody>();
		//this.collider = this.GetComponent<CapsuleCollider>();
		EventManager.Instance.AddListener<DroneSpawnedEvent>(this.OnDroneSpawned);
	}

	private void Start() {
		SfxManager.Instance.StartDroneSound();
	}

	protected virtual void OnDestroy() {
		EventManager.Instance.RemoveListener<DroneSpawnedEvent>(this.OnDroneSpawned);
	}

	private void FixedUpdate() {
		// Get player & controls status
		//float vInput = this.CanMove ? Input.GetAxis("Vertical") * Drone.Sensitivity : 0;
		//float hInput = this.CanMove ? Input.GetAxis("Horizontal") * Drone.Sensitivity : 0;
		float mouseXInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse X") * Drone.Sensitivity;
		float mouseYInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse Y") * Drone.Sensitivity;

		// Move target
		Vector3 targetMoveVect = Time.fixedDeltaTime * new Vector3(mouseXInput, mouseYInput, 0);
		this.target.position += targetMoveVect;

		// Calculate move & rotation
		Vector3 moveToTarget = this.target.position - this.rigidbody.position;
		Vector3 moveVect = Time.fixedDeltaTime * (this.CanMove ? this.moveSpeed : 0) * Vector3.ProjectOnPlane(moveToTarget, Vector3.forward).normalized;
		if (moveVect.magnitude > moveToTarget.magnitude)
			moveVect = moveToTarget;
		//float yRot = Time.fixedDeltaTime * this.rotationSpeed * 30 * mouseXInput;
		//if (yRot < ROTATION_EPSILON && yRot > -ROTATION_EPSILON)
		//	yRot = 0;
		//Quaternion qRot = Quaternion.AngleAxis(yRot, this.transform.up);
		//Quaternion qRotUpright = Quaternion.FromToRotation(this.transform.up, Vector3.up);
		//Quaternion qOrientSlightlyUpright = Quaternion.Slerp(this.transform.rotation, qRotUpright * this.transform.rotation, this.yRecoveryStrengh * Time.fixedDeltaTime);

		// Apply them
		this.rigidbody.MovePosition(this.rigidbody.position + moveVect);
		//this.rigidbody.MoveRotation(qRot * qOrientSlightlyUpright);
		//this.rigidbody.angularVelocity = Vector3.zero;
	}

	private void OnDroneSpawned(DroneSpawnedEvent e) {
		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.LookRotation(Vector3.right);
		this.rigidbody.velocity = Vector3.zero;
		this.target.position = this.transform.position;
	}

	private void OnCollisionEnter(Collision collision) {
		if (!collision.collider.isTrigger) {
			SfxManager.Instance.PlaySfx2D("Crash");
			this.CanMove = false;
			this.Invoke("Crash", 3.5f);
		}
	}
}
