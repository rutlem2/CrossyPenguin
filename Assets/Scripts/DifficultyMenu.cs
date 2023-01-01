using UnityEngine;
using UnityEngine.UI;

public class DifficultyMenu : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI difficultyDescription;
    [SerializeField] Button playButton;
    [SerializeField] GameObject playButtonParticlesWrapper;

    public void SelectEasy()
    {
        GameManager.Instance.SetThemeMusic(AudioManager.EasyTheme);
        GameManager.Instance.SetDifficulty(Difficulty.Easy);
        ShowReadyGameView();
    }

    public void SelectMedium()
    {
        GameManager.Instance.SetThemeMusic(AudioManager.MediumTheme);
        GameManager.Instance.SetDifficulty(Difficulty.Medium);
        ShowReadyGameView();
    }

    public void SelectHard()
    {
        GameManager.Instance.SetThemeMusic(AudioManager.HardTheme);
        GameManager.Instance.SetDifficulty(Difficulty.Hard);
        ShowReadyGameView();
    }

    public void SelectExpert()
    {
        GameManager.Instance.SetThemeMusic(AudioManager.ExpertTheme);
        GameManager.Instance.SetDifficulty(Difficulty.Expert);
        ShowReadyGameView();
    }

    private void ShowReadyGameView()
    {
        SetDescriptionText();
        playButton.gameObject.SetActive(true);
        playButtonParticlesWrapper.SetActive(true);
    }

    private void SetDescriptionText()
    {
        switch (GameManager.Instance.GetDifficulty())
        {
            case Difficulty.Easy:
                difficultyDescription.SetText("Piece of cake. <sprite=1>");
                break;
            case Difficulty.Medium:
                difficultyDescription.SetText("Typical Tuesday traffic. <sprite=3>");
                break;
            case Difficulty.Hard:
                difficultyDescription.SetText("Feelin' like a freeway. <sprite=9>");
                break;
            case Difficulty.Expert:
                difficultyDescription.SetText("L.A. at rush hour. <sprite=15>");
                break;
            default:
                Debug.LogError($"Unknown difficulty~~ #{GameManager.Instance.GetDifficulty()} ~~selected, proceeding with Easy Difficulty");
                GameManager.Instance.SetDifficulty(Difficulty.Easy);
                break;
        }
    }

    public void DifficultyBackButton()
    {
        GameManager.Instance.UpdateGameState(GameState.MainMenu); 
    }

    public void StartGame()
    {
        GameManager.Instance.SetGameIsPreparing();
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }
}
