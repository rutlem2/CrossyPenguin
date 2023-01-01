using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public const string EasyTheme = "EasyTheme";
    public const string MediumTheme = "MediumTheme";
    public const string HardTheme = "HardTheme";
    public const string ExpertTheme = "ExpertTheme";
    public const string PengJump = "PengJump";
    public const string VictoryFX = "VictoryFX";
    public const string LoseFX = "LoseFX";
    public const string TruckHorn = "TruckHorn";
    public const string CarHorn = "CarHorn";
    public const string CarHornHighPitch = "CarHorn-HighPitch";
    public const string PenguinStartClick = "PenguinStartClick";
    public const string PenguinStopClick = "PenguinStopClick";
    public const string MenuSelection = "MenuSelection";
    public const string ExitFX = "FXExit";
    public const string CarHit = "CarHit";
    public const string SliderFX = "FXSlider";
    public const string VehiclePassBy = "VehiclePassBy";

    [SerializeField] Sound[] sounds;
    public static AudioManager Instance;

    void Awake() 
    {
        Instance = this;
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.outputAudioMixerGroup = s.mixer;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Pause();
    }

    public void Resume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.UnPause();
    }

    public bool IsSongPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.isPlaying;
    }
}
