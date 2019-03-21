using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnableObject : MonoBehaviour
{
    // Note for coders in this script:
    // 'Warming' means the object is on the flame but can be saved if the player deactivates the pedestal
    // 'Burning' means the object was warming for so long that it is now dissolving into nothing

    // Duration between when the object first touches the flame and when it starts to dissolve (ie, when the player can't save it anymore)
    public float warmTime;

    // Duration of how long it takes the object to dissolve
    public float burnTime;

    // How long until the reset kicks in after a burn
    public float resetDelay;

    [Header("Reset position as an offset")]
    public bool useStartPositionOffetOnReset;
    private Vector3 startingLocalPosition;
    public Vector3 localOffset;

    [Header("Reset position as an offset")]
    public bool useWorldPositionOnReset;
    public Vector3 resetTransform;

    public bool automaticReset;
    public GameObject resetObject;
    public Quaternion resetRotation;
    public Color heatedColor;

    [Header("Shader Properties")]
    public Collider burnObjectCollider; // Used to calculate heights passed into the burn shader
    public float burnBoundsPadding;

    // State variables
    private BurnerObject currBurner;
    private float timer;
    private bool isWarming;
    private bool isBurning;
    private bool finishedBurning;

    // Material varibables
    private Material burnMaterial;
    private Color colorStart;
    private Color colorEnd;

    // Bounding box values
    private float startBurnHeight;
    private float endBurnHeight;


    // Start is called before the first frame update
    void Start()
    {
        burnMaterial = this.GetComponent<Renderer>().material;

        if (useStartPositionOffetOnReset)
        {
            startingLocalPosition = this.transform.localPosition;
        }


        startBurnHeight = burnObjectCollider.bounds.center.y - (burnObjectCollider.bounds.size.y / 2.0f) - burnBoundsPadding;
        endBurnHeight = burnObjectCollider.bounds.center.y + (burnObjectCollider.bounds.size.y / 2.0f) + burnBoundsPadding;
        burnMaterial.SetFloat("_BurnHeight", startBurnHeight);

        isBurning = false;
        colorStart = burnMaterial.GetColor("_Color");
        colorEnd = heatedColor;
    }

    // Update is called once per frame
    void Update()
    {
        // It's in world space so I have to calculate this every frame, damn
        startBurnHeight = burnObjectCollider.bounds.center.y - (burnObjectCollider.bounds.size.y / 2.0f) - burnBoundsPadding;
        endBurnHeight = burnObjectCollider.bounds.center.y + (burnObjectCollider.bounds.size.y / 2.0f) + burnBoundsPadding;
        burnMaterial.SetFloat("_BurnHeight", startBurnHeight);

        if (!isBurning && currBurner != null && currBurner.IsBurning())
        {
            isWarming = true;
        }
        else
        {
            isWarming = false;
        }

        if (isWarming && timer <= warmTime)
        {
            timer += Time.deltaTime;
            burnMaterial.color = Color.Lerp(colorStart, colorEnd, timer / warmTime);

            if (timer > warmTime)
            {
                // start the burn animation
                isBurning = true;
                timer = 0.0f;
            }
        }
        else if (isBurning && timer <= burnTime + resetDelay)
        {
            timer += Time.deltaTime;
            float percent = Mathf.Clamp((timer / burnTime), 0.0f, 1.0f);
            burnMaterial.SetFloat("_BurnHeight", Mathf.Lerp(startBurnHeight, endBurnHeight, percent));

            if (timer > burnTime)
            {
                this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                this.burnObjectCollider.enabled = false;
            }

            if (timer > burnTime + resetDelay)
            {
                FinishBurn();
                if (automaticReset)
                {
                    ResetThis();
                }
            }
        }
        else
        {
            burnMaterial.SetColor("_Color", colorStart);

            timer = 0.0f;
        }
    }

    private void FinishBurn()
    {
        this.transform.position = resetTransform;
        this.transform.rotation = resetRotation;
        isWarming = false;
        isBurning = false;
        currBurner = null;
        timer = 0.0f;
        finishedBurning = true;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void FixedUpdate()
    {
        Rigidbody body = GetComponent<Rigidbody>();

        if (body.velocity.y > 0)
        {
            body.velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        }
    }

    public void ResetThis()
    {
        if (useStartPositionOffetOnReset)
        {
            this.transform.localPosition = startingLocalPosition + localOffset;
        }
        else
        {
            this.transform.position = resetTransform;
        }
        this.transform.rotation = resetRotation;
        this.burnObjectCollider.enabled = true;
        this.GetComponent<Renderer>().material.SetColor("_Color", colorStart);
        finishedBurning = false;
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    public bool CanBeReset()
    {
        return finishedBurning;
    }

    private void OnTriggerEnter(Collider other)
    {
        BurnerObject burner = other.gameObject.GetComponent<BurnerObject>();
        if (burner != null)
        {
            currBurner = burner;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BurnerObject burner = other.gameObject.GetComponent<BurnerObject>();
        if (burner != null)
        {
            isBurning = false;
        }
    }
}
