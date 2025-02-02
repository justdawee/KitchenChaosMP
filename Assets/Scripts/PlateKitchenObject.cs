using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSo;
    }
    
    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    
    private List<KitchenObjectSO> _kitchenObjectSOList;

    protected override void Awake()
    {
        base.Awake(); // Call the base class's Awake method
        _kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSo)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSo))
        {
            return false; // Invalid ingredient
        }
        if (_kitchenObjectSOList.Contains(kitchenObjectSo))
        {
            return false; // Already contains this ingredient
        }
        AddIngredientServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSo));
        return true; // Successfully added ingredient
    }   

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSoIndex)
    {
        AddIngredientClientRpc(kitchenObjectSoIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSO(kitchenObjectSoIndex);
        _kitchenObjectSOList.Add(kitchenObjectSo);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs {kitchenObjectSo = kitchenObjectSo});
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return _kitchenObjectSOList; // Return the list of kitchen object SOs
    }
}
