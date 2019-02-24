using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControllerSimple))]
public class WaterCollision : MonoBehaviour
{

    public float waitTime = 3f;
    public float inWaterSpeed;
    private float outOfWaterSpeed;
    private bool playerTouching;
    float timer;

    [Header("Colour Effect (Test)")]
    public string colourComponent;
    private Color originalColour;
    private GameObject coloured;

    private bool checkWaterStatus = false;

    private PlayerControllerSimple playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerControllerSimple>();
        playerTouching = false;
        outOfWaterSpeed = playerController.velocity;
        coloured = this.gameObject.transform.Find(colourComponent).gameObject;
        originalColour = coloured.GetComponent<Renderer>().material.GetColor("_Color");
    }

    // Update is called once per frame
    void Update()
    {
        // If the player jumped into water, hasnt gotten out yet, and is still alive
        if (checkWaterStatus && !OutOfWater() && timer <= waitTime)
        {
            timer += Time.deltaTime;
            coloured.GetComponent<Renderer>().material.color = Color.blue;
            float percentToDeath = Mathf.Clamp(timer / waitTime, 0.0f, 1.0f);
            playerController.ScaleLightSource(1.0f - percentToDeath);
            if (timer > waitTime)
            {
                // Reset properties and respawn player
                playerController.GoToLastCheckpoint();
                coloured.GetComponent<Renderer>().material.color = originalColour;
                checkWaterStatus = false;
                playerTouching = false;
                playerController.SetVelocity(outOfWaterSpeed);
                timer = 0.0f;
            }
        }
        else
        {
            // Make sure player has normal speed if we aren't calculating water stuff
            playerController.SetVelocity(outOfWaterSpeed);
        }
    }

    /// <summary>
    /// Checks if the player is not touching the water and is grounded
    /// </summary>
    bool OutOfWater()
    {
        return !playerTouching && playerController.Grounded();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water"))
        {
            return;
        }
        checkWaterStatus = true;
        playerTouching = true;

        playerController.SetVelocity(inWaterSpeed);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
        {
            return;
        }
        playerTouching = false;
    }
}
