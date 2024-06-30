using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using QFSW.QC;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    
    public event EventHandler<StateChangedEventArgs> OnStateChanged;
    public class StateChangedEventArgs : EventArgs
    {
        public State State;
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

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> _fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> _burningTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO _fryingRecipeSo;
    private BurningRecipeSO _burningRecipeSo;
    
    public override void OnNetworkSpawn()
    {
        _fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        _burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        _state.OnValueChanged += State_OnValueChanged;
    }
    
    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new StateChangedEventArgs {State = _state.Value});
    
        if (_state.Value == State.Burned || _state.Value == State.Idle)
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
            switch (_state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    _fryingTimer.Value += Time.deltaTime; // Increase the frying timer
                
                    if (_fryingTimer.Value > _fryingRecipeSo.fryingTimeMax) // Frying is done?
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy the input
                        KitchenObject.SpawnKitchenObject(_fryingRecipeSo.output, this); // Spawn the output

                        SetStateServerRpc(State.Fried); // Change the state
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
                        SetStateServerRpc(State.Burned); // Change the state
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
                        SetStateServerRpc(State.Idle);
                    }
                }
            }
            else
            {
                // Player is picking up the kitchen object
                GetKitchenObject().SetKitchenObjectParent(player); // Give the kitchen object to the player
                SetStateServerRpc(State.Idle);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateServerRpc(State newState)
    {
        _state.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSoIndex)
    {
        _fryingTimer.Value = 0f; // Reset the frying timer
        _burningTimer.Value = 0f; // Reset the burning timer
        
        SetStateServerRpc(State.Frying);
        SetFryingRecipeSoClientRpc(kitchenObjectSoIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSoClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectSoIndex);
        _fryingRecipeSo = GetFryingRecipeSoWithInput(kitchenObjectSo);
    }
    
    [ClientRpc]
    private void SetBurningRecipeSoClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectSoIndex);
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

    public bool IsFried() => _state.Value == State.Fried;
}