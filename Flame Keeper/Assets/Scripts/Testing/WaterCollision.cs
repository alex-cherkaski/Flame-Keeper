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
    private bool playerAlive;
    float timer;

    [Header("Colour Effect (Test)")]
    public string colourComponent;
    private Color originalColour;
    private GameObject coloured;

    private PlayerControllerSimple playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerControllerSimple>();
        playerTouching = false;
        playerAlive = true;
        outOfWaterSpeed = playerController.velocity;
        coloured = this.gameObject.transform.Find(colourComponent).gameObject;
        originalColour = coloured.GetComponent<Renderer>().material.GetColor("_Color");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTouching && playerAlive)
        {
            timer += Time.deltaTime;
            coloured.GetComponent<Renderer>().material.color = Color.blue;
            float percentToDeath = Mathf.Clamp(timer / waitTime, 0.0f, 1.0f);
            playerController.ScaleLightSource(1.0f-percentToDeath);
            if (timer > waitTime)
            {
                coloured.GetComponent<Renderer>().material.color = Color.white;
                playerController.GoToLastGroundedPosition();
                playerAlive = false;
                timer = 0f;
            }
        }
        else
        {
            timer = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water"))
        {
            return;
        }
        playerTouching = true;
        playerController.inWater = true;

        playerController.SetVelocity(inWaterSpeed);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
        {
            return;
        }
        playerTouching = false;
        playerAlive = true;
        coloured.GetComponent<Renderer>().material.color = Color.red;
        playerController.SetVelocity(outOfWaterSpeed);
        playerController.inWater = false;
    }
}
