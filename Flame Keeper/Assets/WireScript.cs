using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireScript : ActivatableObject
{
    public Color activatedColor;
    public Color deactivatedColor;
    public Pedestal pedestal;

    private Color currentColor;
    private MeshRenderer meshRenderer;

    protected override void OnDepowered(Pedestal pedestal, int newLevel)
    {
        currentColor = deactivatedColor;
    }

    protected override void OnPowered(Pedestal pedestal, int newLevel)
    {
        currentColor = activatedColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (pedestal.GetCurrLevel() > 0)
        {
            meshRenderer.material.color = activatedColor;
            currentColor = activatedColor;
        }
        else
        {
            meshRenderer.material.color = deactivatedColor;
            currentColor = deactivatedColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, currentColor, Time.deltaTime);
    }
}
