using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
    public void CreditsBackButton() 
    {
        GameManager.Instance.UpdateGameState(GameState.MainMenu); 
    }
}
