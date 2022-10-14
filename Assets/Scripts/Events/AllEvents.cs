using UnityEngine;

namespace Events {
	#region Game Events

	public class GamePlayEvent : Event {}

    public class GameOverEvent : Event {
    	public int Meters { get; private set; }
    	public int Delivered { get; private set; }

    	public GameOverEvent(int meters, int delivered) {
    		this.Meters = meters;
            this.Delivered = delivered;
        }
    }

    public class GameAbortedEvent : Event {}

	#endregion

	#region Drone Events

	public class DroneSpawnedEvent : Event {
		public Transform Target { get; private set; }

		public DroneSpawnedEvent(Transform target) {
			this.Target = target;
		}
	}

	public class DroneCrashedEvent : Event {}

	#endregion

	#region Gameplay Events

	public class DeliveryTakeEvent : Event {
		public bool CanTake { get; set; }
	}

	public class DeliveryDropEvent : Event {
		public bool CanDrop { get; set; }
	}

	public class DeliverStartEvent : Event {}

	public class DeliverEndEvent : Event {
		public bool Success { get; private set; }

		public DeliverEndEvent(bool success) {
			this.Success = success;
		}
	}

	#endregion

	#region Values Events

	public class MetersUpdatedEvent : Event {
		public int Meters { get; private set; }

		public MetersUpdatedEvent(int meters) {
			this.Meters = meters;
		}
	}

	public class DeliveredUpdatedEvent : Event {
		public int Delivered { get; private set; }

		public DeliveredUpdatedEvent(int delivered) {
			this.Delivered = delivered;
		}
	}

	public class DeliveringUpdatedEvent : Event {
		public bool Delivering { get; private set; }

		public DeliveringUpdatedEvent(bool delivering) {
			this.Delivering = delivering;
		}
	}

	public class GPSUpdatedEvent : Event {
		public int Direction { get; private set; }

		public GPSUpdatedEvent(int direction = 2) {
			this.Direction = direction;
		}
	}

	#endregion
}
