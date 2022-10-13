﻿using Common;
using Events;
using UnityEngine;

public class Delivery : MonoBehaviour {
	[SerializeField] private bool end;

	private void OnTriggerEnter(Collider other) {
		if (this.end) {
			DeliverEvent deliverEvent = new DeliverEvent();
			EventManager.Instance.Raise(deliverEvent);
			if (deliverEvent.Success) {
				EventManager.Instance.Raise(new DeliverEndEvent(true));
				SfxManager.Instance.PlaySfx2D("DeliverSuccess");
				Destroy(this.gameObject);
			}
		} else {
			DeliverStartEvent deliverStartEvent = new DeliverStartEvent();
	        EventManager.Instance.Raise(deliverStartEvent);
	        if (deliverStartEvent.CanTake) {
        		SfxManager.Instance.PlaySfx2D("DeliverStart");
        		Destroy(this.gameObject);
	        }
		}
	}
}
