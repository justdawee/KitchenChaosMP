using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSo;
    public KitchenObjectSO GetKitchenObjectSo() => kitchenObjectSo;
    private IKitchenObjectParent _kitchenObjectParent;
    public IKitchenObjectParent GetKitchenObjectParent() => _kitchenObjectParent;
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
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
        
        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        transform.position = kitchenObjectParent.GetKitchenObjectFollowTransform().position;
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
    
    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSo, IKitchenObjectParent kitchenObjectParent)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSo.prefab);
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        return kitchenObject;
    }
}
