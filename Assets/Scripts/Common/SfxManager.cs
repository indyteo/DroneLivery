using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Common {
	[Serializable]
	public class MyAudioClip {
		public AudioClip clip;
		public float volume;

		public MyAudioClip(AudioClip clip, float volume) {
			this.clip = clip;
			this.volume = volume;
		}
	}

	/// <summary>
	///     Sfx manager.
	/// </summary>
	public class SfxManager : Singleton<SfxManager> {
		[Header("SfxManager")]
		[SerializeField] private TextAsset sfxXmlSetup;

		[SerializeField] private string resourcesFolderName;

		[SerializeField] private int nAudioSources = 2;
		[SerializeField] private GameObject audioSourceModel;

		[SerializeField] private AudioSource backgroundMusic;
		[SerializeField] private AudioSource droneSound;
		[SerializeField] private float droneVolume = 0.075f;
		[SerializeField] private float droneFlyVolume = 0.15f;

		private readonly List<AudioSource> audioSources = new List<AudioSource>();
		private readonly Dictionary<string, MyAudioClip> dicoAudioClips = new Dictionary<string, MyAudioClip>();

		// Use this for initialization
		private void Start() {
			CsharpUtils.FixCsharpBadDecimalSeparator();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(this.sfxXmlSetup.text);

			foreach (XmlNode node in xmlDoc.GetElementsByTagName("SFX"))
				if (node.NodeType != XmlNodeType.Comment)
					this.dicoAudioClips.Add(
							node.Attributes["name"].Value,
							new MyAudioClip(
									(AudioClip) Resources.Load(this.resourcesFolderName + "/" + node.Attributes["name"].Value, typeof(AudioClip)),
									float.Parse(node.Attributes["volume"].Value)
							)
					);

			this.audioSources.Add(this.audioSourceModel.GetComponent<AudioSource>());
			for (int i = 0; i < this.nAudioSources - 1; i++)
				this.AddAudioSource();
		}

		private void AddAudioSource() {
			GameObject newGO = Instantiate(this.audioSourceModel, this.transform);
			newGO.name = "AudioSource";

			AudioSource audioSource = newGO.GetComponent<AudioSource>();
			this.audioSources.Add(audioSource);

			audioSource.loop = false;
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 1;
		}

		public void PlaySfx3D(string sfxName, Vector3 pos) {
			this.PlaySfx(sfxName, pos);
		}

		public void PlaySfx2D(string sfxName) {
			this.PlaySfx3D(sfxName, Camera.main.transform.position);
		}

		private void PlaySfx(string sfxName, Vector3 pos) {
			MyAudioClip audioClip;
			if (!this.dicoAudioClips.TryGetValue(sfxName, out audioClip)) {
				Debug.LogError("SFX, no audio clip with name: " + sfxName);
				return;
			}

			AudioSource audioSource = this.audioSources.Find(item => !item.isPlaying);
			if (audioSource) {
				audioSource.transform.position = pos;
				audioSource.PlayOneShot(audioClip.clip, audioClip.volume);
			}
		}

		public void StartBackgroundMusic() {
			this.backgroundMusic.Play();
		}

		public void HaltBackgroundMusic() {
			this.backgroundMusic.Pause();
		}

		public void StopBackgroundMusic() {
			this.backgroundMusic.Stop();
		}

		public void StartDroneSound() {
			this.PlaySfx2D("DroneStart");
			this.droneSound.PlayDelayed(3);
			this.droneSound.volume = this.droneVolume;
		}

		public void SetDroneFly(bool flying) {
			this.droneSound.volume = flying ? this.droneFlyVolume : this.droneVolume;
		}

		public void StopDroneSound() {
			this.droneSound.Stop();
		}
	}
}
