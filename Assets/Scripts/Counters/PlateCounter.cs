    using System;
using ScriptableObjects;
using Unity.Netcode;
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
            if (!IsServer) return;

            _spawnPlateTimer += Time.deltaTime;
            if (_spawnPlateTimer > _spawnPlateTimerMax)
            {
                _spawnPlateTimer = 0f;
                if(KitchenGameManager.Instance.IsGamePlaying() && _platesAmount < _platesAmountMax)
                {
                    SpawnPlateServerRpc();
                }
            }
        }

        [ServerRpc]
        private void SpawnPlateServerRpc()
        {
            SpawnPlateClientRpc();
        }

        [ClientRpc]
        private void SpawnPlateClientRpc()
        {
            _platesAmount++;
            OnPlateSpawned?.Invoke(this, EventArgs.Empty);
        }
        
        public override void Interact(Player player)
        {
            if (!player.HasKitchenObject())
            {
                // Player has no items in hand
                if (_platesAmount > 0)
                {
                    KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                    InteractLogicServerRpc();
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractLogicServerRpc()
        {
            InteractLogicClientRpc(); // Call the client RPC to handle the interaction logic
        }

        [ClientRpc]
        private void InteractLogicClientRpc() // Handle the interaction logic
        {
            _platesAmount--;
            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
