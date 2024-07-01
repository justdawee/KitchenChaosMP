using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }
    
    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnGameResumed;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    
    private enum State
    {
        WaitingToStart,
        Countdown,
        GamePlaying,
        GameOver
    }

    private NetworkVariable<State> _state = new(State.WaitingToStart);
    private bool _isLocalPlayerReady;
    private float _waitingTimer;
    private NetworkVariable<float> _countdownToStartTimer = new(3f);
    private NetworkVariable<float> _gamePlayingTimer = new(0f) ;
    private readonly float _gamePlayingTimerMax = 90f;
    private NetworkVariable<bool> _isGamePaused = new(false);
    private bool _isLocalPaused = false;
    private Dictionary<ulong, bool> _playerReadyDictionary;
    private Dictionary<ulong, bool> _playerPausedDictionary;

    private void Awake()
    {
        Instance = this;
        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += State_OnValueChanged;
        _isGamePaused.OnValueChanged += IsLocalPaused_OnValueChanged;
    }

    private void IsLocalPaused_OnValueChanged(bool previousvalue, bool newvalue)
    {
        if (_isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state.Value == State.WaitingToStart)
        {
            _isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        _playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;
        bool allPlayersReady = true;
        
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                allPlayersReady = false;
                break;
            }
        }

        if (allPlayersReady)
        {
            _state.Value = State.Countdown;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        PauseGame();
    }

    private void Update()
    {
        if (!IsServer) return;
        
        switch (_state.Value)
        { 
            case State.WaitingToStart:
                break;
            case State.Countdown:
                _countdownToStartTimer.Value -= Time.deltaTime;
                if(_countdownToStartTimer.Value <= 0f)
                {
                    _state.Value = State.GamePlaying;
                    _gamePlayingTimer.Value = _gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                _gamePlayingTimer.Value -= Time.deltaTime;
                if(_gamePlayingTimer.Value <= 0f)
                {
                    _state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }
    }
    
    public bool IsGamePlaying()
    {
        return _state.Value == State.GamePlaying;
    }

    public bool IsCountdownActive()
    {
        return _state.Value == State.Countdown;
    }
    
    public float GetCountdownTime()
    {
        return _countdownToStartTimer.Value;
    }
    
    public bool IsGameOver()
    {
        return _state.Value == State.GameOver;
    }
    
    public float GetGamePlayingTimeNormalized()
    {
        return 1 - (_gamePlayingTimer.Value / _gamePlayingTimerMax);
    }

    public void PauseGame()
    {
        _isLocalPaused = !_isLocalPaused;
        
        if (_isLocalPaused)
        {
            // show pause menu
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // hide pause menu
            UnpauseGameServerRpc();
            OnGameResumed?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams rpcParams = default)
    {
        _playerPausedDictionary[rpcParams.Receive.SenderClientId] = true;
        TestGamePausedState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams rpcParams = default)
    {
        _playerPausedDictionary[rpcParams.Receive.SenderClientId] = false;
        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (_playerPausedDictionary.ContainsKey(clientId) && _playerPausedDictionary[clientId])
            {
                // player is paused
                _isGamePaused.Value = true;
                return;
            }
        }
        // all players unpaused
        _isGamePaused.Value = false;
    }

    public bool IsLocalPlayerReady() => _isLocalPlayerReady;
}
