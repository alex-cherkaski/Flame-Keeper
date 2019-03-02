using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnableObject : MonoBehaviour
{
    public float burnTime;

    private float timer;
    private bool isBurning;

    private Color colorStart;
    private Color colorEnd;
    private BurnerObject currBurner;
    // Start is called before the first frame update
    void Start()
    {
        isBurning = false;
        colorStart = this.GetComponent<Renderer>().material.GetColor("_Color");
        colorEnd = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        if (currBurner != null && currBurner.IsBurning())
        {
            isBurning = true;
        }
        else
        {
            isBurning = false;
        }

        if (isBurning && timer <= burnTime)
        {
            Debug.Log("im burning");
            timer += Time.deltaTime;
            this.GetComponent<Renderer>().material.color = Color.blue;
            this.GetComponent<Renderer>().material.color = Color.Lerp(colorStart, colorEnd, burnTime);

            if (timer > burnTime)
            {
                // Reset properties and respawn player
                Object.Destroy(this.gameObject);
            }
        }
        else
        {
            // Make sure player has normal speed if we aren't calculating water stuff
            this.GetComponent<Renderer>().material.SetColor("_Color", colorStart);
            timer = 0.0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BurnerObject burner = other.gameObject.GetComponent<BurnerObject>();
        if (burner != null)
        {
            Debug.Log("i found a burner");
            currBurner = burner;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isBurning = false;
    }
}
