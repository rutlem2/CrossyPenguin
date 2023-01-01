using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseMenu : MonoBehaviour
{
    const float timeIncrement = 0.01f;
    const float buttonDelayTime = 2.0f;
    [SerializeField] TMPro.TextMeshProUGUI victoryLossMsg;
    [SerializeField] TMPro.TextMeshProUGUI playAgainText;
    [SerializeField] TMPro.TextMeshProUGUI exitButtonText;
    [SerializeField] Button playAgainButton;
    [SerializeField] Button exitButton;
    float alpha = 0;

    public void OpenWinLoseMenu(GameState instanceState)
    {
        Time.timeScale = 0f;
        string activeThemeSong = GameManager.Instance.GetCurrentTheme();

        if (AudioManager.Instance.IsSongPlaying(activeThemeSong))
            AudioManager.Instance.Stop(activeThemeSong);
        if (instanceState == GameState.Victory)
        {
            victoryLossMsg.SetText("You won, great job!");
            AudioManager.Instance.Play(AudioManager.VictoryFX);
        }
        else if (instanceState == GameState.Lose)
        {
            victoryLossMsg.SetText("Want to try again?");
            AudioManager.Instance.Play(AudioManager.LoseFX);
        }

        playAgainButton.interactable = false;
        exitButton.interactable = false;
        StartCoroutine(FadeInButtonsAndActivate(buttonDelayTime));
    }

    public IEnumerator FadeInButtonsAndActivate(float duration)
    {
        while (alpha < 1.0f)
        {
            alpha += timeIncrement / duration;
            playAgainText.color = new Color(victoryLossMsg.color.r, victoryLossMsg.color.g, victoryLossMsg.color.b, alpha);
            exitButtonText.color = new Color(victoryLossMsg.color.r, victoryLossMsg.color.g, victoryLossMsg.color.b, alpha);
            yield return new WaitForSecondsRealtime(timeIncrement);
        }

        playAgainButton.interactable = true;
        exitButton.interactable = true;
    }

    public void PlayAgainButton() 
    { 
        GameManager.Instance.PlayAgain(); 
    }
    
    public void ReturnToMainMenuButton() 
    { 
        GameManager.Instance.ResetGame(); 
    }
}
