using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleLevelConfig : LevelConfig
{
    public PlayerControllerSimple player;

    [Header("Player parameters")]
    public Vector3 playerStartingPosition;
    public int startingLanternUses;
    public int maxLanternUses;

    [Header("Datamining parameters")]
    public DataminingController.DataScenes scene;

    private List<Pedestal> levelPedestals;
    //private Totem levelTotem; or whatever, link this when we have the script

    protected override void OnLevelStart()
    {
        base.OnLevelStart();

        if (player == null)
        {
            Debug.LogError("No player assigned to this temple level's config!");
            return;
        }

        FlameKeeper.Get().dataminingController.StartTrackingScene(scene);

        // Set up the player according the level parameters
        player.Setup(playerStartingPosition,
            startingLanternUses,
            maxLanternUses);
    }

    private void OnDestroy()
    {
        FlameKeeper.Get().dataminingController.StopTrackingScene();
    }

    // Finds and stores all the important objects we want to reference in each level
    // ie, pedestals, totems, etc
    void GetSceneObjects()
    {
        levelPedestals = new List<Pedestal>(FindObjectsOfType<Pedestal>());
        //levelTotem = FindObjectOfType<Totem>();
    }

    public List<Pedestal> GetAllPedestals()
    {
        return levelPedestals;
    }
}

