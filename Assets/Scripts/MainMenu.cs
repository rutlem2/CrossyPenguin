using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGoesToDifficultyMenu()
    {
        GameManager.Instance.UpdateGameState(GameState.DifficultyMenu);
    }

    public void OpenOptionsMenu() 
    { 
        GameManager.Instance.UpdateGameState(GameState.MainOptionsMenu); 
    }

    public void OpenCreditsMenu() 
    {
        GameManager.Instance.UpdateGameState(GameState.CreditsMenu); 
    }

    public void OpenHowToPlayMenu() 
    {
        GameManager.Instance.UpdateGameState(GameState.HowToPlayMenu); 
    }

    public void ExitGame()
    {
        AudioManager.Instance.Play(AudioManager.ExitFX);
        Application.Quit();
    }
}
