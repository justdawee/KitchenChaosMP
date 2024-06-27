using System;
using UnityEngine;

namespace Counters
{
    public class BaseCounter : MonoBehaviour, IKitchenObjectParent
    {
        public static event EventHandler OnAnyObjectPlaced;
        
        public static void ResetStaticData()
        {
            OnAnyObjectPlaced = null;
        }
        
        [SerializeField] private Transform counterTopPoint;
        private KitchenObject _kitchenObject;
    
        public virtual void Interact(Player player)
        {
            Debug.LogError("ERROR: Interact method not implemented!");
        }
    
        public virtual void InteractAlternate(Player player)
        {
            // Debug.LogError("ERROR: InteractAlternate method not implemented!");
        }
    
        public Transform GetKitchenObjectFollowTransform() => counterTopPoint;
    
        public void SetKitchenObject(KitchenObject kitchenObject)
        {
            _kitchenObject = kitchenObject;
            OnAnyObjectPlaced?.Invoke(this, EventArgs.Empty);
        }

        public KitchenObject GetKitchenObject() => _kitchenObject;

        public void ClearKitchenObject() => _kitchenObject = null;
    
        public bool HasKitchenObject() => _kitchenObject != null;
    }
}
