using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float SFXVolume = 1.0f;
    [Range(0f, 1f)]
    public float MusicVolume = 1.0f;

    [Header("Control Settings")]
    public bool VibrationEnabled = true;

    public static GameSettings DefaultSettings
    {
        get
        {
            var settings = CreateInstance<GameSettings>();
            settings.SFXVolume = 1.0f;
            settings.MusicVolume = 1.0f;
            settings.VibrationEnabled = true;
            return settings;
        }
    }
}