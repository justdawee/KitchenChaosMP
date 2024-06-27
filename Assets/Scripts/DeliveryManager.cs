using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }
    
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeDelivered;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    
    [FormerlySerializedAs("recipeListSO")] [SerializeField] private RecipeListSO recipeListSo;
    
    private List<RecipeSO> _waitingRecipeSoList;
    private float _spawnRecipeTimer;
    private readonly float _spawnRecipeTimerMax = 4f;
    private readonly int _waitingRecipesMax = 4;
    private int _successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;
        _waitingRecipeSoList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        _spawnRecipeTimer -= Time.deltaTime;
        if (_spawnRecipeTimer <= 0f)
        {
            _spawnRecipeTimer = _spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && _waitingRecipeSoList.Count < _waitingRecipesMax)
            {
                int waitingRecipeSoIndex = Random.Range(0, recipeListSo.recipeSOList.Count);
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSoIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSoIndex)
    {
        RecipeSO waitingRecipeSo = recipeListSo.recipeSOList[waitingRecipeSoIndex];
        _waitingRecipeSoList.Add(waitingRecipeSo);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < _waitingRecipeSoList.Count; i++)
        {
            RecipeSO waitingRecipeSo = _waitingRecipeSoList[i];
            if (waitingRecipeSo.KitchenObjectSoList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateHasAllIngredients = true;
                // Check if the plate has the same kitchen objects as the waiting recipe
                foreach (KitchenObjectSO recipeKitchenObjectSo in waitingRecipeSo.KitchenObjectSoList)
                {
                    bool ingredientFound = false;
                    // Cycle through the kitchen objects in the recipe
                    foreach (KitchenObjectSO plateKitchenObjectSo in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        // Cycle through the kitchen objects in the plate
                        if (recipeKitchenObjectSo == plateKitchenObjectSo)
                        {
                            // If the kitchen object in the waiting recipe is the same as the kitchen object in the plate
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        // If the kitchen object in the waiting recipe is not found in the plate
                        plateHasAllIngredients = false;
                    }
                }
                if (plateHasAllIngredients)
                {
                    // Player delivered the correct recipe
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        // No matches found
        // Player not delivered the correct recipe
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSoListIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSoListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSoListIndex)
    {
        _successfulRecipesAmount++;
        _waitingRecipeSoList.RemoveAt(waitingRecipeSoListIndex);
        
        OnRecipeDelivered?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }
    
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSoList()
    {
        return _waitingRecipeSoList;
    }
    
    public int GetSuccessfulRecipesAmount()
    {
        return _successfulRecipesAmount;
    }
}
