using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        playerTouching = false;
        playerAlive = true;
        outOfWaterSpeed = this.GetComponent<PlayerControllerSimple>().velocity;
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
            if (timer > waitTime)
            {
                coloured.GetComponent<Renderer>().material.color = Color.white;
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

        this.GetComponent<PlayerControllerSimple>().SetVelocity(inWaterSpeed);
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
        this.GetComponent<PlayerControllerSimple>().SetVelocity(outOfWaterSpeed);
    }
}
