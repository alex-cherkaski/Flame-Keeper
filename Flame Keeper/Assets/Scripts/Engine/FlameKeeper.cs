using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class FlameKeeper : MonoBehaviour
{
    private FlameKeeper() {} // Ensures private constructor

    protected static FlameKeeper _instance;
    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("Trying to create multiple game manager singletons!");
            Destroy(this);
        }

        _instance = this;
        DontDestroyOnLoad(this);

        controllersList.Add(levelController);

        foreach (BaseController controller in controllersList)
        {
            controller.Initialize();
        }
    }

    /// <summary>
    /// Use this to get the universal game manager that is the parent manager to
    /// all game controllers.
    /// </summary>
    /// <returns> The benevolent game manager </returns>
    public static FlameKeeper Get()
    {
        return _instance;
    }

    private List<BaseController> controllersList = new List<BaseController>(); // List of all controllers so looping over all of them is fast

    /// <summary>
    /// Game controllers:
    /// Add controllers here as needed, and avoid adding functionality to this class directly.
    /// </summary>
    public LevelController levelController;
}
