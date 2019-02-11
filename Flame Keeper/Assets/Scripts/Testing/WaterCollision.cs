using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    
    public float waitTime = 3f;
    public int inWaterSpeed;
    private int outOfWaterSpeed;
    private bool playerTouching;
    private bool playerAlive;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        playerTouching = false;
        playerAlive = true;
        outOfWaterSpeed = this.GetComponent<Player>().forceMagnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTouching && playerAlive)
        {
            timer += Time.deltaTime;
            this.GetComponent<Renderer>().material.color = Color.blue;
            if (timer > waitTime)
            {
                this.GetComponent<Renderer>().material.color = Color.red;
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

        this.GetComponent<Player>().SetSpeed(inWaterSpeed);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water"))
        {
            return;
        }
        playerTouching = false;
        playerAlive = true;
        this.GetComponent<Renderer>().material.color = Color.white;
        this.GetComponent<Player>().SetSpeed(outOfWaterSpeed);
    }
}
