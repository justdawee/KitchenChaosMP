using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    [SerializeField] private TextMeshProUGUI keyboardMoveUpText;
    [SerializeField] private TextMeshProUGUI keyboardMoveDownText;
    [SerializeField] private TextMeshProUGUI keyboardMoveLeftText;
    [SerializeField] private TextMeshProUGUI keyboardMoveRightText;
    [SerializeField] private TextMeshProUGUI keyboardInteractText;
    [SerializeField] private TextMeshProUGUI keyboardInteractAltText;
    [SerializeField] private TextMeshProUGUI keyboardPauseText;
    
    [SerializeField] private TextMeshProUGUI gamePadMoveText;
    [SerializeField] private TextMeshProUGUI gamePadInteractText;
    [SerializeField] private TextMeshProUGUI gamePadInteractAltText;
    [SerializeField] private TextMeshProUGUI gamePadPauseText;

    private void Start()
    {
        GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        UpdateVisual();
        Show();
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownActive())
        {
            Hide();
        }
    }

    private void GameInput_OnBindingRebind(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        keyboardMoveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardMoveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveDown, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardMoveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardMoveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardInteractAltText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate, GameInput.CONTROL_SCHEME_KEYBOARD);
        keyboardPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause, GameInput.CONTROL_SCHEME_KEYBOARD);

        gamePadMoveText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveUp, GameInput.CONTROL_SCHEME_GAMEPAD);
        gamePadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact, GameInput.CONTROL_SCHEME_GAMEPAD);
        gamePadInteractAltText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate, GameInput.CONTROL_SCHEME_GAMEPAD);
        gamePadPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause, GameInput.CONTROL_SCHEME_GAMEPAD);
    }
}
