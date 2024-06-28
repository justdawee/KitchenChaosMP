using System;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace Counters
{
    public class ContainerCounter : BaseCounter
    {
        [SerializeField] private KitchenObjectSO kitchenObjectSo;

        public event EventHandler OnPlayerGrabbedObject;

        public override void Interact(Player player)
        {
            if (player.HasKitchenObject())
            {
                // Player is carrying something
            }
            else
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, player); // Give the kitchen object to the player
                InteractLogicServerRpc(); // Call the server RPC to handle the interaction logic
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
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }
}