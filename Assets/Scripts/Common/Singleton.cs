using UnityEngine;

namespace Common {
	public abstract class Singleton<T> : MonoBehaviour where T : Component {
		[Header("Singleton")]
		[SerializeField] private bool doNotDestroyGameObjectOnLoad;

		public static T Instance { get; private set; }

		protected virtual void Awake() {
			if (Instance != null)
				Destroy(this.gameObject);
			else
				Instance = this as T;

			if (this.doNotDestroyGameObjectOnLoad)
				DontDestroyOnLoad(this.gameObject);
		}
	}
}
