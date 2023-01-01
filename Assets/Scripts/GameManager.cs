using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState 
{
    MainMenu,
    DifficultyMenu,
    CreditsMenu,
    MainOptionsMenu,
    PausedOptionsMenu,
    HowToPlayMenu,
    Playing,
    Paused,
    Victory,
    Lose,
    Reloaded
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Expert
} 

public class GameManager : MonoBehaviour
{
    private const float transitionTime = 3.2f;
    private const int MaxScore = 7;

    public static GameManager Instance;
    public static int score;
    private static string playerReloadChoice = null;
    private static Difficulty playerReloadDifficulty = Difficulty.Easy;
    private static string playerReloadTheme = "";

    private string themeSong;
    private bool gameIsPreparing;
    private GameState state;
    private Difficulty difficulty;

    private TransitionHandler transitionHandler;
    private PenguinSpawner penguinSpawner;
    private VehicleSpawner vehicleSpawner;
    private PlayerInputActions playerInputActions;

    private TMPro.TextMeshProUGUI scoreText;
    private TMPro.TextMeshProUGUI timerText;
    private Image sliderFill;

    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject difficultyMenu;
    [SerializeField] GameObject winLoseMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject howToPlayMenu;
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject playerUI;
    [SerializeField] GameObject scoreboard;
    [SerializeField] Slider timerSlider;
    [SerializeField] float maxTime = 60f; //default is 1 minute
    private float timeLeft;
    private bool timerCanTick;

