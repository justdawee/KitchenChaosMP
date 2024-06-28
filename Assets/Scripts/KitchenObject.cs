using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSo;
    
    private FollowTransform _followTransform;
    private IKitchenObjectParent _kitchenObjectParent;
    
    protected virtual void Awake()
    {
        _followTransform = GetComponent<FollowTransform>();
    }
    
    public KitchenObjectSO GetKitchenObjectSo() => kitchenObjectSo;
    public IKitchenObjectParent GetKitchenObjectParent() => _kitchenObjectParent;

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference IKitchenObjectParentNetworkReference)
    {
        SetKitchenObjectParentClientRpc(IKitchenObjectParentNetworkReference);
    }
    
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference IKitchenObjectParentNetworkReference)
    {
        IKitchenObjectParentNetworkReference.TryGet(out NetworkObject IKitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = IKitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        
        if (_kitchenObjectParent != null)
        {
            _kitchenObjectParent.ClearKitchenObject();            
        }
        
        _kitchenObjectParent = kitchenObjectParent; // Set the kitchen object parent of the kitchen object to the new clear counter
        
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("ERROR: The IKitchenObjectParent already has a kitchen object!");
        }
        
        kitchenObjectParent.SetKitchenObject(this); // Set the kitchen object of the clear counter to this kitchen object
        
        _followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }
    
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }

    public void DestroySelf()
    {
        _kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }
    
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSo, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSo, kitchenObjectParent);
    }
}
