using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        GameManager.Instance.UpdateGameState(GameState.Playing);
    }

    public void OpenOptionsMenu() 
    { 
        GameManager.Instance.UpdateGameState(GameState.PausedOptionsMenu); 
    }

    public void QuitLevelOption() 
    { 
        GameManager.Instance.ResetGame();
    }
}
