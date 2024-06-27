using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }
    
    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResumed;
    
    private enum State
    {
        WaitingToStart,
        Countdown,
        GamePlaying,
        GameOver
    }

    private State _state;
    private float _waitingTimer;
    private float _countdownToStartTimer = 3f;
    private float _gamePlayingTimer;
    private float _gamePlayingTimerMax = 90f;
    private bool _isPaused;

    private void Awake()
    {
        Instance = this;
        _state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state == State.WaitingToStart)
        {
            _state = State.Countdown;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        PauseGame();
    }

    private void Update()
    {
        switch (_state)
        {
            case State.WaitingToStart:
                
                break;
            case State.Countdown:
                _countdownToStartTimer -= Time.deltaTime;
                if(_countdownToStartTimer <= 0f)
                {
                    _state = State.GamePlaying;
                    _gamePlayingTimer = _gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                _gamePlayingTimer -= Time.deltaTime;
                if(_gamePlayingTimer <= 0f)
                {
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                
                break;
        }
    }
    
    public bool IsGamePlaying()
    {
        return _state == State.GamePlaying;
    }

    public bool IsCountdownActive()
    {
        return _state == State.Countdown;
    }
    
    public float GetCountdownTime()
    {
        return _countdownToStartTimer;
    }
    
    public bool IsGameOver()
    {
        return _state == State.GameOver;
    }
    
    public float GetGamePlayingTimeNormalized()
    {
        return 1 - (_gamePlayingTimer / _gamePlayingTimerMax);
    }

    public void PauseGame()
    {
        _isPaused = !_isPaused; // toggle the pause state
        
        if(_isPaused)
        {
            // show pause menu
            Time.timeScale = 0f; // pause the game
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // hide pause menu
            Time.timeScale = 1f; // resume the game
            OnGameResumed?.Invoke(this, EventArgs.Empty);
        }
    }
}
