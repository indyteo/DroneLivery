using System;
using System.Collections;
using Events;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour {
	[Header("Parkour & Delivering info")]
	[SerializeField] private Text scoreInGameText;
	[SerializeField] private Text metersText;
	[SerializeField] private Text deliveredText;
	[SerializeField] private GameObject deliveringIndicator;
	[Header("Timer")]
	[SerializeField] private Text timerText;
	[Header("GPS")]
	[SerializeField] private GameObject left;
	[SerializeField] private GameObject center;
	[SerializeField] private GameObject right;

	private int _delivered;
	private int _meters;
	private float start;

	private int delivered {
		get => this._delivered;
		set {
			this._delivered = value;
			this.deliveredText.text = value.ToString();
			this.UpdateScoreInGame();
		}
	}
	
	private int meters {
		get => this._meters;
		set {
			this._meters = value;
			this.metersText.text = value.ToString();
			this.UpdateScoreInGame();
		}
	}

	private void Awake() {
		EventManager.Instance.AddListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.AddListener<MetersUpdatedEvent>(this.OnMetersUpdated);
		EventManager.Instance.AddListener<DeliveredUpdatedEvent>(this.OnDeliveredUpdated);
		EventManager.Instance.AddListener<DeliveringUpdatedEvent>(this.OnDeliveringUpdated);
		EventManager.Instance.AddListener<GPSUpdatedEvent>(this.OnGPSUpdated);
	}

	private IEnumerator Start() {
		while (this.enabled) {
			this.UpdateTimer();
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.RemoveListener<MetersUpdatedEvent>(this.OnMetersUpdated);
		EventManager.Instance.RemoveListener<DeliveredUpdatedEvent>(this.OnDeliveredUpdated);
		EventManager.Instance.RemoveListener<DeliveringUpdatedEvent>(this.OnDeliveringUpdated);
		EventManager.Instance.RemoveListener<GPSUpdatedEvent>(this.OnGPSUpdated);
	}

	private void UpdateTimer() {
		this.timerText.text = new TimeSpan(0, 0, Mathf.CeilToInt(Time.time - this.start)).ToString("hh':'mm':'ss");
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.start = Time.time;
	}

	private void OnMetersUpdated(MetersUpdatedEvent e) {
		this.meters = e.Meters;
	}

	private void OnDeliveredUpdated(DeliveredUpdatedEvent e) {
		this.delivered = e.Delivered;
	}

	private void OnDeliveringUpdated(DeliveringUpdatedEvent e) {
		this.deliveringIndicator.SetActive(e.Delivering);
	}

	private void OnGPSUpdated(GPSUpdatedEvent e) {
		this.left.SetActive(false);
		this.center.SetActive(false);
		this.right.SetActive(false);
		switch (e.Direction) {
		case -1:
			this.left.SetActive(true);
			break;
		case 0:
			this.center.SetActive(true);
			break;
		case 1:
			this.right.SetActive(true);
			break;
		}
	}

	private void UpdateScoreInGame() {
		this.scoreInGameText.text = "Score " + GameManager.ComputeScore(this.meters, this.delivered);
	}
}
