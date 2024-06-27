using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public const string PLAYER_PREFS_BINDINGS = "Bindings";
    public const string CONTROL_SCHEME_KEYBOARD = "Keyboard";
    public const string CONTROL_SCHEME_GAMEPAD = "Gamepad";

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Interact,
        InteractAlternate,
        Pause
    }

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    private PlayerInputActions _playerInputActions;
    private InputActionRebindingExtensions.RebindingOperation _currentRebindingOperation;

    private void Awake()
    {
        Instance = this;
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Interact.performed += InteractOnPerformed;
        _playerInputActions.Player.InteractAlternate.performed += InteractAlternateOnPerformed;
        _playerInputActions.Player.Pause.performed += PauseOnPerformed;

        LoadBindings();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        _playerInputActions.Player.Interact.performed -= InteractOnPerformed;
        _playerInputActions.Player.InteractAlternate.performed -= InteractAlternateOnPerformed;
        _playerInputActions.Player.Pause.performed -= PauseOnPerformed;
        _playerInputActions.Dispose();
    }

    private void PauseOnPerformed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternateOnPerformed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractOnPerformed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    public string GetBindingText(Binding binding, string controlScheme)
    {
        InputAction action = GetAction(binding);
        int bindingIndex = GetBindingIndex(binding, controlScheme);
        return action.bindings[bindingIndex].ToDisplayString();
    }

    public void RebindKeyboardKey(Binding binding, Action onComplete)
    {
        Rebind(binding, onComplete, CONTROL_SCHEME_KEYBOARD, excludeMouse: false);
    }

    public void RebindControllerKey(Binding binding, Action onComplete)
    {
        Rebind(binding, onComplete, CONTROL_SCHEME_GAMEPAD, excludeMouse: true);
    }

    private void Rebind(Binding binding, Action onComplete, string controlScheme, bool excludeMouse)
    {
        if (_currentRebindingOperation != null && !_currentRebindingOperation.completed)
        {
            _currentRebindingOperation.Cancel();
        }

        _playerInputActions.Player.Disable();
        var action = GetAction(binding);
        var rebindOperation = action.PerformInteractiveRebinding(GetBindingIndex(binding, controlScheme));

        if (excludeMouse)
        {
            rebindOperation = rebindOperation.WithControlsExcluding("Mouse");
        }

        _currentRebindingOperation = rebindOperation
            .OnComplete(operation =>
            {
                action.Enable();
                _currentRebindingOperation.Dispose();
                _currentRebindingOperation = null;
                _playerInputActions.Player.Enable();
                SaveBindings();
                onComplete?.Invoke();
                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }

    private InputAction GetAction(Binding binding)
    {
        switch (binding)
        {
            case Binding.MoveUp:
            case Binding.MoveDown:
            case Binding.MoveLeft:
            case Binding.MoveRight:
                return _playerInputActions.Player.Move;
            case Binding.Interact:
                return _playerInputActions.Player.Interact;
            case Binding.InteractAlternate:
                return _playerInputActions.Player.InteractAlternate;
            case Binding.Pause:
                return _playerInputActions.Player.Pause;
            default:
                throw new ArgumentOutOfRangeException(nameof(binding), binding, null);
        }
    }

    private int GetBindingIndex(Binding binding, string controlScheme)
    {
        // Adjust these indices based on your specific input action bindings configuration
        switch (binding)
        {
            case Binding.MoveUp:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 1 : 10;
            case Binding.MoveDown:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 2 : 10;
            case Binding.MoveLeft:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 3 : 10;
            case Binding.MoveRight:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 4 : 10;
            case Binding.Interact:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 0 : 1;
            case Binding.InteractAlternate:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 0 : 1;
            case Binding.Pause:
                return controlScheme == CONTROL_SCHEME_KEYBOARD ? 0 : 1;
            default:
                throw new ArgumentOutOfRangeException(nameof(binding), binding, null);
        }
    }

    private void SaveBindings()
    {
        var bindings = _playerInputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, bindings);
        PlayerPrefs.Save();
    }

    private void LoadBindings()
    {
        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            var bindings = PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS);
            _playerInputActions.LoadBindingOverridesFromJson(bindings);
        }
    }

    public void RestoreDefaultBindings()
    {
        _playerInputActions.RemoveAllBindingOverrides();
        SaveBindings();
    }
}