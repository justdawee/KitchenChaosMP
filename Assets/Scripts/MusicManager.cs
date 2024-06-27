using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";
    
    public static  MusicManager Instance { get; private set; }
    
    private AudioSource _audioSource;
    private float _volume = .3f;

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, 1f);
    }

    public void ChangeVolume()
    {
        _volume = Mathf.Round((_volume + 0.1f) * 10f) / 10f;
        
        if (_volume > 1f)
        {
            _volume = 0f;
        }
        _audioSource.volume = _volume;
        
        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, _volume);
        PlayerPrefs.Save();
    }
    
    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp(volume, 0f, 1f);
        _audioSource.volume = _volume;
        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, _volume);
        PlayerPrefs.Save();
    }

    
    public float Volume => _volume;
}
