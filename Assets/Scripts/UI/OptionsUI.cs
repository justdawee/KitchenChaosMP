using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button sfxButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI musicText;

    // Bindings
    [SerializeField] private Transform pressToRebindKeyTransform;
    [SerializeField] private Button resetBindingsButton;

    private Action _onCloseButtonAction;

    // Controls input references
    // Keyboard
    [SerializeField] private Button moveUpKeyboardButton;
    [SerializeField] private TextMeshProUGUI moveUpButtonKeyboardText;
    [SerializeField] private Button moveDownKeyboardButton;
    [SerializeField] private TextMeshProUGUI moveDownButtonKeyboardText;
    [SerializeField] private Button moveLeftKeyboardButton;
    [SerializeField] private TextMeshProUGUI moveLeftButtonKeyboardText;
    [SerializeField] private Button moveRightKeyboardButton;
    [SerializeField] private TextMeshProUGUI moveRightButtonKeyboardText;
    [SerializeField] private Button interactKeyboardButton;
    [SerializeField] private TextMeshProUGUI interactButtonKeyboardText;
    [SerializeField] private Button interactAltKeyboardButton;
    [SerializeField] private TextMeshProUGUI interactAltButtonKeyboardText;
    [SerializeField] private Button pauseKeyboardButton;
    [SerializeField] private TextMeshProUGUI pauseButtonKeyboardText;

    // Gamepad
    [SerializeField] private Button moveUpGamepadButton;
    [SerializeField] private TextMeshProUGUI moveUpButtonGamepadText;
    [SerializeField] private Button moveDownGamepadButton;
    [SerializeField] private TextMeshProUGUI moveDownButtonGamepadText;
    [SerializeField] private Button moveLeftGamepadButton;
    [SerializeField] private TextMeshProUGUI moveLeftButtonGamepadText;
    [SerializeField] private Button moveRightGamepadButton;
    [SerializeField] private TextMeshProUGUI moveRightButtonGamepadText;
    [SerializeField] private Button interactGamepadButton;
    [SerializeField] private TextMeshProUGUI interactButtonGamepadText;
    [SerializeField] private Button interactAltGamepadButton;
    [SerializeField] private TextMeshProUGUI interactAltButtonGamepadText;
    [SerializeField] private Button pauseGamepadButton;
    [SerializeField] private TextMeshProUGUI pauseButtonGamepadText;

    private bool _isRebinding = false;

    private void Awake()
    {
        Instance = this;

        sfxButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume(); // Increment the sfx volume by 0.1f
            UpdateText();
            UpdateSliders();
        });

        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume(); // Increment the music volume by 0.1f
            UpdateText();
            UpdateSliders();
        });

        backButton.onClick.AddListener(() =>
        {
            Hide();
            _onCloseButtonAction();
        });

        sfxSlider.onValueChanged.AddListener((value) =>
        {
            SoundManager.Instance.SetVolume(value);
            UpdateText();
        });

        musicSlider.onValueChanged.AddListener((value) =>
        {
            MusicManager.Instance.SetVolume(value);
            UpdateText();
        });

        moveUpKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.MoveUp));
        moveDownKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.MoveDown));
        moveLeftKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.MoveLeft));
        moveRightKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.MoveRight));
        interactKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.Interact));
        interactAltKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.InteractAlternate));
        //pauseKeyboardButton.onClick.AddListener(() => RebindKeyboardBinding(GameInput.Binding.Pause));

        moveUpGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.MoveUp));
        moveDownGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.MoveDown));
        moveLeftGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.MoveLeft));
        moveRightGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.MoveRight));
        interactGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.Interact));
        interactAltGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.InteractAlternate));
        //pauseGamepadButton.onClick.AddListener(() => RebindControllerBinding(GameInput.Binding.Pause));

        resetBindingsButton.onClick.AddListener(() =>
        {
            GameInput.Instance.RestoreDefaultBindings();
            UpdateText();
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        UpdateText();
        UpdateSliders();
        Hide();
    }

    private void Update()
    {
        if (_isRebinding && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button8))) // Escape key or Start button on gamepad
        {
            CancelRebinding();
        }
    }

    private void KitchenGameManager_OnGamePaused(object sender, EventArgs e)
    {
        Hide();
    }

    public void Show(Action onCloseButtonAction)
    {
        _onCloseButtonAction = onCloseButtonAction;
        gameObject.SetActive(true);
        sfxButton.Select();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateText()
    {
        sfxText.text = "SOUND EFFECTS: " + Mathf.Round(SoundManager.Instance.Volume * 10f);
        musicText.text = "MUSIC: " + Mathf.Round(MusicManager.Instance.Volume * 10f);

        moveUpButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp, GameInput.CONTROL_SCHEME_KEYBOARD);
        moveDownButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown, GameInput.CONTROL_SCHEME_KEYBOARD);
        moveLeftButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft, GameInput.CONTROL_SCHEME_KEYBOARD);
        moveRightButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight, GameInput.CONTROL_SCHEME_KEYBOARD);
        interactButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact, GameInput.CONTROL_SCHEME_KEYBOARD);
        interactAltButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate, GameInput.CONTROL_SCHEME_KEYBOARD);
        pauseButtonKeyboardText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause, GameInput.CONTROL_SCHEME_KEYBOARD);

        moveUpButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp, GameInput.CONTROL_SCHEME_GAMEPAD);
        moveDownButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown, GameInput.CONTROL_SCHEME_GAMEPAD);
        moveLeftButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft, GameInput.CONTROL_SCHEME_GAMEPAD);
        moveRightButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight, GameInput.CONTROL_SCHEME_GAMEPAD);
        interactButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact, GameInput.CONTROL_SCHEME_GAMEPAD);
        interactAltButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate, GameInput.CONTROL_SCHEME_GAMEPAD);
        pauseButtonGamepadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause, GameInput.CONTROL_SCHEME_GAMEPAD);
    }

    private void UpdateSliders()
    {
        sfxSlider.value = SoundManager.Instance.Volume;
        musicSlider.value = MusicManager.Instance.Volume;
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindKeyboardBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindKeyboardKey(binding, () =>
        {
            HidePressToRebindKey();
            UpdateText();
        });
    }

    private void RebindControllerBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindControllerKey(binding, () =>
        {
            HidePressToRebindKey();
            UpdateText();
        });
    }
    
    private void CancelRebinding()
    {
        HidePressToRebindKey();
        _isRebinding = false;
        // Additional logic for handling the cancellation if needed
    }
}