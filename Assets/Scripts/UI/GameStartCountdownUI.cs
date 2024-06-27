using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPopup";
    
    [SerializeField] private TextMeshProUGUI countdownText;

    private Animator _animator;
    private int _previousNumber;
    private static readonly int NumberPopup = Animator.StringToHash(NUMBER_POPUP);

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        HideCountdown();
    }

    private void Update()
    {
        int countdownNumber = Mathf.CeilToInt(KitchenGameManager.Instance.GetCountdownTime());
        countdownText.text = countdownNumber.ToString();

        if (_previousNumber != countdownNumber)
        {
            _previousNumber = countdownNumber;
            _animator.SetTrigger(NumberPopup);
            SoundManager.Instance.PlayCountdownSound();
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownActive())
        {
            ShowCountdown();
        }
        else
        {
            HideCountdown();
        }
    }
    
    private void ShowCountdown()
    {
        gameObject.SetActive(true);
    }
    
    private void HideCountdown()
    {
        gameObject.SetActive(false);
    }
}
