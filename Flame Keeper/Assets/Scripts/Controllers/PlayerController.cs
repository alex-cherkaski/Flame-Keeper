using UnityEngine;
using UnityEngine.PostProcessing;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed;
    public float sidewaysSpeed;
    public float jumpForce;
    public float warmth = 50;   // 0 to 100
    public LayerMask ground;

    public int startingLanternUses = 3;
    public int maxLanternUses = 3;

    [Header("Light Emission Properties")]
    public PostProcessingProfile postProcessing;
    public float vignetteAnimSpeed = 1.0f;
    public float lightFalloffFactor = 1.0f;

    private VignetteModel.Settings vignette;
    private int currentLanternUses;

    private Rigidbody rb;
    private SphereCollider collider;

    // Input Axis
    private string moveInputAxis = "Vertical";
    private string turnInputAxis = "Horizontal";

   
    public float rotationRate = 90;

    public float moveSpeed = 0.1f;

    void Start()
    {
        vignette = postProcessing.vignette.settings;
        currentLanternUses = startingLanternUses;

        vignette.intensity = 1.0f;
        postProcessing.vignette.settings = vignette;

        rb = GetComponent<Rigidbody>();
        collider = GetComponent<SphereCollider>();
    }

    private void OnApplicationQuit()
    {
        // Post processing values save during play, so reset them here
        vignette.intensity = 0.0f;
        postProcessing.vignette.settings = vignette;
    }

    void Update()
    {
        // Calculate vignette values
        float lightDistance = GetDistanceToNearestLight();

        float targetVignetteIntensity = 1.0f - ((float)currentLanternUses / maxLanternUses);
        targetVignetteIntensity -= Mathf.Clamp(1.0f - lightDistance * lightFalloffFactor, 0.0f, 1.0f);
        targetVignetteIntensity = Mathf.Clamp(targetVignetteIntensity, 0.0f, 1.0f);

        vignette.intensity = Mathf.Lerp(vignette.intensity, targetVignetteIntensity, Time.deltaTime * vignetteAnimSpeed);
        postProcessing.vignette.settings = vignette;
    }


    // TODO:
    // Delete this, not the responsibility of the player. We should have a controller
    // that manages all light sources within a scene, and it can provide this data to the player.
    private float GetDistanceToNearestLight()
    {
        Light[] sceneLights = FindObjectsOfType<Light>();
        int lightCount = sceneLights.Length;
        float minDist = float.PositiveInfinity;

        for (int i = 0; i < lightCount; i++)
        {
            if (sceneLights[i].type == LightType.Directional)
                continue;

            float distToLight = Vector3.Distance(sceneLights[i].transform.position, this.transform.position);
            minDist = Mathf.Min(minDist, distToLight);
        }

        return minDist;
    }


    /// <summary>
    /// Uses the lantern and returns true if possible, otherwise, false;
    /// </summary>
    public bool RequestLanternUse()
    {
        if (currentLanternUses == 0)
        {
            return false;
        }

        currentLanternUses--;
        return true;
    }

    /// <summary>
    /// Adds to the lantern uses and returns true if possible, otherwise, false;
    /// </summary>
    public bool RequestLanternAddition()
    {
        if (currentLanternUses == maxLanternUses)
        {
            return false;
        }

        currentLanternUses++;
        return true;
    }

    private void FixedUpdate()
    {
        if (Input.GetKey("d"))
        {
            this.rb.AddForce(0, 0, -forwardSpeed);
        }
        if (Input.GetKey("a"))
        {
            this.rb.AddForce(0, 0, forwardSpeed);
        }
        if (Input.GetKey("w"))
        {
            this.rb.AddForce(sidewaysSpeed, 0, 0);
        }
        if (Input.GetKey("s"))
        {
            this.rb.AddForce(-sidewaysSpeed, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space) && Grounded())
        {
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
        }
    }

    private bool Grounded()
    {
        return Physics.CheckSphere(transform.position, collider.radius, ground);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Crystal"))
        {
            currentLanternUses++;
            warmth += other.gameObject.GetComponent<CrystalScript>().GetCharge();
            other.gameObject.SetActive(false);
        }
    }
}