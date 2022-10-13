﻿using System.Collections.Generic;
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

	public bool Crashing { get; set; }

	private float _meters;
	private int _delivered;
	private bool _delivering;

	private Vector3 start;
	private Vector3 direction = Vector3.forward;
	private Vector3 generatedUntil;
	private List<GameObject> roads = new List<GameObject>();

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
			Instantiate(this.road, nextGeneration, this.droneContainer.rotation);
			if (Random.value < this.deliveryChance) {
				GameObject model = this.delivering ? this.deliveryEnd : this.deliveryStart;
				int y = Random.Range(1, 5);
				bool left = Random.value < 0.5;
				int x = left ? -1 : 1;
				Quaternion rotation = left ? TURN_AROUND_Y : Quaternion.identity;
				Instantiate(model, nextGeneration + x * this.droneContainer.right + y * Vector3.up, this.droneContainer.rotation * rotation);
			}
			this.generatedUntil = nextGeneration;
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
		if (this.delivering)
			return;
		e.CanTake = true;
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
}
