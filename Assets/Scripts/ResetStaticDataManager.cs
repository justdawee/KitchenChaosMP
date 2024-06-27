using System;
using System.Collections;
using System.Collections.Generic;
using Counters;
using UnityEngine;

public class ResetStaticDataManager : MonoBehaviour
{
    private void Awake()
    {
        BaseCounter.ResetStaticData(); // Reset the static data for the BaseCounter
        CuttingCounter.ResetStaticData(); // Reset the static data for the CuttingCounter
        TrashCounter.ResetStaticData(); // Reset the static data for the TrashCounter
    }
}
