using System;
using Common;
using Events;
using Unity.VisualScripting;
using UnityEngine;

public class Delivery : MonoBehaviour {
	private void OnCollisionEnter(Collision collision) {
		EventManager.Instance.Raise(new DeliverStartEvent());
		SfxManager.Instance.PlaySfx2D("DeliverStart");
		Destroy(this.gameObject);
	}
}
