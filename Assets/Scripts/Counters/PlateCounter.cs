using System;
using ScriptableObjects;
using UnityEngine;

namespace Counters
{
    public class PlatesCounter : BaseCounter
    {
        public event EventHandler OnPlateSpawned;
        public event EventHandler OnPlateRemoved;
        
        [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
        
        private float _spawnPlateTimer;
        private readonly float _spawnPlateTimerMax = 4f;
        private int _platesAmount;
        private readonly int _platesAmountMax = 4;

        private void Update()
        {
            _spawnPlateTimer += Time.deltaTime;
            if (_spawnPlateTimer > _spawnPlateTimerMax)
            {
                _spawnPlateTimer = 0f;
                if(KitchenGameManager.Instance.IsGamePlaying() && _platesAmount < _platesAmountMax)
                {
                    _platesAmount++;
                    OnPlateSpawned?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public override void Interact(Player player)
        {
            if (!player.HasKitchenObject())
            {
                // Player has no items in hand
                if (_platesAmount > 0)
                {
                    _platesAmount--;
                    KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                    OnPlateRemoved?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
