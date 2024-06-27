using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> _plateVisuals;


    private void Awake()
    {
        _plateVisuals = new List<GameObject>();
    }

    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounterOnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounterOnPlateRemoved;
    }

    private void PlatesCounterOnPlateRemoved(object sender, EventArgs e)
    {
        GameObject plateGameObject = _plateVisuals[_plateVisuals.Count - 1];
        _plateVisuals.Remove(plateGameObject);
        Destroy(plateGameObject);
    }

    private void PlatesCounterOnPlateSpawned(object sender, EventArgs e)
    {
        Transform plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);

        float offsetY = .1f;
        plateVisualTransform.localPosition = new Vector3(0, offsetY * _plateVisuals.Count, 0);
        _plateVisuals.Add(plateVisualTransform.gameObject);
    }
}
