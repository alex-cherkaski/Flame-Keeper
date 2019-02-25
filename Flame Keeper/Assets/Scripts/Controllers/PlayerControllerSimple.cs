using UnityEngine;

public class PlayerControllerSimple : MonoBehaviour, DynamicLightSource
{
    [Header("Parameters")]
    public float normalVelocity = 5;
    private float currVelocity;
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

    public float jumpForce = 1.0f;
    public float gravityModifier = 1.0f;
    public LayerMask ground;

    private float lockMovementTime = 0.0f;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    public MeshRenderer renderer;
    private Vector3 checkpointPosition;

    private bool enableInput;

    [Header("Water Collision")]
    public float waitTime = 3f;
    public float inWaterSpeed;
    private bool playerTouchingWater;
    private bool checkWaterStatus = false;
    private float timer;

    /// <summary>
    /// Setup initial parameters for the character
    /// </summary>
    public void Setup(Vector3 startingPosition, int startingLanternUses, int maxLanternUses)
    {
        this.transform.position = startingPosition;
        this.lanternUses = startingLanternUses;
        this.maxLanternUses = Mathf.Max(startingLanternUses, maxLanternUses);
        currVelocity = normalVelocity;

        RecordCheckpoint();

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

        //water collision setup
        playerTouchingWater = false;

    }

    /// <summary>
    /// Non-physics based update
    /// </summary>
    void Update()
    {
        // Pass in player's position to all materials which is used in lighting
        Shader.SetGlobalFloat("_PlayerMaxLightRange", 15.0f);
        Shader.SetGlobalFloat("_PlayerCurrentLightRange", playerLightController.pointLight.range);
        Shader.SetGlobalVector("_PlayerLightPosition", playerLightController.pointLight.transform.position);

        // Check if player has fallen out of the world
        if (transform.position.y < -25)
        {
            GoToLastCheckpoint();
        }

        // If the player jumped into water, hasnt gotten out yet, and is still alive
        if (checkWaterStatus && !OutOfWater() && timer <= waitTime)
        {
            timer += Time.deltaTime;
            float percentToDeath = Mathf.Clamp(timer / waitTime, 0.0f, 1.0f);
            this.ScaleLightSource(1.0f - percentToDeath);
            if (timer > waitTime)
            {
                // Reset properties and respawn player
                this.GoToLastCheckpoint();
                checkWaterStatus = false;
                playerTouchingWater = false;
                this.SetVelocity(normalVelocity);
                timer = 0.0f;
            }
        }
        else
        {
            // Make sure player has normal speed if we aren't calculating water stuff
            checkWaterStatus = false;
            playerTouchingWater = false;
            this.SetVelocity(normalVelocity);
            timer = 0.0f;
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
        transform.position += transform.forward * currVelocity * Time.deltaTime;
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

            // Blink character, uncomment when player is in
            //renderer.enabled = (Time.time % 0.35f < 0.175) || lockMovementTime < 0.0f;

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
    public bool Grounded()
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
    public float GetCurrVelocity()
    {
        return currVelocity;
    }

    /// <summary>
    /// Sets the current velocity in the transform's forward direction
    /// </summary>
    public void SetVelocity(float newVelocity)
    {
        currVelocity = newVelocity;
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

        //if the player is touching water
        if (other.CompareTag("Water"))
        {
            checkWaterStatus = true;
            playerTouchingWater = true;
            this.SetVelocity(inWaterSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if the player is getting out of the water
        if (other.CompareTag("Water"))
        {
            playerTouchingWater = false;
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
        this.rb.velocity = Vector3.zero;

        // TODO: Either don't lock movement or have the player blink like a normal respawn mechanic
        lockMovementTime = 1.0f;
    }

    /// <summary>
    /// Allows movement that requires input (e.g after getting out of a cutscene)
    /// </summary>
    public void EnableInput()
    {
        enableInput = true;
    }

    /// <summary>
    /// Disables movement that requires input (e.g during cutscenes)
    /// </summary>
    public void DisableInput()
    {
        enableInput = false;
    }

    /// <summary>
    /// Checks if the player is not touching the water and is grounded
    /// </summary>
    bool OutOfWater()
    {
        return !playerTouchingWater && this.Grounded();
    }


}
