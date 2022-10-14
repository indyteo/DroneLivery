using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Warning : MonoBehaviour {
	[SerializeField] private Image warn;
	[SerializeField] private AnimationCurve curve;

	private int triggered;

	private IEnumerator Start() {
		Color color = this.warn.color;
		color.a = 0;
		this.warn.color = color;
		while (this.enabled) {
			yield return new WaitWhile(() => this.triggered == 0);
			yield return this.WarnEffect();
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (!other.isTrigger)
			this.triggered++;
	}

	private void OnTriggerExit(Collider other) {
		if (!other.isTrigger)
			this.triggered--;
	}

	private IEnumerator WarnEffect() {
		Color color = this.warn.color;
		yield return this.WarnEffectTransition(color, true);
		yield return new WaitUntil(() => this.triggered == 0);
		yield return this.WarnEffectTransition(color, false);
		color.a = 0;
		this.warn.color = color;
	}

	private IEnumerator WarnEffectTransition(Color color, bool isTransitionIn) {
		float start = Time.time;
		float length = 0.2f;
		float duration;
		while ((duration = Time.time - start) < length) {
			float progress = duration / length;
			color.a = this.curve.Evaluate(isTransitionIn ? progress : 1 - progress);
			this.warn.color = color;
			yield return null;
		}
	}
}
