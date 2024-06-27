using System;
using ScriptableObjects;
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
                        player.GetKitchenObject().SetKitchenObjectParent(this); // Give the kitchen object to the counter
                        _cuttingProgress = 0; // Reset the cutting progress
                        CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo()); // Get the recipe for the input
                    
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = (float)_cuttingProgress / cuttingRecipeSo.cuttingProgressMax // Update the progress
                        });
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
                            GetKitchenObject().DestroySelf();
                        }
                    }
                }
                else
                {
                    GetKitchenObject().SetKitchenObjectParent(player); // Give the kitchen object to the player
                }
            }
        }

        public override void InteractAlternate(Player player)
        {
            if (HasKitchenObject() &&
                HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSo())) // Counter has a kitchen object and it can be cut?
            {
                _cuttingProgress++; // Increase the cutting progress
            
                OnCutting?.Invoke(this, EventArgs.Empty);
                OnAnyCut?.Invoke(this, EventArgs.Empty);

                CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo());

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = (float)_cuttingProgress / cuttingRecipeSo.cuttingProgressMax // Update the progress
                });

                if (_cuttingProgress >= cuttingRecipeSo.cuttingProgressMax) // Cutting is done?
                {
                    KitchenObjectSO outputKitchenObjectSo = GetOutputForInput(GetKitchenObject().GetKitchenObjectSo()); // Get the output kitchen object for the input kitchen object
                    GetKitchenObject().DestroySelf(); // Destroy the current kitchen object

                    KitchenObject.SpawnKitchenObject(outputKitchenObjectSo, this); // Spawn the cutted kitchen object
                }
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