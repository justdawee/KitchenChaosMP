using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject _plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> _ingredientVisualList;

    [Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    private void Start()
    {
        _plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
        foreach (KitchenObjectSO_GameObject kitchenObjectSoGameObject in _ingredientVisualList)
        {
            kitchenObjectSoGameObject.gameObject.SetActive(false); // Hide all the ingredient visuals
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (KitchenObjectSO_GameObject kitchenObjectSoGameObject in _ingredientVisualList)
        {
            if (kitchenObjectSoGameObject.kitchenObjectSO == e.kitchenObjectSo)
            {
                kitchenObjectSoGameObject.gameObject.SetActive(true); // Show the ingredient visual
            }
        }
    }
}
