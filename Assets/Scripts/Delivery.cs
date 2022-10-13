using Common;
using Events;
using UnityEngine;

public class Delivery : MonoBehaviour {
	[SerializeField] private bool end;

	private void OnTriggerEnter(Collider other) {
		if (this.end) {
			DeliveryDropEvent deliveryDropEvent = new DeliveryDropEvent();
			EventManager.Instance.Raise(deliveryDropEvent);
			if (deliveryDropEvent.CanDrop) {
				EventManager.Instance.Raise(new DeliverEndEvent(true));
				SfxManager.Instance.PlaySfx2D("DeliverSuccess");
				Destroy(this.gameObject);
			}
		} else {
			DeliveryTakeEvent deliveryTakeEvent = new DeliveryTakeEvent();
	        EventManager.Instance.Raise(deliveryTakeEvent);
	        if (deliveryTakeEvent.CanTake) {
		        EventManager.Instance.Raise(new DeliverStartEvent());
        		SfxManager.Instance.PlaySfx2D("DeliverStart");
        		Destroy(this.gameObject);
	        }
		}
	}
}
