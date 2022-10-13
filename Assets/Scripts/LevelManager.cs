using System.Collections.Generic;
using Common;
using Events;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {
	[Header("LevelManager")]
	[SerializeField] private Transform droneContainer;
	[SerializeField] private Transform droneTarget;
	[SerializeField] private int generateNSections = 10;

	[SerializeField] private GameObject road;

	public bool Crashing { get; set; }

	private float _meters;
	private int _delivered;
	private bool _delivering;

	private Vector3 start;
	private float speed = 3f;
	private Vector3 direction = Vector3.forward;
	private Vector3 generatedUntil;
	private List<GameObject> roads = new List<GameObject>();

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
			EventManager.Instance.Raise(new DeliveringUpdatedEvent(value));
		}
	}

	protected override void Awake() {
		base.Awake();
		this.start = this.droneContainer.position;
		EventManager.Instance.AddListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.AddListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.AddListener<DeliverEndEvent>(this.OnDeliverEnd);
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
			this.roads.Add(Instantiate(this.road, nextGeneration, Quaternion.identity));
			this.generatedUntil = nextGeneration;
			this.DeleteBackedObject();
		}
		
		
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.RemoveListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.RemoveListener<DeliverEndEvent>(this.OnDeliverEnd);
		EventManager.Instance.RemoveListener<DroneCrashedEvent>(this.OnDroneCrashEvent);
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.Crashing = false;
		this.meters = 0;
		this.delivered = 0;
		this.droneContainer.position = this.start;
		Instantiate(Resources.Load<GameObject>("Drone"), this.droneContainer);
		EventManager.Instance.Raise(new DroneSpawnedEvent(this.droneTarget));
	}

	private void OnDeliverStart(DeliverStartEvent e) {
		this.delivering = true;
	}

	private void OnDeliverEnd(DeliverEndEvent e) {
		this.delivering = false;
		if (e.Success)
			this.delivered++;
	}

	private void OnDroneCrashEvent(DroneCrashedEvent e) {
		EventManager.Instance.Raise(new GameOverEvent(Mathf.RoundToInt(this.meters), this.delivered));
	}

	private bool IsBackObject(GameObject gameObject) {
		return gameObject.transform.position.z+20 < this.droneTarget.position.z;
	}

	private void DeleteBackedObject() {
		foreach (var o in this.roads) {
			if (IsBackObject(o)) Destroy(o);
		}
		this.roads.RemoveAll(IsBackObject);
	}
	
	
}
