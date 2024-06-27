using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using ScriptableObjects;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSoArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSoArray;

    private State state;
    private float _fryingTimer;
    private float _burningTimer;
    private FryingRecipeSO _fryingRecipeSo;
    private BurningRecipeSO _burningRecipeSo;


    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    _fryingTimer += Time.deltaTime; // Increase the frying timer
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = _fryingTimer / _fryingRecipeSo.fryingTimeMax });
                    if (_fryingTimer > _fryingRecipeSo.fryingTimeMax) // Frying is done?
                    {
                        GetKitchenObject().DestroySelf(); // Destroy the input
                        KitchenObject.SpawnKitchenObject(_fryingRecipeSo.output, this); // Spawn the output

                        state = State.Fried; // Change the state
                        _burningTimer = 0f; // Reset the burning timer
                        _burningRecipeSo = GetBurningRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo());
                        
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state});
                    }
                    break;
                case State.Fried:
                    _burningTimer += Time.deltaTime; // Increase the frying timer
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = _burningTimer / _burningRecipeSo.burningTimeMax });
                    if (_burningTimer > _burningRecipeSo.burningTimeMax) // Frying is done?
                    {
                        GetKitchenObject().DestroySelf(); // Destroy the input
                        KitchenObject.SpawnKitchenObject(_burningRecipeSo.output, this); // Spawn the output
                        state = State.Burned; // Change the state
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state});
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject()) // Counter is empty?
        {
            if (player.HasKitchenObject()) // Player is holding a kitchen object?
            {
                if (HasRecipeWithInput(player.GetKitchenObject()
                        .GetKitchenObjectSo())) // Counter has a recipe for the input?
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this); // Give the kitchen object to the counter
                    _fryingRecipeSo = GetFryingRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSo());
                    state = State.Frying;
                    _fryingTimer = 0f; // Reset the frying timer
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state});
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = _fryingTimer / _fryingRecipeSo.fryingTimeMax });
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
                        
                        state = State.Idle; // Reset the state to Idle
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state});
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
                    }
                }
            }
            else
            {
                // Player is picking up the kitchen object
                GetKitchenObject().SetKitchenObjectParent(player); // Give the kitchen object to the player
                
                state = State.Idle; // Reset the state to Idle
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state});
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSo)
    {
        FryingRecipeSO
            fryingRecipeSo = GetFryingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
        return fryingRecipeSo != null; // Return if the recipe exists
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSo)
    {
        FryingRecipeSO
            fryingRecipeSo = GetFryingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
        if (fryingRecipeSo != null)
        {
            return fryingRecipeSo.output; // Return the output
        }

        return null; // No output
    }

    private FryingRecipeSO GetFryingRecipeSoWithInput(KitchenObjectSO inputKitchenObjectSo)
    {
        foreach (FryingRecipeSO fryingRecipeSo in fryingRecipeSoArray)
        {
            if (fryingRecipeSo.input == inputKitchenObjectSo) // Found the recipe for the input
            {
                return fryingRecipeSo; // Return the recipe
            }
        }

        return null; // No recipe
    }
    
    private BurningRecipeSO GetBurningRecipeSoWithInput(KitchenObjectSO inputKitchenObjectSo)
    {
        foreach (BurningRecipeSO burningRecipeSo in burningRecipeSoArray)
        {
            if (burningRecipeSo.input == inputKitchenObjectSo) // Found the recipe for the input
            {
                return burningRecipeSo; // Return the recipe
            }
        }

        return null; // No recipe
    }

    public bool IsFried() => state == State.Fried;
}