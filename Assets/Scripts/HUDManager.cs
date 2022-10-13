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
		this.timerText.text = new TimeSpan(0, 0, Mathf.CeilToInt(Time.time - this.start)).ToString("hh'h 'mm'm 'ss's'");
	}

	private void OnGamePlay(GamePlayEvent e) {
		this.start = Time.time;
	}

	private void OnMetersUpdated(MetersUpdatedEvent e) {
		this.metersText.text = e.Meters.ToString();
	}

	private void OnDeliveredUpdated(DeliveredUpdatedEvent e) {
		this.deliveredText.text = e.Delivered.ToString();
	}

	private void OnDeliveringUpdated(DeliveringUpdatedEvent e) {
		this.deliveringIndicator.SetActive(e.Delivering);
	}
}
