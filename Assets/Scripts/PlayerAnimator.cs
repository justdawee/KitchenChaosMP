using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private Player player;
    private static readonly int IsWalking = Animator.StringToHash(IS_WALKING);
    private const string IS_WALKING = "IsWalking"; // "IsWalking" is a parameter in the Animator that we want to set to true or false based on the player's movement

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.SetBool(IsWalking, player.IsWalking());
    }
}
