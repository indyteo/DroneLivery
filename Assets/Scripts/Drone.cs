using Common;
using Events;
using UnityEngine;

public class Drone : MonoBehaviour {
	[SerializeField] private float moveSpeed = 1.5f;

	private bool spawned;
	private Transform target;

	public bool TargetLocked { get; set; }
	public bool CanMove { get; set; } = true;

	public new Rigidbody rigidbody { get; private set; }
	//public new CapsuleCollider collider { get; private set; }

	private static readonly float MOVE_EPSILON = 0.0001f;
	//private static readonly float ROTATION_EPSILON = 0.001f;
	public static float Sensitivity { get; set; } = 1;

	private void Crash() {
		SfxManager.Instance.StopDroneSound();
		EventManager.Instance.Raise(new DroneCrashedEvent());
		DestroyImmediate(this.gameObject);
	}

	private void Awake() {
		this.rigidbody = this.GetComponent<Rigidbody>();
		//this.collider = this.GetComponent<CapsuleCollider>();
		EventManager.Instance.AddListener<DroneSpawnedEvent>(this.OnDroneSpawned);
		EventManager.Instance.AddListener<GameAbortedEvent>(this.OnGameAborted);
	}

	private void Start() {
		SfxManager.Instance.StartDroneSound();
	}

	protected virtual void OnDestroy() {
		EventManager.Instance.RemoveListener<DroneSpawnedEvent>(this.OnDroneSpawned);
		EventManager.Instance.RemoveListener<GameAbortedEvent>(this.OnGameAborted);
	}

	private void FixedUpdate() {
		if (!this.spawned)
			return;

		// Get player & controls status
		//float vInput = this.CanMove ? Input.GetAxis("Vertical") * Drone.Sensitivity : 0;
		//float hInput = this.CanMove ? Input.GetAxis("Horizontal") * Drone.Sensitivity : 0;
		float mouseXInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse X") * Drone.Sensitivity;
		float mouseYInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse Y") * Drone.Sensitivity;

		// Move target
		Vector3 targetMoveVect = Time.fixedDeltaTime * 25 * new Vector3(mouseXInput, mouseYInput, 0);
		Vector3 nextTargetPos = this.target.position + targetMoveVect;
		nextTargetPos.x = Mathf.Clamp(nextTargetPos.x, -1.5f, 1.5f);
		nextTargetPos.y = Mathf.Clamp(nextTargetPos.y, 0, 6f);
		this.target.position = nextTargetPos;

		// Calculate move & rotation
		Vector3 moveToTarget = this.target.position - this.rigidbody.position;
		Vector3 moveVect = Time.fixedDeltaTime * (this.CanMove ? this.moveSpeed : 0) * Vector3.ProjectOnPlane(moveToTarget, Vector3.forward).normalized;
		if (moveVect.sqrMagnitude > moveToTarget.sqrMagnitude)
			moveVect = moveToTarget;
		//float yRot = Time.fixedDeltaTime * this.rotationSpeed * 30 * mouseXInput;
		//if (yRot < ROTATION_EPSILON && yRot > -ROTATION_EPSILON)
		//	yRot = 0;
		//Quaternion qRot = Quaternion.AngleAxis(yRot, this.transform.up);
		//Quaternion qRotUpright = Quaternion.FromToRotation(this.transform.up, Vector3.up);
		//Quaternion qOrientSlightlyUpright = Quaternion.Slerp(this.transform.rotation, qRotUpright * this.transform.rotation, this.yRecoveryStrengh * Time.fixedDeltaTime);

		// Apply them
		this.transform.position += moveVect;
		//this.rigidbody.MovePosition(this.rigidbody.position + moveVect);
		//this.rigidbody.MoveRotation(qRot * qOrientSlightlyUpright);
		//this.rigidbody.angularVelocity = Vector3.zero;

		// Sound
		
		SfxManager.Instance.SetDroneFly(moveVect.sqrMagnitude > MOVE_EPSILON);
		Debug.Log(moveVect.sqrMagnitude > MOVE_EPSILON);
	}

	private void OnDroneSpawned(DroneSpawnedEvent e) {
		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.LookRotation(Vector3.right);
		this.rigidbody.velocity = Vector3.zero;
		this.target = e.Target;
		this.target.position = this.transform.position;
		this.spawned = true;
	}

	private void OnCollisionEnter(Collision collision) {
		if (!collision.collider.isTrigger) {
			SfxManager.Instance.PlaySfx2D("Crash");
			this.CanMove = false;
			this.TargetLocked = true;
			LevelManager.Instance.Crashing = true;
			this.Invoke("Crash", 3.5f);
		}
	}

	private void OnGameAborted(GameAbortedEvent e) {
		DestroyImmediate(this.gameObject);
	}
}
