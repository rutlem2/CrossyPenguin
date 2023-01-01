using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void OptionsBackButton()
    {
        if (GameManager.Instance.GetState() == GameState.MainOptionsMenu)
            GameManager.Instance.UpdateGameState(GameState.MainMenu);
        else
            GameManager.Instance.UpdateGameState(GameState.Paused);
    }

    public void AdjustVolume(float volume)
    {
        AudioManager.Instance.Play(AudioManager.SliderFX);
        audioMixer.SetFloat("Volume", volume);
    }
}
