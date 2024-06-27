using System;
using UnityEngine;

namespace Counters
{
    public class CuttingCounterVisual : MonoBehaviour
    {
        [SerializeField] private CuttingCounter cuttingCounter;
    
        private Animator _animator;
        private static readonly int Cut = Animator.StringToHash(CUT);
        private const string CUT = "Cut"; // "OpenClose" is a trigger in the Animator that we want to set to true when the player grabs an object from the counter

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    
        private void Start()
        {
            cuttingCounter.OnCutting += CuttingCounter_OnCutting;
        }

        private void CuttingCounter_OnCutting(object sender, EventArgs e)
        {
            _animator.SetTrigger(Cut);
        }
    }
}