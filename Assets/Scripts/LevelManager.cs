using System.Collections.Generic;
using Common;
using Events;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager> {
	[Header("LevelManager")]
	[SerializeField] private Transform droneContainer;
	[SerializeField] private Transform droneTarget;
	[SerializeField] private float speed = 0.5f;
	[SerializeField] private int generateNSections = 10;
	[Header("Features Generation Chance")]
	[SerializeField] private float deliveryChance = 0.01f;
	[Header("Features")]
	[SerializeField] private GameObject road;
	[SerializeField] private GameObject deliveryStart;
	[SerializeField] private GameObject deliveryEnd;

	private int layerDelivery;
	private int layerDeposit;
	public bool Crashing { get; set; }

	private float _meters;
	private int _delivered;
	private bool _delivering;

	private Vector3 start;
	private Vector3 direction = Vector3.forward;
	private Vector3 generatedUntil;
	private List<GameObject> features = new List<GameObject>();

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
		EventManager.Instance.AddListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.AddListener<DeliverEvent>(this.OnDeliver);
		EventManager.Instance.AddListener<DroneCrashedEvent>(this.OnDroneCrashEvent);
	}

	private void Update() {
		if (this.Crashing || !GameManager.Instance.IsPlaying)
			return;
		
		// Move forward
		Vector3 move = Time.deltaTime * this.speed * this.direction;
		this.droneContainer.position += move;
		this.meters += move.magnitude;
		
		// Generate terrain
		Vector3 nextGeneration = this.generatedUntil + this.direction * 3;
		if ((this.droneContainer.position - nextGeneration).sqrMagnitude < (this.generateNSections * this.generateNSections * 9)) {
			this.features.Add(Instantiate(this.road, nextGeneration, this.droneContainer.rotation));
            if (Random.value < this.deliveryChance) {
				GameObject model = this.delivering ? this.deliveryEnd : this.deliveryStart;
				int y = Random.Range(1, 5);
				bool left = Random.value < 0.5;
				int x = left ? -1 : 1;
				Quaternion rotation = left ? TURN_AROUND_Y : Quaternion.identity;
				this.features.Add(Instantiate(model, nextGeneration + x * this.droneContainer.right + y * Vector3.up, this.droneContainer.rotation * rotation));
			}
			this.generatedUntil = nextGeneration;
		}
		
		this.DeleteBackObject();
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.RemoveListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.RemoveListener<DeliverEvent>(this.OnDeliver);
		EventManager.Instance.RemoveListener<DroneCrashedEvent>(this.OnDroneCrashEvent);
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.Crashing = false;
		this.meters = 0;
		this.delivered = 0;
		this.delivering = false;
		this.droneContainer.position = this.start;
		this.DestroyAllFeatures();
		Instantiate(Resources.Load<GameObject>("Drone"), this.droneContainer);
		EventManager.Instance.Raise(new DroneSpawnedEvent(this.droneTarget));
	}

	private void OnDeliverStart(DeliverStartEvent e) {
		if (this.delivering)
			return;
		e.CanTake = true;
		this.delivering = true;
	}

	private void OnDeliver(DeliverEvent e) {
		if (this.delivering) {
			this.delivering = false;
			this.delivered++;
			e.Success = true;
		}
	}

	private void OnDroneCrashEvent(DroneCrashedEvent e) {
		EventManager.Instance.Raise(new GameOverEvent(Mathf.RoundToInt(this.meters), this.delivered));
	}

	private bool IsBackObject(GameObject obj) {
		return Vector3.Dot(this.direction, obj.transform.position + 10 * this.direction - this.droneTarget.position) < 0;
	}

	private void DeleteBackObject() {
		foreach (var o in this.features)
			if (IsBackObject(o))
				Destroy(o);
		this.features.RemoveAll(IsBackObject);
	}

	private void DestroyAllFeatures() {
		foreach (var o in this.features)
			Destroy(o);
		this.features.Clear();
		this.generatedUntil = Vector3.zero;
	}
}
