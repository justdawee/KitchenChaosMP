using System;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace Counters
{
    public class CuttingCounter : BaseCounter, IHasProgress
    {
        public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
        public event EventHandler OnCutting;
        public static event EventHandler OnAnyCut;

        public new static void ResetStaticData()
        {
            OnAnyCut = null;
        }

        [SerializeField] private CuttingRecipeSO[] cuttingRecipeSoArray;

        private int _cuttingProgress;

        public override void Interact(Player player)
        {
            if (!HasKitchenObject()) // Counter is empty?
            {
                if (player.HasKitchenObject()) // Player is holding a kitchen object?
                {
                    if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSo())) // Counter has a recipe for the input?
                    {
                        KitchenObject kitchenObject = player.GetKitchenObject(); // Get the kitchen object from the player
                        kitchenObject.SetKitchenObjectParent(this); // Give the kitchen object to the counter
                        InteractLogicPlaceObjectOnCounterServerRpc();
                    }
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
                            KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        }
                    }
                }
                else
                {
                    GetKitchenObject().SetKitchenObjectParent(player); // Give the kitchen object to the player
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractLogicPlaceObjectOnCounterServerRpc()
        {
            InteractLogicPlaceObjectOnCounterClientRpc();
        }
        
        [ClientRpc]
        private void InteractLogicPlaceObjectOnCounterClientRpc()
        {
            _cuttingProgress = 0; // Reset the cutting progress
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f // Reset the progress
            });
        }

        public override void InteractAlternate(Player player)
        {
            if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSo())) // Counter has a kitchen object and it can be cut?
            {
                CutObjectServerRpc();
                TestCuttingProgressDoneServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void CutObjectServerRpc()
        {
            CutObjectClientRpc();
        }
        
        [ClientRpc]
        private void CutObjectClientRpc()
        {
            _cuttingProgress++; // Increase the cutting progress
            
            OnCutting?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)_cuttingProgress / cuttingRecipeSo.cuttingProgressMax // Update the progress
            });
        }

        [ServerRpc(RequireOwnership = false)]
        private void TestCuttingProgressDoneServerRpc()
        {
            CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo());
            
            if (_cuttingProgress >= cuttingRecipeSo.cuttingProgressMax) // Cutting is done?
            {
                KitchenObjectSO outputKitchenObjectSo = GetOutputForInput(GetKitchenObject().GetKitchenObjectSo()); // Get the output kitchen object for the input kitchen object
                KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy the current kitchen object
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSo, this); // Spawn the cutted kitchen object
            }
        }

        private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSo)
        {
            CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
            return cuttingRecipeSo != null; // Return if the recipe exists
        }

        private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSo)
        {
            CuttingRecipeSO
                cuttingRecipeSo = GetCuttingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
            if (cuttingRecipeSo != null)
            {
                return cuttingRecipeSo.output; // Return the output
            }

            return null; // No output
        }

        private CuttingRecipeSO GetCuttingRecipeSoWithInput(KitchenObjectSO inputKitchenObjectSo)
        {
            foreach (CuttingRecipeSO cuttingRecipeSo in cuttingRecipeSoArray)
            {
                if (cuttingRecipeSo.input == inputKitchenObjectSo) // Found the recipe for the input
                {
                    return cuttingRecipeSo; // Return the recipe
                }
            }

            return null; // No recipe
        }
    }
}