using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using UnityEngine;
using UnityEngine.UI;

public class ProgressbarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private Image barImage;
    
    private IHasProgress _hasProgress;
    private void Start()
    {
        _hasProgress = hasProgressGameObject.GetComponent<IHasProgress>(); // Get the IHasProgress component from the hasProgressGameObject
        
        if (_hasProgress == null)
        {
            Debug.LogError("ProgressbarUI: Start: hasProgressGameObject does not have a component that implements IHasProgress");
        }
        else
        {
            _hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
            barImage.fillAmount = 0f;
            HideProgressbar();
        }
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if(e.progressNormalized == 0f || e.progressNormalized == 1f)
        {
            HideProgressbar();
        }
        else
        {
            ShowProgressbar();
        }
    }

    private void ShowProgressbar()
    {
        gameObject.SetActive(true);
    }
    
    private void HideProgressbar()
    {
        gameObject.SetActive(false);
    }
}