    private void Awake() 
    {
        Instance = this;
        Instance.state = GameState.MainMenu;
        Instance.difficulty = Difficulty.Easy;
        Instance.gameIsPreparing = false;
        Instance.themeSong = "";

        timerCanTick = false;
        score = 0;
        scoreText = scoreboard.GetComponent<TMPro.TextMeshProUGUI>();
        timerText = timerSlider.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        sliderFill = timerSlider.GetComponentsInChildren<Image>()[1];

        timerSlider.maxValue = maxTime;
        timerSlider.value = maxTime;
        timeLeft = maxTime;
        timerText.SetText(String.Format("{0:0}:{1:00}", Mathf.FloorToInt((timeLeft) / 60f), Mathf.CeilToInt((timeLeft) % 60f)));

        transitionHandler = GetComponent<TransitionHandler>();
        penguinSpawner = GetComponent<PenguinSpawner>();
        vehicleSpawner = GetComponent<VehicleSpawner>();
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable() 
    {
        playerInputActions.GameFlow.Enable();
        playerInputActions.GameFlow.PauseGame.performed += OnPauseInput;
    }

    private void Start()
    {
        SetInitialScoreboard();
        Enum.TryParse(playerReloadChoice, out GameState reloadResult);
        if (IsReloaded(reloadResult))
            ReloadGame(); //must be called in Start() to allow AudioManager to attach AudioSources
    }

    private void FixedUpdate() 
    {
        HandleTimer();
        CheckForGameWinLose();
        LoopTheme();
        
        if (vehicleSpawner.CanSpawnAVehicle())
            StartCoroutine(vehicleSpawner.SpawnVehicle());
        if (GameIsPlaying() && penguinSpawner.CanSpawnAPenguin())
            penguinSpawner.SpawnPenguin();
    }

    private void OnDisable()
    {
        playerInputActions.GameFlow.Disable();
        playerInputActions.GameFlow.PauseGame.performed -= OnPauseInput;
    }

    private void CheckForGameWinLose()
    {
        if (score == MaxScore)
            UpdateGameState(GameState.Victory);
        else if (score < 0)
            UpdateGameState(GameState.Lose);
    }

    private void LoopTheme()
    {
        if (GameIsPlaying() && !AudioManager.Instance.IsSongPlaying(themeSong))
            AudioManager.Instance.Play(themeSong);
    }

    public string GetCurrentTheme()
    {
        return themeSong;
    }

    private bool GameIsPlaying()
    {
        return GetState() == GameState.Playing;
    }

    private void HandleTimer()
    {
        if (!timerCanTick)
            return;

        timeLeft -= Time.deltaTime; //FixedUpdate() will run at 50 calls/second: 50*Time.deltaTime == 1 
        if (timeLeft <= 0f)
        {
            ShowGameOverTime();
            UpdateGameState(GameState.Lose);
        }
        else if (Mathf.CeilToInt(timeLeft) % 60 == 0)
            ShowSingleMinuteTime();
        else
            ShowGameTime();
    }

    private void ShowGameOverTime()
    {
        timerSlider.value = 0f;
        timerText.SetText("0:00");
    }

    private void ShowSingleMinuteTime()
    {
        timerText.SetText(String.Format("{0:0}:{1:00}", Mathf.CeilToInt((timeLeft) / 60f), 0));
        timerSlider.value = timeLeft;
    }

    private void ShowGameTime()
    {
        //set 0th argument to minutes formatted like '0' and set 1st argument to seconds formatted like '00'
        timerText.SetText(String.Format("{0:0}:{1:00}", Mathf.FloorToInt((timeLeft) / 60f), Mathf.CeilToInt((timeLeft) % 60f)));
        timerSlider.value = timeLeft;

        if (timeLeft <= maxTime * .20f && sliderFill.color.r != 255f/255f)
            ShowWarningTimeColor();
    }

    private void ShowWarningTimeColor()
    {
        Color red = new Color(255f/255f, 74f/255f, 74f/255f);
        sliderFill.color = red;
    }

    public GameState GetState()
    {
        return Instance.state;
    }

    public void UpdateGameState(GameState newState)
    {
        DeactivateViews(newState);
        Instance.state = newState;
        PlayStateTransitionAudio();

        if (GameIsPreparing())
        {
            ShowFadeTransition();
            StartCoroutine(PrepareGame());
        }

        ActivateViews();
    }

    public bool GameIsPreparing()
    {
        return Instance.gameIsPreparing;
    }

    public void SetGameIsPreparing()
    {
        Instance.gameIsPreparing = true;
    }

    private void SetGamePreparationFinished()
    {
        Instance.gameIsPreparing = false;
    }

    public void DecrementScore()
    {
        if (GetDifficulty() != Difficulty.Easy)
        {
            --score;
            UpdateScoreboard();
        }   
    }

    public void SetThemeMusic(string theme)
    {
        themeSong = theme;
    }

    public void SetDifficulty(Difficulty difficultyChoice)
    {
        Instance.difficulty = difficultyChoice;
    }

    public Difficulty GetDifficulty()
    {
        return Instance.difficulty;
    }

    private void DeactivateViews(GameState nextState)
    {
        if (nextState == GameState.Playing)
            menuUI.SetActive(false);

        switch (Instance.state)
        {
            case GameState.Playing:
                return;
            case GameState.MainOptionsMenu:
            case GameState.PausedOptionsMenu:
                optionsMenu.SetActive(false);
                break;
            case GameState.CreditsMenu:
                creditsMenu.SetActive(false);
                break;
            case GameState.MainMenu:
                mainMenu.SetActive(false);
                break;
            case GameState.DifficultyMenu:
                difficultyMenu.SetActive(false);
                break;
            case GameState.HowToPlayMenu:
                howToPlayMenu.SetActive(false);
                break;
            case GameState.Paused:
                pauseMenu.SetActive(false);
                break;
            case GameState.Victory:
            case GameState.Lose:
                winLoseMenu.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Instance.state), Instance.state, null);
        }
    }

    private void PlayStateTransitionAudio()
    {
        if (Instance.state != GameState.Victory && Instance.state != GameState.Lose)
            AudioManager.Instance.Play(AudioManager.MenuSelection);
    }

    private void ShowFadeTransition()
    {
        transitionHandler.TransitionToGame();
    }

    private IEnumerator PrepareGame()
    {
        yield return new WaitForSeconds(transitionTime);

        playerUI.SetActive(true);
        timerCanTick = true;
        SetGamePreparationFinished();
    }

    private void ActivateViews()
    {
        if (Instance.state != GameState.Playing)
            menuUI.SetActive(true);

        switch (Instance.state)
        {
             case GameState.Playing:
                if (!GameIsPreparing())
                    playerUI.SetActive(true);
                break;
            case GameState.MainOptionsMenu:
            case GameState.PausedOptionsMenu:
                optionsMenu.SetActive(true);
                break;
            case GameState.CreditsMenu:
                creditsMenu.SetActive(true);
                break;
            case GameState.MainMenu:
                mainMenu.SetActive(true);
                break;
            case GameState.DifficultyMenu:
                difficultyMenu.SetActive(true);
                break;
            case GameState.HowToPlayMenu:
                howToPlayMenu.SetActive(true);
                break;
            case GameState.Paused:
                pauseMenu.SetActive(true);
                break;
            case GameState.Victory:
            case GameState.Lose:
                winLoseMenu.SetActive(true);
                winLoseMenu.GetComponent<WinLoseMenu>().OpenWinLoseMenu(Instance.state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Instance.state), Instance.state, null);
        }
    }

    private void SetInitialScoreboard()
    {
        scoreText.text = ($"{score}/{MaxScore}");
    }

    public void UpdateScoreboard()
    {
        scoreText.text = score >= 0 ? ($"{score}/{MaxScore}") : ($"0/{MaxScore}");
    }

    public void UpdateScoreboardWithSound()
    {
        scoreText.text = score >= 0 ? ($"{score}/{MaxScore}") : ($"0/{MaxScore}");
        if (score != MaxScore)
            AudioManager.Instance.Play(AudioManager.MenuSelection);
    }

    public void ResetGame()
    {
        playerReloadChoice = null;

        ResumeTimeAndLoadSceneForDifficulty();
    }

    public void PlayAgain()
    {
        playerReloadChoice = "Reloaded";

        ResumeTimeAndLoadSceneForDifficulty();
    }

    private void ResumeTimeAndLoadSceneForDifficulty()
    {
        playerReloadDifficulty = GetDifficulty();
        playerReloadTheme = themeSong;

        VehicleSpawner.ResetSpawnerForReloadGame();
        StopAllCoroutines();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool IsReloaded(GameState reloadChoice)
    {
        return reloadChoice == GameState.Reloaded;
    }

    public void ReloadGame()
    {
        menuUI.SetActive(false);
        mainMenu.SetActive(false);

        SetThemeMusic(playerReloadTheme);
        SetDifficulty(playerReloadDifficulty);
        SetGameIsPreparing();
        UpdateGameState(GameState.Playing);
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        if (context.performed && GameIsPlaying() && !GameIsPreparing())
        {
            UpdateGameState(GameState.Paused);
            Time.timeScale = 0f;

            if (AudioManager.Instance.IsSongPlaying(themeSong))
                AudioManager.Instance.Pause(themeSong);
        }
    }
}
