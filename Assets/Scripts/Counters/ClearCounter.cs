using ScriptableObjects;
using UnityEngine;

namespace Counters
{
    public class ClearCounter : BaseCounter
    {
        [SerializeField] private KitchenObjectSO kitchenObjectSo;

        public override void Interact(Player player)
        {
            if (!HasKitchenObject()) // Counter is empty?
            {
                if (player.HasKitchenObject()) // Player is holding a kitchen object?
                {
                    player.GetKitchenObject()
                        .SetKitchenObjectParent(this); // Give the kitchen object to the counter
                }
            }
            else
            {
                if (player.HasKitchenObject()) // Player has a kitchen object?
                {
                    // Player is carrying something
                    if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                    {
                        // Player is holding a plate
                        if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSo()))
                        {
                            // Add the ingredient to the plate
                            GetKitchenObject().DestroySelf();
                        }
                    }
                    else
                    {
                        // Player is not holding a plate
                        if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                        {
                            // Counter has a plate
                            if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSo()))
                            {
                                // Add the ingredient to the plate
                                player.GetKitchenObject().DestroySelf();
                            }
                        }
                    }
                }
                else
                {
                    GetKitchenObject()
                        .SetKitchenObjectParent(player); // Give the kitchen object to the player
                }
            }
        }
    }
}