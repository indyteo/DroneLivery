using System;
using System.Collections.Generic;

namespace Events {
	/// <summary>
	///     Event Manager manages publishing raised events to subscribing/listening classes.
	/// 
	///     @example subscribe
	///     EventManager.Instance.AddListener&lt;SomethingHappenedEvent&gt;(OnSomethingHappened);
	/// 
	///     @example unsubscribe
	///     EventManager.Instance.RemoveListener&lt;SomethingHappenedEvent&gt;(OnSomethingHappened);
	/// 
	///     @example publish an event
	///     EventManager.Instance.Raise(new SomethingHappenedEvent());
	/// 
	///     This class is a minor variation on http://www.willrmiller.com/?p=87
	/// </summary>
	public class EventManager {
		public delegate void EventDelegate<T>(T e) where T : Event;

		private static EventManager instance;

		/// <summary>
		///     Lookups only, there is one delegate lookup per listener
		/// </summary>
		private readonly Dictionary<Delegate, EventDelegate> delegateLookup = new Dictionary<Delegate, EventDelegate>();

		/// <summary>
		///     The actual delegate, there is one delegate per unique event. Each
		///     delegate has multiple invocation list items.
		/// </summary>
		private readonly Dictionary<Type, EventDelegate> delegates = new Dictionary<Type, EventDelegate>();

		public static EventManager Instance {
			get {
				if (instance == null)
					instance = new EventManager();
				return instance;
			}
		}

		/// <summary>
		///     The count of delegate lookups. The delegate lookups will increase by
		///     one for each unique AddListener. Useful for debugging and not much else.
		/// </summary>
		public int DelegateLookupCount => this.delegateLookup.Count;

		/// <summary>
		///     Add the delegate.
		/// </summary>
		public void AddListener<T>(EventDelegate<T> del) where T : Event {
			if (this.delegateLookup.ContainsKey(del))
				return;

			// Create a new non-generic delegate which calls our generic one.
			// This is the delegate we actually invoke.
			EventDelegate internalDelegate = e => del((T) e);
			this.delegateLookup[del] = internalDelegate;

			EventDelegate tempDel;
			if (this.delegates.TryGetValue(typeof(T), out tempDel))
				this.delegates[typeof(T)] = tempDel + internalDelegate;
			else
				this.delegates[typeof(T)] = internalDelegate;
		}

		/// <summary>
		///     Remove the delegate. Can be called multiple times on same delegate.
		/// </summary>
		public void RemoveListener<T>(EventDelegate<T> del) where T : Event {
			EventDelegate internalDelegate;
			if (this.delegateLookup.TryGetValue(del, out internalDelegate)) {
				EventDelegate tempDel;
				if (this.delegates.TryGetValue(typeof(T), out tempDel)) {
					tempDel -= internalDelegate;
					if (tempDel == null)
						this.delegates.Remove(typeof(T));
					else
						this.delegates[typeof(T)] = tempDel;
				}

				this.delegateLookup.Remove(del);
			}
		}

		/// <summary>
		///     Raise the event to all the listeners
		/// </summary>
		public void Raise(Event e) {
			EventDelegate del;
			if (this.delegates.TryGetValue(e.GetType(), out del))
				del.Invoke(e);
		}

		private delegate void EventDelegate(Event e);
	}
}
