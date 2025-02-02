using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{   
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            KitchenGameManager.Instance.PauseGame();
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        
        optionsButton.onClick.AddListener(() =>
        {
            Hide();
            OptionsUI.Instance.Show(Show);
        });
        
        Time.timeScale = 1f; // This is to ensure that the game is not paused when the scene is loaded
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePaused += KitchenGameManagerOnLocalGamePaused;
        KitchenGameManager.Instance.OnGameResumed += KitchenGameManager_OnGameResumed;
        
        Hide();
    }

    private void KitchenGameManager_OnGameResumed(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManagerOnLocalGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        resumeButton.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
