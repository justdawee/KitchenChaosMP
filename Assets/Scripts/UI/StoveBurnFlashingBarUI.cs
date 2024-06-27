using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";
    
    [SerializeField] private StoveCounter stoveCounter;

    private Animator _animator;
    private static readonly int IsFlashing = Animator.StringToHash(IS_FLASHING);

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        _animator.SetBool(IsFlashing, false);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnThreshold = 0.5f;
        bool show = stoveCounter.IsFried() && e.progressNormalized >= burnThreshold;
        
        _animator.SetBool(IsFlashing, show);
    }
}
