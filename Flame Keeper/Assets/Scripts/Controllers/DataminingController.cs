using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using System.IO;

public class DataminingController : BaseController
{
    private string filePath;
    private StreamWriter textWriter;

    private DataScenes currentScene;
    private float levelStartTime;
    private int respawns;

    public enum DataScenes
    {
        Tutorial = 0,
        Level1 = 1
    }

    public void Start()
    {
        string timeAtStartup = DateTime.Now.ToString("yyyy_MM_dd___hh_mm_ss");

        string directoryPrefix = Application.dataPath + "/../PlaytestData";
        System.IO.Directory.CreateDirectory(directoryPrefix); // Make sure our directory exists
        filePath = directoryPrefix  + "/" + timeAtStartup + ".txt";
        textWriter = new StreamWriter(filePath, true);
        textWriter.WriteLine("Playtest report for " + DateTime.Now.ToString("yyyy/MM/dd hh:mm tt") + ":");
        textWriter.WriteLine("");
    }

    // Call when you want to start tracking data in a scene (ie, when the scene starts)
    public void StartTrackingScene(DataScenes scene)
    {
        currentScene = scene;
        levelStartTime = Time.time;
        respawns = 0;
    }

    // Call when you want to stop tracking data in a scene (ie, when a user completes the level)
    public void StopTrackingScene()
    {
        if (textWriter == null)
            return;

        textWriter.WriteLine("=== User completed " + DataSceneToString(currentScene) + " ===");
        textWriter.WriteLine("--- Scene statistics ---");
        textWriter.WriteLine("Completion time: " + (Time.time - levelStartTime) + " seconds");
        textWriter.WriteLine("Respawns: " + respawns);
        textWriter.WriteLine("");
    }

    // Call when the player respawns
    public void OnPlayerRespawn()
    {
        respawns++;
    }

    private string DataSceneToString(DataScenes scene)
    {
        switch (scene)
        {
            case DataScenes.Tutorial:
                return "Tutorial Level";
            case DataScenes.Level1:
                return "Level 1";
            default:
                return "Unknown";
        }
        return string.Empty;
    }

    // Close and write the datamining when this controller is destroyed
    // (ie, application exits)
    public void OnDestroy()
    {
        if (textWriter == null)
            return;

        textWriter.Close();
    }
}