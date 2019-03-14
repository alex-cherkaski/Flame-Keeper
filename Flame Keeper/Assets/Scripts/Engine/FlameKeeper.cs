using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

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
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this);
        SetupControllers();
    }

    /// <summary>
    /// Use this to get the universal game manager that is the parent manager to
    /// all game controllers.
    /// </summary>
    /// <returns> The benevolent game manager </returns>
    public static FlameKeeper Get()
    {
        if (_instance == null)
        {
            // Game didn't start from root or the game manager got deleted.
            // Warn that the game is in "simulator" mode and wasn't created organically
            Debug.LogError("No game manager, creating one now (Simulation Mode)");
            _instance = Resources.Load<FlameKeeper>(StringConstants.PrefabPaths.ManagerPath);
            DontDestroyOnLoad(_instance);
            _instance.SetupControllers();
            _instance.levelController.StartSimulationMode();
        }

        return _instance;
    }

    private List<BaseController> controllersList = new List<BaseController>(); // List of all controllers so looping over all of them is fast

    /// <summary>
    /// Game controllers:
    /// Add controllers here as needed, and avoid adding functionality to this class directly.
    /// </summary>
    public LevelController levelController;
    public DataminingController dataminingController;
    public MusicController musicController;

    private void SetupControllers()
    {
        if (levelController)
            controllersList.Add(levelController);
        if (dataminingController)
            controllersList.Add(dataminingController);
        if (musicController)
            controllersList.Add(musicController);

        foreach (BaseController controller in controllersList)
        {
            controller.Initialize();
        }
    }

    public void Update()
    {
        // If holding down the two reset buttons and start, reset the game
        if (Input.GetButton(StringConstants.Input.ResetOne)
            && Input.GetButton(StringConstants.Input.ResetTwo)
            && Input.GetButton(StringConstants.Input.Start))
        {
            ResetGame();
        }
    }

    /// <summary>
    /// Destroys the game manager and returns to the root scene
    /// used in case we need to reset the game while running or whatever
    /// </summary>
    private void ResetGame()
    {
        foreach (BaseController controller in controllersList)
        {
            controller.OnReset();
            controller.Initialize();
        }

        // This is gonna try to create another game manager, but whatever, we enforce only one can exist anyways
        SceneManager.LoadScene(StringConstants.SceneNames.RootSceneName);
    }
}
