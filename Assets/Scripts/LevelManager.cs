using System.Collections.Generic;
using Common;
using Events;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager> {
	[Header("LevelManager")]
	[SerializeField] private Transform droneContainer;
	[SerializeField] private Transform droneTarget;
	[SerializeField] private float initialSpeed = 1;
	[SerializeField] private float speedUpgrade = 0.25f;
	[SerializeField] private float milestones = 100;
	[SerializeField] private int generateNSections = 10;
	[Header("Features Generation Chance")]
	[SerializeField] private float intersectionChance = 0.01f;
	[SerializeField] private float deliveryChance = 0.01f;
	[SerializeField] private float ponctualChance = 0.15f;
	[SerializeField] private float hBarChance = 0.05f;
	[SerializeField] private float vBarChance = 0.05f;
	[Header("Features")]
	[SerializeField] private GameObject road;
	[SerializeField] private GameObject intersection;
	[SerializeField] private GameObject deliveryStart;
	[SerializeField] private GameObject deliveryEnd;
	[SerializeField] private GameObject balcony;
	[SerializeField] private GameObject bird;
	[SerializeField] private GameObject drone;
	[SerializeField] private GameObject droneDelivery;
	[SerializeField] private GameObject chair;
	[SerializeField] private GameObject bridge;
	[SerializeField] private GameObject tree;
	[SerializeField] private GameObject banner;

	private int layerDelivery;
	private int layerDeposit;
	public bool Move { get; set; }
	public float Speed { get; private set; }
	public bool FollowGPS => this.delivering;

	private float _meters;
	private int _delivered;
	private bool _delivering;

	private Vector3 start;
	private Vector3 direction = Vector3.forward;
	private Vector3 generatedUntil;
	private List<GameObject> features = new List<GameObject>();
	private int canGenerateIntersection;

	private static Quaternion TURN_AROUND_Y = Quaternion.AngleAxis(180, Vector3.up);

	private float meters {
		get => this._meters;
		set {
			this._meters = value;
			EventManager.Instance.Raise(new MetersUpdatedEvent(Mathf.RoundToInt(value)));
		}
	}

	private int delivered {
		get => this._delivered;
		set {
			this._delivered = value;
			EventManager.Instance.Raise(new DeliveredUpdatedEvent(value));
		}
	}

	private bool delivering {
		get => this._delivering;
		set {
			this._delivering = value;
			Camera cam = Camera.main;
			if (value)
				cam.cullingMask = (cam.cullingMask & ~this.layerDelivery) | this.layerDeposit;
			else
				cam.cullingMask = (cam.cullingMask & ~this.layerDeposit) | this.layerDelivery;
			EventManager.Instance.Raise(new DeliveringUpdatedEvent(value));
		}
	}

	protected override void Awake() {
		base.Awake();
		this.start = this.droneContainer.position;
		this.layerDelivery = 1 << LayerMask.NameToLayer("Delivery");
		this.layerDeposit = 1 << LayerMask.NameToLayer("Deposit");
		EventManager.Instance.AddListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.AddListener<DeliveryTakeEvent>(this.OnDeliveryTake);
		EventManager.Instance.AddListener<DeliveryDropEvent>(this.OnDeliveryDrop);
		EventManager.Instance.AddListener<DeliverEndEvent>(this.OnDeliverEnd);
		EventManager.Instance.AddListener<DroneCrashedEvent>(this.OnDroneCrashEvent);
	}

	private void Update() {
		if (!this.Move || !GameManager.Instance.IsPlaying)
			return;

		// Move forward
		Vector3 move = Time.deltaTime * this.Speed * this.direction;
		this.droneContainer.position += move;
		this.meters += move.magnitude;
		this.Speed = this.initialSpeed + Mathf.Clamp(Mathf.Floor(this.meters / this.milestones) * this.speedUpgrade, 0, 2 * this.initialSpeed);

		// Generate terrain
		Vector3 nextGeneration = this.generatedUntil + this.direction * 3;
		if ((this.droneContainer.position - nextGeneration).sqrMagnitude < (this.generateNSections * this.generateNSections * 9)) {
			GameObject roadModel = this.canGenerateIntersection <= 0 && Random.value < this.intersectionChance ? this.intersection : this.road;
			this.features.Add(Instantiate(roadModel, nextGeneration, this.droneContainer.rotation, this.transform));
			if (roadModel == this.intersection)
				this.canGenerateIntersection = 5;
			if (roadModel == this.road) {
				this.canGenerateIntersection--;
				if (Random.value < this.deliveryChance) {
                	GameObject model = this.delivering ? this.deliveryEnd : this.deliveryStart;
                	int y = Random.Range(1, 5);
                	bool left = Random.value < 0.5;
                	int x = left ? -1 : 1;
                	Quaternion rotation = left ? TURN_AROUND_Y : Quaternion.identity;
                	this.features.Add(Instantiate(model, nextGeneration + x * this.droneContainer.right + y * Vector3.up, this.droneContainer.rotation * rotation, this.transform));
                } else if (Random.value < this.ponctualChance) {
                    int y = Random.Range(0, 6);
                    int x = Random.Range(-1, 2);
                    GameObject model = y == 0 ? this.chair : x != 0 && Random.value < 0.25 ? this.balcony : Random.value < 0.65 ? this.bird : Random.value < 0.35 ? this.droneDelivery : this.drone;
                    Quaternion rotation = model == this.balcony && x == -1 ? TURN_AROUND_Y : Quaternion.identity;
                    this.features.Add(Instantiate(model, nextGeneration + x * this.droneContainer.right + y * Vector3.up, this.droneContainer.rotation * rotation, this.transform));
                } else if (Random.value < this.vBarChance) {
                    int x = Random.Range(-1, 2);
                    int y = x == 0 ? 0 : Random.Range(0, 3);
                    GameObject model = y == 0 ? this.tree : this.banner;
                    this.features.Add(Instantiate(model, nextGeneration + x * this.droneContainer.right + y * Vector3.up, this.droneContainer.rotation, this.transform));
                } else if (Random.value < this.hBarChance) {
					int y = Random.Range(1, 6);
					this.features.Add(Instantiate(this.bridge, nextGeneration + y * Vector3.up, this.droneContainer.rotation, this.transform));
				}
			}
			this.generatedUntil = nextGeneration;
		}

		this.DeleteBackObject();
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.RemoveListener<DeliveryTakeEvent>(this.OnDeliveryTake);
		EventManager.Instance.RemoveListener<DeliveryDropEvent>(this.OnDeliveryDrop);
		EventManager.Instance.RemoveListener<DeliverEndEvent>(this.OnDeliverEnd);
		EventManager.Instance.RemoveListener<DroneCrashedEvent>(this.OnDroneCrashEvent);
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.Speed = this.initialSpeed;
		this.meters = 0;
		this.delivered = 0;
		this.delivering = false;
		this.droneContainer.position = this.start;
		this.direction = Vector3.forward;
		this.droneContainer.rotation = Quaternion.identity;
		EventManager.Instance.Raise(new GPSUpdatedEvent());
		this.DestroyAllFeatures();
		Instantiate(Resources.Load<GameObject>("Drone"), this.droneContainer);
		EventManager.Instance.Raise(new DroneSpawnedEvent(this.droneTarget));
		this.Move = true;
	}

	private void OnDeliveryTake(DeliveryTakeEvent e) {
		if (this.delivering)
			return;
		e.CanTake = true;
		this.delivering = true;
	}

	private void OnDeliveryDrop(DeliveryDropEvent e) {
		e.CanDrop = this.delivering;
	}

	private void OnDeliverEnd(DeliverEndEvent e) {
		this.delivering = false;
		if (e.Success)
			this.delivered++;
	}

	private void OnDroneCrashEvent(DroneCrashedEvent e) {
		EventManager.Instance.Raise(new GameOverEvent(Mathf.RoundToInt(this.meters), this.delivered));
	}

	private bool IsBackObject(GameObject obj) {
		return Vector3.Dot(this.direction, obj.transform.position + 10 * this.direction - this.droneTarget.position) < 0;
	}

	private void DeleteBackObject() {
		foreach (var o in this.features)
			if (this.IsBackObject(o))
				Destroy(o);
		this.features.RemoveAll(this.IsBackObject);
	}

	private void DestroyAllFeatures() {
		foreach (var o in this.features)
			Destroy(o);
		this.features.Clear();
		this.generatedUntil = 7 * this.direction;
	}

	public void Rotate(Quaternion rotation, Vector3 position) {
		this.direction = rotation * this.direction;
		this.droneContainer.position = position;
		this.droneContainer.rotation *= rotation;
		this.generatedUntil = this.droneContainer.position - 3 * Vector3.up + 4 * this.direction;
		this.canGenerateIntersection = 5;
	}
}
