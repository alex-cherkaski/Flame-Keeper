using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnerObject : ActivatableObject
{
    public GameObject burningIndicator;

    private bool isBurning;
    private Color origColor;

    protected override void OnPowered(Pedestal pedestal, int newLevel)
    {
        if (newLevel > 0)
        {
            isBurning = true;
        }
        else
        {
            isBurning = false;
        }
    }

    protected override void OnDepowered(Pedestal pedestal, int newLevel)
    {
        if (newLevel > 0)
        {
            isBurning = true;
        }
        else
        {
            isBurning = false;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (GetLevel() > 0)
        {
            isBurning = true;
        }
        else
        {
            isBurning = false;
        }
        origColor = burningIndicator.GetComponent<Renderer>().material.GetColor("_Color");
    }

    // Update is called once per frame
    private void Update()
    {
        if (isBurning)
        {
            burningIndicator.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
    }

    public bool IsBurning()
    {
        return isBurning;
    }
}
