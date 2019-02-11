using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelConfig : MonoBehaviour
{
    public PlayerControllerSimple player;

    [Header("Player parameters")]
    public Vector3 playerStartingPosition;
    public int startingLanternUses;
    public int maxLanternUses;

    private List<Pedestal> levelPedestals;
    //private Totem levelTotem; or whatever, link this when we have the script

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
