using UnityEngine;

public class PlayerControllerSimple : MonoBehaviour, DynamicLightSource
{
    [Header("Parameters")]
    public float velocity = 5;
    public float turnSpeed = 10;

    [Header("Child Controllers")]
    public DynamicLightController playerLightController;

    int _lanternUses;
    private int lanternUses
    {
        get
        {
            return _lanternUses;
        }
        set
        {
            _lanternUses = value;

            if (playerLightController && playerLightController.IsSetup())
                OnLightSourceValueChange(_lanternUses);
        }
    }
    private int maxLanternUses = 6;
    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;
    private Transform levelCamera;

    public bool inWater = false; // TODO: Dont make public, bring WaterCollision.cs into this script
    public float jumpForce = 1.0f;
    public float gravityModifier = 1.0f;
    public LayerMask ground;

    private float lockMovementTime = 0.0f;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Vector3 checkpointPosition;

    private bool enableInput;

        /// <summary>
    /// Setup initial parameters for the character
    /// </summary>
    public void Setup(Vector3 startingPosition, int startingLanternUses, int maxLanternUses)
    {
        this.transform.position = startingPosition;
        this.lanternUses = startingLanternUses;
        this.maxLanternUses = Mathf.Max(startingLanternUses, maxLanternUses);

        if (playerLightController)
        {
            playerLightController.Setup(this);
        }
    }

    /// <summary>
    /// Assigns GameObject references
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        levelCamera = Camera.main.transform;
        enableInput = true;
    }

    /// <summary>
    /// Non-physics based update
    /// </summary>
    void Update()
    {
        // Check if player has fallen out of the world
        if (transform.position.y < -25)
        {
            GoToLastCheckpoint();
        }
    }

    /// <summary>
    /// Notifies light controller of new value
    /// </summary>
    public void OnLightSourceValueChange(int newValue)
    {
        playerLightController.CalculateLightTargets(newValue);
    }

    /// <summary>
    /// Returns the number of lantern uses the player currently has
    /// </summary>
    /// <returns></returns>
    public int GetLightSourceValue()
    {
        return lanternUses;
    }

    /// <summary>
    /// Returns the number of lantern uses the player currently has
    /// </summary>
    /// <returns></returns>
    public int GetCurrentLanternUsesLeft()
    {
        return lanternUses;
    }

    /// <summary>
    /// Uses the lantern and returns true if possible, otherwise, false;
    /// </summary>
    public bool RequestLanternUse()
    {
        if (lanternUses == 0)
        {
            return false;
        }

        lanternUses--;
        return true;
    }

    /// <summary>
    /// Adds to the lantern uses and returns true if possible, otherwise, false;
    /// </summary>
    public bool RequestLanternAddition()
    {
        if (lanternUses == maxLanternUses)
        {
            return false;
        }

        lanternUses++;
        return true;
    }

    /// <summary>
    /// Read movement axis and store it in the input variable
    /// </summary>
    private void GetInput()
    {
        if (enableInput)
        {
            input.x = Input.GetAxisRaw(StringConstants.Input.HorizontalMovement);
            input.y = Input.GetAxisRaw(StringConstants.Input.VerticalMovement);
        }
        else
        {
            input.x = 0.0f;
            input.y = 0.0f;
        }
        
    }

    /// <summary>
    /// Calculates the current angle of movement relative to the camera
    /// </summary>
    private void CalculateDirection()
    {
        angle = Mathf.Atan2(input.x, input.y);
        angle = Mathf.Rad2Deg * angle;
        angle += levelCamera.eulerAngles.y;
    }

    /// <summary>
    /// Rotates the character transform
    /// </summary>
    private void Rotate()
    {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Moves the character transform with respect to its velocity
    /// </summary>
    private void Move()
    {
        transform.position += transform.forward * velocity * Time.deltaTime;
    }

    /// <summary>
    /// Physics based update
    /// </summary>
    void FixedUpdate()
    {
        // Modify gravity when falling
        if (rb.useGravity)
        {
            // Increase gravity on when falling, how most other platformers do it (Mario, basically).
            if (rb.velocity.y < -0.01f)
            {
                rb.AddForce(Physics.gravity * rb.mass * gravityModifier);
            }
            else
            {
                rb.AddForce(Physics.gravity * rb.mass);
            }
        }

        if (lockMovementTime > 0.0f)
        {
            lockMovementTime -= Time.deltaTime;
            return;
        }

        if (Grounded() && Input.GetButton(StringConstants.Input.JumpButton) && enableInput)
        {
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
        }

        GetInput();

        if (Mathf.Abs(input.x) == 0 && Mathf.Abs(input.y) == 0)
        {
            return;
        }

        CalculateDirection();
        Rotate();
        Move();
    }

    /// <summary>
    /// Checks if the player is on the ground by raycasting downwards and checking
    /// if the hit object has a "Ground" layer
    /// </summary>
    private bool Grounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 1.0f, ground);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop angular velocity when colliding with an object or else the player
        // will continue to rotate
        rb.angularVelocity = new Vector3(0, 0, 0);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Stop angular velocity when colliding with an object or else the player
        // will continue to rotate
        rb.angularVelocity = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Gets the current velocity in the transform's forward direction
    /// </summary>
    public float GetVelocity()
    {
        return velocity;
    }

    /// <summary>
    /// Sets the current velocity in the transform's forward direction
    /// </summary>
    public void SetVelocity(float newVelocity)
    {
        velocity = newVelocity;
    }


    /// <summary>
    /// Checks if we have picked up a crystal
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringConstants.Tags.CrystalTag))
        {
            CrystalScript crystalScript = other.gameObject.GetComponent<CrystalScript>();
            if (crystalScript != null)
            {
                lanternUses += (int)crystalScript.GetWarmth();
                other.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("No Crystal Script attached to an object labeled as a crystal! " + other.name);
            }
        }
    }

    /// <summary>
    /// Overrides light value for animations (eg, floating in water)
    /// </summary>
    public void ScaleLightSource(float percent)
    {
        playerLightController.Scale(percent);
    }

    /// <summary>
    /// Sets the player's current position as the last checkpoint
    /// </summary>
    public void RecordCheckpoint()
    {
        checkpointPosition = this.transform.position;
    }

    /// <summary>
    /// Reset the player back to their last checkpoint (ie, last pedestal lit)
    /// </summary>
    public void GoToLastCheckpoint()
    {
        this.transform.position = checkpointPosition;

        // TODO: Either don't lock movement or have the player blink like a normal respawn mechanic
        lockMovementTime = 1.0f;
    }

    public void EnableInput()
    {
        enableInput = true;
    }

    public void DisableInput()
    {
        enableInput = false;
    }
}
