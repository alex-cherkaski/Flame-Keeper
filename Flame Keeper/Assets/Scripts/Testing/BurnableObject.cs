using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnableObject : MonoBehaviour
{
    public float burnTime;
    public bool automaticReset;
    public GameObject resetObject;
    public Vector3 resetTransform;
    public Quaternion resetRotation;

    private float timer;
    private bool isBurning;
    private bool finishedBurning;

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
            timer += Time.deltaTime;
            this.GetComponent<Renderer>().material.color = Color.Lerp(colorStart, colorEnd, Time.deltaTime * burnTime);

            if (timer > burnTime)
            {
                // destroy the object after the specified wait time
                Burn();
                if (automaticReset)
                {
                    ResetThis();
                }
            }
        }
        else
        {
            this.GetComponent<Renderer>().material.SetColor("_Color", colorStart);
            
            timer = 0.0f;
        }
    }

    private void FixedUpdate()
    {
        Rigidbody body = GetComponent<Rigidbody>();

        if (body.velocity.y > 0)
        {
            body.velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        }
    }

    private void Burn()
    {
        isBurning = false;
        currBurner = null;
        timer = 0.0f;
        finishedBurning = true;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        this.transform.position = resetTransform;
        this.transform.rotation = resetRotation;
    }

    public void ResetThis()
    {
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
        isBurning = false;
    }
}
