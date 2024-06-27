using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player _player;
    private float _footstepTimer;
    private readonly float _footstepInterval = .1f;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0)
        {
            _footstepTimer = _footstepInterval;

            if (_player.IsWalking())
            {
                float volume = 1f;
                SoundManager.Instance.PlayFootstepSound(_player.transform.position, volume);
            }
        }
    }
}
