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

        gameControllers.Add((int)ControllerType.TutorialController, new TutorialController());

        controllersList.AddRange(gameControllers.Values);
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


    /// Game Controllers:
    /// Add controllers here as opposed to adding specific methods to this class, just keeps
    /// everything a bit more organized.
    public enum ControllerType
    {
        TutorialController
    }

    private Dictionary<int, BaseController> gameControllers = new Dictionary<int, BaseController>(); // Dictionary of all controllers so look ups are fast
    private List<BaseController> controllersList = new List<BaseController>(); // List of all controllers so looping over all of them is fast

    public TutorialController tutorialController { get { return gameControllers[(int)ControllerType.TutorialController] as TutorialController;  } }

    private void Update()
    {
        foreach (BaseController controller in controllersList)
        {
            try
            {
                controller.Update(Time.deltaTime);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
