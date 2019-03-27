using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestalWireScript : ActivatableObject
{
    private Transform[] allChildren;
    private List<GameObject> childObjects;

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
        allChildren = GetComponentsInChildren<Transform>();
        childObjects = new List<GameObject>();
        foreach (Transform child in allChildren)
        {
            childObjects.Add(child.gameObject);
        }

        meshRenderer = GetComponent<MeshRenderer>();
        foreach (GameObject child in childObjects)
        {
            if (child.CompareTag("ActivatableWire"))
            {
                meshRenderer = child.GetComponent<MeshRenderer>();
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(childObjects.Count);
        foreach (GameObject child in childObjects)
        {
            if (child.CompareTag(StringConstants.Tags.ActivatableWire))
            {
                meshRenderer = child.GetComponent<MeshRenderer>();
                meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, currentColor, Time.deltaTime);
            }
        }
    }
}
