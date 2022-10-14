using Common;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum GameState {
	TitleScreen,
	Play,
	Pause,
	End
}

public class GameManager : Singleton<GameManager> {
	[Header("Title Screen")]
	[SerializeField] private GameObject titleScreen;
	[SerializeField] private Button playButton;
	[SerializeField] private Button quitButton;
	[SerializeField] private Slider volumeSlider;
	[SerializeField] private Text volumeText;
	[SerializeField] private Slider sensitivitySlider;
	[SerializeField] private Text sensitivityText;
	[Header("End Overlay")]
	[SerializeField] private GameObject endOverlay;
	[SerializeField] private Text metersText;
	[SerializeField] private Text deliveredText;
	[SerializeField] private Text scoreText;
	[SerializeField] private Text highScoreText;
	[SerializeField] private GameObject newHighScore;
	[SerializeField] private Button continueButton;
	[SerializeField] private Button playAgainButton;
	[Header("Pause Overlay")]
	[SerializeField] private GameObject pauseOverlay;
	[SerializeField] private Button resumeButton;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button menuButton;
	
	private GameState gameState;

	public bool IsPlaying => this.gameState == GameState.Play;

	public int HighScore {
		get => PlayerPrefs.GetInt("HighScore", 0);
		private set => PlayerPrefs.SetInt("HighScore", value);
	}

	protected override void Awake() {
		base.Awake();
		EventManager.Instance.AddListener<GameOverEvent>(this.OnGameOver);
		// Title Screen
		this.playButton.onClick.AddListener(this.Play);
		this.quitButton.onClick.AddListener(this.Quit);
		this.volumeSlider.onValueChanged.AddListener(this.Volume);
		this.sensitivitySlider.onValueChanged.AddListener(this.Sensitivity);
		// End Overlay
		this.continueButton.onClick.AddListener(this.Menu);
		this.playAgainButton.onClick.AddListener(this.Play);
		// Pause Overlay
		this.resumeButton.onClick.AddListener(this.Resume);
		this.restartButton.onClick.AddListener(this.Restart);
		this.menuButton.onClick.AddListener(this.Menu);
	}

	private void Start() {
		this.TitleScreen();

		Button[] buttons = FindObjectsOfType<Button>(true);
		for (int i = 0; i < buttons.Length; i++) 
			buttons[i].onClick.AddListener(this.Click);

		this.volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1);
		this.sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1);
	}

	private void Update() {
		if (Input.GetButtonDown("Cancel")) {
			if (this.gameState == GameState.Play)
            	this.Pause();
            else if (this.gameState == GameState.Pause)
            	this.Resume();
			else if (this.gameState == GameState.End)
				this.Menu();
		}

		if (Input.GetButtonDown("Jump") && this.gameState == GameState.Pause)
			this.pauseOverlay.SetActive(!this.pauseOverlay.activeSelf);
	}

	private void OnDestroy() {
		EventManager.Instance.RemoveListener<GameOverEvent>(this.OnGameOver);
		// Title Screen
		this.playButton.onClick.RemoveListener(this.Play);
		this.quitButton.onClick.RemoveListener(this.Quit);
		this.volumeSlider.onValueChanged.RemoveListener(this.Volume);
		this.sensitivitySlider.onValueChanged.RemoveListener(this.Sensitivity);
		// End Overlay
		this.continueButton.onClick.RemoveListener(this.Menu);
		this.playAgainButton.onClick.RemoveListener(this.Play);
		// Pause Overlay
		this.resumeButton.onClick.RemoveListener(this.Resume);
		this.restartButton.onClick.RemoveListener(this.Restart);
		this.menuButton.onClick.RemoveListener(this.Menu);

		Button[] buttons = FindObjectsOfType<Button>(true);
		for (int i = 0; i < buttons.Length; i++) 
			buttons[i].onClick.RemoveListener(this.Click);
	}

	private void TitleScreen() {
		this.gameState = GameState.TitleScreen;
		this.titleScreen.SetActive(true);
        this.endOverlay.SetActive(false);
        EventSystem.current.SetSelectedGameObject(this.playButton.gameObject);
        Time.timeScale = 0;
        UnlockCursor();
	}

	private void Play() {
		this.gameState = GameState.Play;
		this.titleScreen.SetActive(false);
		this.endOverlay.SetActive(false);
		Time.timeScale = 1;
		LockCursor();
		SfxManager.Instance.StartBackgroundMusic();
		EventManager.Instance.Raise(new GamePlayEvent());
	}

	private void End(int meters, int delivered, int score, bool isNewHighScore) {
		this.gameState = GameState.End;
		this.metersText.text = meters.ToString();
		this.deliveredText.text = delivered.ToString();
		this.scoreText.text = $"Score {score}";
		this.highScoreText.gameObject.SetActive(!isNewHighScore);
		this.highScoreText.text = $"High Score {this.HighScore}";
		this.newHighScore.SetActive(isNewHighScore);
		this.endOverlay.SetActive(true);
		EventSystem.current.SetSelectedGameObject(this.playAgainButton.gameObject);
		Time.timeScale = 0;
		UnlockCursor();
		SfxManager.Instance.StopBackgroundMusic();
	}

	private void Pause() {
		this.gameState = GameState.Pause;
		this.pauseOverlay.SetActive(true);
		EventSystem.current.SetSelectedGameObject(this.resumeButton.gameObject);
		Time.timeScale = 0;
		UnlockCursor();
		SfxManager.Instance.HaltBackgroundMusic();
		SfxManager.Instance.HaltDroneSound();
	}

	private void Resume() {
		this.gameState = GameState.Play;
		this.pauseOverlay.SetActive(false);
		Time.timeScale = 1;
		LockCursor();
		SfxManager.Instance.ResumeBackgroundMusic();
		SfxManager.Instance.ResumeDroneSound();
	}

	private void Restart() {
		this.pauseOverlay.SetActive(false);
		EventManager.Instance.Raise(new GameAbortedEvent());
		this.Play();
	}

	private void Menu() {
		if (this.gameState == GameState.Pause)
			EventManager.Instance.Raise(new GameAbortedEvent());
		this.gameState = GameState.TitleScreen;
		this.pauseOverlay.SetActive(false);
		this.titleScreen.SetActive(true);
	}

	private void Quit() {
		Application.Quit();
	}

	private void Click() {
		SfxManager.Instance.PlaySfx2D("Click");
	}

	private void Volume(float volume) {
		PlayerPrefs.SetFloat("Volume", volume);
		this.volumeText.text = $"{Mathf.Round(volume * 100)}%";
		AudioListener.volume = volume;
	}

	private void Sensitivity(float sensitivity) {
		PlayerPrefs.SetFloat("Sensitivity", sensitivity);
		this.sensitivityText.text = $"{Mathf.Round(sensitivity * 100)}%";
		Drone.Sensitivity = sensitivity;
	}

	private void OnGameOver(GameOverEvent e) {
		int score = ComputeScore(e.Meters, e.Delivered);
		bool isNewHighScore = score > this.HighScore;
		if (isNewHighScore)
			this.HighScore = score;
		this.End(e.Meters, e.Delivered, score, isNewHighScore);
	}

	public static void LockCursor() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public static void UnlockCursor() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public static int ComputeScore(int meters, int delivered) {
		return meters + 250 * delivered;
	}
}
