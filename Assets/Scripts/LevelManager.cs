using Events;
using UnityEngine;

public class LevelManager : MonoBehaviour {
	[SerializeField] private Transform droneContainer;

	private int _meters;
	private int _delivered;
	private bool _delivering;

	private int meters {
		get => this._meters;
		set {
			this._meters = value;
			EventManager.Instance.Raise(new MetersUpdatedEvent(value));
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

	protected void Awake() {
		EventManager.Instance.AddListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.AddListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.AddListener<DeliverEndEvent>(this.OnDeliverEnd);
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.RemoveListener<DeliverStartEvent>(this.OnDeliverStart);
		EventManager.Instance.RemoveListener<DeliverEndEvent>(this.OnDeliverEnd);
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.meters = 0;
		this.delivered = 0;
		Instantiate(Resources.Load<GameObject>("Drone"), this.droneContainer);
		EventManager.Instance.Raise(new DroneSpawnedEvent());
	}

	private void OnDeliverStart(DeliverStartEvent e) {
		this.delivering = true;
	}

	private void OnDeliverEnd(DeliverEndEvent e) {
		this.delivering = false;
		if (e.Success)
			this.delivered++;
	}
}
