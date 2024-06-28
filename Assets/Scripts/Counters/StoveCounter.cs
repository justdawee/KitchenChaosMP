using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using ScriptableObjects;
using Unity.Netcode;
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

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> _fryingTimer = new(0f);
    private NetworkVariable<float> _burningTimer =new(0f);
    private FryingRecipeSO _fryingRecipeSo;
    private BurningRecipeSO _burningRecipeSo;
    
    public override void OnNetworkSpawn()
    {
        _fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        _burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {state = state.Value});
        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
        }
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = _fryingRecipeSo != null ? _fryingRecipeSo.fryingTimeMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = _fryingTimer.Value / fryingTimerMax
        });
    }
    
    private void BurningTimer_OnValueChanged(float previousvalue, float newvalue)
    {
        float burningTimerMax = _burningRecipeSo != null ? _burningRecipeSo.burningTimeMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs()
        {
            progressNormalized = _burningTimer.Value / burningTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer) return;
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    _fryingTimer.Value += Time.deltaTime; // Increase the frying timer
                    
                    if (_fryingTimer.Value > _fryingRecipeSo.fryingTimeMax) // Frying is done?
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy the input
                        KitchenObject.SpawnKitchenObject(_fryingRecipeSo.output, this); // Spawn the output

                        state.Value = State.Fried; // Change the state
                        _burningTimer.Value = 0f; // Reset the burning timer
                        SetBurningRecipeSoClientRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSo()));
                    }
                    break;
                case State.Fried:
                    _burningTimer.Value += Time.deltaTime; // Increase the frying timer
                    
                    if (_burningTimer.Value > _burningRecipeSo.burningTimeMax) // Frying is done?
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy the input
                        KitchenObject.SpawnKitchenObject(_burningRecipeSo.output, this); // Spawn the output
                        state.Value = State.Burned; // Change the state
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
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSo())) // Counter has a recipe for the input?
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this); // Give the kitchen object to the counter
                    InteractLogicPlaceObjectOnCounterServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSo()));
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
                        
                        state.Value = State.Idle; // Reset the state to Idle
                    }
                }
            }
            else
            {
                // Player is picking up the kitchen object
                GetKitchenObject().SetKitchenObjectParent(player); // Give the kitchen object to the player
                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        _fryingTimer.Value = 0f; // Reset the frying timer
        state.Value = State.Frying;
        SetFryingRecipeSoClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSoClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectSOIndex);
        _fryingRecipeSo = GetFryingRecipeSoWithInput(kitchenObjectSo);
    }
    
    [ClientRpc]
    private void SetBurningRecipeSoClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectSOIndex);
        _burningRecipeSo = GetBurningRecipeSoWithInput(kitchenObjectSo);
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSo)
    {
        FryingRecipeSO fryingRecipeSo = GetFryingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
        return fryingRecipeSo != null; // Return if the recipe exists
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSo)
    {
        FryingRecipeSO fryingRecipeSo = GetFryingRecipeSoWithInput(inputKitchenObjectSo); // Get the recipe for the input
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

    public bool IsFried() => state.Value == State.Fried;
}