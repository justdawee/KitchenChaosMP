using System;
using ScriptableObjects;
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
                OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}