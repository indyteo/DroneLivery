namespace Events {
	#region Game Events

	public class GamePlayEvent : Event {}

    public class GameOverEvent : Event {
    	public int Score { get; private set; }

    	public GameOverEvent(int score) {
    		this.Score = score;
    	}
    }

	#endregion

	#region Drone Events

	public class DroneSpawnedEvent : Event {}

	public class DroneCrashedEvent : Event {}

	#endregion

	#region Gameplay Events

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

	#endregion
}
