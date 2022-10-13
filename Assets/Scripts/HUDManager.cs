using System;
using System.Collections;
using Events;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour {
	[Header("Parkour & Delivering info")]
	[SerializeField] private Text metersText;
	[SerializeField] private Text deliveredText;
	[SerializeField] private GameObject deliveringIndicator;
	[Header("Timer")]
	[SerializeField] private Text timerText;
	[SerializeField] private Text scoreInGameText;


	private int _delivered;
	private int _meters;

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
	
	private float start;

	private void Awake() {
		EventManager.Instance.AddListener<GamePlayEvent>(this.OnGamePlay);
		EventManager.Instance.AddListener<MetersUpdatedEvent>(this.OnMetersUpdated);
		EventManager.Instance.AddListener<DeliveredUpdatedEvent>(this.OnDeliveredUpdated);
		EventManager.Instance.AddListener<DeliveringUpdatedEvent>(this.OnDeliveringUpdated);
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
	}

	private void UpdateTimer() {
		this.timerText.text = new TimeSpan(0, 0, Mathf.CeilToInt(Time.time - this.start)).ToString("hh':'mm':'ss");
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.start = Time.time;
	}

	private void OnMetersUpdated(MetersUpdatedEvent e) {
		this.metersText.text = e.Meters.ToString();
	}

	private void OnDeliveredUpdated(DeliveredUpdatedEvent e) {
		this.delivered = e.Delivered;
	}

	private void OnDeliveringUpdated(DeliveringUpdatedEvent e) {
		this.deliveringIndicator.SetActive(e.Delivering);
	}

	private void UpdateScoreInGame() {
		this.scoreInGameText.text = "Score " + GameManager.ComputeScore(this.delivered, this.meters);
	}
}
