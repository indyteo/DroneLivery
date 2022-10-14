using Common;
using Events;
using UnityEngine;

public class Drone : MonoBehaviour {
	[SerializeField] private float moveSpeed = 1.5f;
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private GameObject deliveryModel;

	private bool spawned;
	private Transform target;
	private GameObject delivery;

	public bool TargetLocked { get; set; }
	public bool CanMove { get; set; } = true;

	public new Rigidbody rigidbody { get; private set; }

	private static readonly float MOVE_EPSILON = 0.0001f;
	public static float Sensitivity { get; set; } = 1;

	private void Crash() {
		EventManager.Instance.Raise(new DroneCrashedEvent());
		DestroyImmediate(this.gameObject);
	}

	private void Awake() {
		this.rigidbody = this.GetComponent<Rigidbody>();
		EventManager.Instance.AddListener<DroneSpawnedEvent>(this.OnDroneSpawned);
		EventManager.Instance.AddListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.AddListener<DeliverEndEvent>(this.OnDeliverEnd);
		EventManager.Instance.AddListener<GameAbortedEvent>(this.OnGameAborted);
	}

	private void Start() {
		SfxManager.Instance.StartDroneSound();
	}

	protected virtual void OnDestroy() {
		EventManager.Instance.RemoveListener<DroneSpawnedEvent>(this.OnDroneSpawned);
		EventManager.Instance.RemoveListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.RemoveListener<DeliverEndEvent>(this.OnDeliverEnd);
		EventManager.Instance.RemoveListener<GameAbortedEvent>(this.OnGameAborted);
	}

	private void FixedUpdate() {
		if (!this.spawned)
			return;

		// Intersection
		float hInput = this.CanMove ? Input.GetAxis("Horizontal") * Drone.Sensitivity : 0;
		if (Intersection.CanTurn(this)) {
			if (hInput < -0.5)
				Intersection.Turn(this, true);
			else if (hInput > 0.5)
				Intersection.Turn(this, false);
		}

		// Get player & controls status
		float mouseXInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse X") * Drone.Sensitivity;
		float mouseYInput = this.TargetLocked ? 0 : Input.GetAxisRaw("Mouse Y") * Drone.Sensitivity;

		// Move target
		Vector3 targetMoveVect = Time.fixedDeltaTime * 25 * new Vector3(mouseXInput, mouseYInput, 0);
		Vector3 nextTargetPos = this.target.localPosition + targetMoveVect;
		nextTargetPos.x = Mathf.Clamp(nextTargetPos.x, -1.5f, 1.5f);
		nextTargetPos.y = Mathf.Clamp(nextTargetPos.y, -3, 3);
		this.target.localPosition = nextTargetPos;

		// Calculate move & rotation
		Vector3 moveToTarget = nextTargetPos - this.transform.localPosition;
		Vector3 moveVect = (this.CanMove ? Time.fixedDeltaTime * (this.moveSpeed + LevelManager.Instance.Speed * 0.2f) : 0) * Vector3.ProjectOnPlane(moveToTarget, Vector3.forward).normalized;
		if (moveVect.magnitude > moveToTarget.magnitude)
			moveVect = moveToTarget;

		// Visual rotation from movement
		float targetAngle = -15 * moveVect.x / Time.fixedDeltaTime;
		float currentAngle = this.transform.localEulerAngles.z;
		if (currentAngle > 180)
			currentAngle -= 360;
		float rotateToAngle = targetAngle - currentAngle;
		float rotation = this.CanMove ? Mathf.Sign(rotateToAngle) * Time.fixedDeltaTime * this.rotationSpeed : 0;
		rotation = Mathf.Clamp(rotation, -Mathf.Abs(rotateToAngle), Mathf.Abs(rotateToAngle));
		float nextAngle = Mathf.Clamp(currentAngle + rotation, -45, 45);
		if (nextAngle < 0)
			nextAngle += 360;

		// Apply them
		this.transform.localPosition += moveVect;
		this.transform.localEulerAngles = new Vector3(0, 0, nextAngle);

		// Sound
		SfxManager.Instance.SetDroneFly(moveVect.magnitude > MOVE_EPSILON);
	}

	private void OnDroneSpawned(DroneSpawnedEvent e) {
		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.LookRotation(Vector3.right);
		this.rigidbody.velocity = Vector3.zero;
		this.target = e.Target;
		this.target.position = this.transform.position;
		this.spawned = true;
	}

	private void OnDeliverStart(DeliverStartEvent e) {
		if (this.delivery == null)
			this.delivery = Instantiate(this.deliveryModel, this.transform);
	}

	private void OnDeliverEnd(DeliverEndEvent e) {
		if (this.delivery != null)
			Destroy(this.delivery);
	}

	private void OnCollisionEnter(Collision collision) {
		if (!collision.collider.isTrigger) {
			Instantiate(Resources.Load<GameObject>("Crash"), collision.GetContact(0).point, Quaternion.identity, this.transform);
			SfxManager.Instance.StopDroneSound();
			SfxManager.Instance.PlaySfx2D("Crash");
			this.CanMove = false;
			this.TargetLocked = true;
			LevelManager.Instance.Move = false;
			this.Invoke("Crash", 3.5f);
		}
	}

	private void OnGameAborted(GameAbortedEvent e) {
		DestroyImmediate(this.gameObject);
	}
}
