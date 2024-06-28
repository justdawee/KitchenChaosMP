using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Counters
{
    public class TrashCounter : BaseCounter
    {
        public static event EventHandler OnAnyObjectTrashed;
        
        public new static void ResetStaticData()
        {
            OnAnyObjectTrashed = null;
        }
        
        public override void Interact(Player player)
        {
            if (player.HasKitchenObject())
            {
                KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                InteractLogicServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractLogicServerRpc()
        {
            InteractLogicClientRpc();
        }
        
        [ClientRpc]
        private void InteractLogicClientRpc()
        {
            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }
}
