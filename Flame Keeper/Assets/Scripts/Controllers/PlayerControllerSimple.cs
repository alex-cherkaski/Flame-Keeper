using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerControllerSimple : MonoBehaviour, DynamicLightSource
{
    [Header("Parameters")]
    public float maxVelocity = 5;
    private float currVelocity;
    public float turnSpeed = 10;

    public float targetVelocity = 0.0f;
    public float targetAngular = 0.0f;

    [Space]
    [Header("Child Controllers")]
    public DynamicLightController playerLightController;
    public PlayerOrbitals playerOrbitals;

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
    private Camera levelCamera;
    private Vector3 startingScale;

    [Space]
    [Header("Jump Parameters")]
    public float jumpForce = 1.0f;
    public float highJumpAdditionalForce = 1.0f;

    public float ascensionGravityModifier; // Gravity modifier when player is moving upwards veritcally
    public float descensionGravityModifier; // Gravity modifier when player is moving downwards veritcally
    public float groundedVelocityThreshold; // If the player exceeds this vertical velocity, they can not be considered grounded
    public float jumpDelay; // How long we wait before we honor another jump execution
    public float jumpBufferTime; // How long we will wait after the player presses jump to see if they are grounded
    public float highJumpBufferTime; // How long the player needs to hold the jump button to jump a high jump
    private float lastJumpPress; // The last time the player pressed the jumped button
    private float lastJumpExecute; // The last time we added the jump force to a player
    public float jumpBoxHeightOffset;
    public Vector3 jumpBoxDimensions;
    public LayerMask ground;

    private GameObject audioController;

    private bool validHighJump = false;
    private bool trackApexTime = false;
    private bool trackJumpTime = false;

    [Space]
    [Header("Global Light Source Parameters")]
    public float surfaceAttenuationFactor = 0.2f;
    public float surfaceNormalFactor = 0.2f;
    public Texture surfaceRampTex;

    [Space]
    private float lockMovementTime = 0.0f;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private List<MeshRenderer> renderers;
    private List<SkinnedMeshRenderer> skinRenderers;
    private Vector3 checkpointPosition;

    private bool enableInput = true;
    private bool enableRotation = true;

    [Header("Water Collision")]
    public float waitTime = 1f;
    public float inWaterSpeed;

    [Header("Animations")]
    public Animator animator;
    private const string runAnimBool = "Running";
    private const string drownAnimBool = "Drowning";
    private const string groundedAnimBool = "Grounded";
    private const string lightAnimTrigger = "Light";
    private const string jumpAnimTrigger = "Jump";

    private bool playerTouchingWater;
    private bool checkWaterStatus = false;
    private float timer;

    private Water currentWaterCollision;
    private bool currentGroundedState;
    private bool pastGroundedState;

    /// <summary>
    /// Variables to override where the character is going
    /// </summary>
    private bool isCustomTargetPositionSet = false;
    private Vector3 customTargetPosition = Vector3.zero;
    private Action customTargetOnComplete = null;
    private bool isCustomTargetRotationSet = false;
    private Vector3 customRotationPosition = Vector3.zero; // Point the player is suppose to look at
    private Action customRotateOnComplete = null;

    /// <summary>
    /// Draws the box check for jumping
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        Gizmos.DrawCube(transform.position + Vector3.up * jumpBoxHeightOffset, jumpBoxDimensions);
    }

    /// <summary>
    /// Setup initial parameters for the character
    /// </summary>
    public void Setup(Vector3 startingPosition, int startingLanternUses, int maxLanternUses)
    {
        this.transform.position = startingPosition;
        this.lanternUses = startingLanternUses;
        this.maxLanternUses = Mathf.Max(startingLanternUses, maxLanternUses);

        currVelocity = 0.0f;
        startingScale = this.transform.localScale;

        playerOrbitals.OnLanternUsesChanged(startingLanternUses, this.transform.position);

        animator.SetBool(runAnimBool, false);

        RecordCheckpoint();

        if (playerLightController)
        {
            playerLightController.Setup(this);
            Shader.SetGlobalFloat("_AttenutaionIntensity", surfaceAttenuationFactor);
            Shader.SetGlobalFloat("_NormalLightIntensity", surfaceNormalFactor);
            Shader.SetGlobalTexture("_PlayerRampTex", surfaceRampTex);
        }
    }

    /// <summary>
    /// Assigns GameObject references
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        levelCamera = Camera.main;
        renderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
        skinRenderers = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        //water collision setup
        playerTouchingWater = false;

        currentGroundedState = Grounded();
        pastGroundedState = Grounded();

        // Sound controller for player
        audioController = (GameObject)Instantiate(Resources.Load("audioController"), transform.position, transform.rotation);
        audioController.transform.SetParent(this.transform);
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

        animator.SetBool(groundedAnimBool, Grounded());
        animator.SetBool(drownAnimBool, playerTouchingWater);

        float curAnimVelocity = animator.GetFloat("Velocity");
        animator.SetFloat("Velocity", Mathf.Lerp(curAnimVelocity, targetVelocity, Time.deltaTime * 10.0f));

        float curAnimAngular = animator.GetFloat("Angular");
        animator.SetFloat("Angular", Mathf.Lerp(curAnimAngular, targetAngular, Time.deltaTime * 2.5f));

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
                this.SetVelocity(maxVelocity * input.magnitude);
                timer = 0.0f;
            }

            // Make the player sink while in water
            if (currentWaterCollision != null)
            {
                currentWaterCollision.Sink(percentToDeath);
            }
        }
        else
        {
            // Make sure player has normal speed if we aren't calculating water stuff
            checkWaterStatus = false;
            currentWaterCollision = null;
            playerTouchingWater = false;
            this.SetVelocity(maxVelocity * input.magnitude);
            timer = 0.0f;
        }

        // Check for jump button (Note: this needs to be in update)
        if (enableInput && Input.GetButtonDown(StringConstants.Input.JumpButton))
        {
            lastJumpPress = Time.fixedTime;
        }
    }

    public void MoveToPoint(Vector3 point, Action onComplete)
    {
        isCustomTargetPositionSet = true;
        customTargetPosition = point;
        customTargetOnComplete = onComplete;
    }

    public void RotateToPoint(Vector3 point, Action onComplete)
    {
        isCustomTargetRotationSet = true;
        customRotationPosition = point;
        customRotateOnComplete = onComplete;
    }

    public void SetBurnPercent(float percent)
    {
        float playerHeight = 4.0f; // estimate, close enough, need to update if player size changes for whatever reason
        float burnHeight = percent == 0.0f ? -1000.0f : (this.transform.position.y - playerHeight / 2.0f) + percent * playerHeight;
        renderers.RemoveAll(item => item == null);
        skinRenderers.RemoveAll(item => item == null);
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material.SetFloat("_BurnHeight", burnHeight);
        }
        foreach (SkinnedMeshRenderer renderer in skinRenderers)
        {
            renderer.material.SetFloat("_BurnHeight", burnHeight);
        }
    }

    /// <summary>
    /// Physics based update
    /// </summary>
    void FixedUpdate()
    {
        targetAngular = 0.0f;

        if (lockMovementTime > 0.0f)
        {
            animator.SetBool(runAnimBool, false);

            lockMovementTime -= Time.deltaTime;

            renderers.RemoveAll(item => item == null);
            skinRenderers.RemoveAll(item => item == null);

            // Blink character, uncomment when player is in
            foreach (MeshRenderer rend in renderers)
            {
                rend.enabled = (Time.time % 0.35f < 0.175) || lockMovementTime < 0.0f;
            }

            foreach (SkinnedMeshRenderer rend in skinRenderers)
            {
                rend.enabled = (Time.time % 0.35f < 0.175) || lockMovementTime < 0.0f;
            }

            return;
        }

        // Jump logic
        bool jumpDelayed = (Time.fixedTime - lastJumpExecute) < jumpDelay;

        // Check for jump execution
        if (!jumpDelayed && Grounded() && (Time.fixedTime - lastJumpPress) < jumpBufferTime)
        {
            // Start the initial jump
            if (!playerTouchingWater)
            {
                audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.jumpNormal);
            }

            animator.SetTrigger(jumpAnimTrigger);
            lastJumpExecute = Time.fixedTime;
            validHighJump = true;
            trackApexTime = true;
            trackJumpTime = true;
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
        }

        // Check if they are holding down the jump button
        if (validHighJump && !Input.GetButton(StringConstants.Input.JumpButton))
        {
            validHighJump = false;
        }

        // Check for a high jump (button held down after jump)
        if (validHighJump && (Time.fixedTime - lastJumpExecute) > highJumpBufferTime)
        {
            // They held the button, do a high jump
            //rb.AddForce(0, highJumpAdditionalForce, 0, ForceMode.Impulse);
            validHighJump = false;
        }

        // Modify gravity when falling
        if (rb.useGravity)
        {
            // Increase gravity on when falling, how most other platformers do it (Mario, basically).
            if (rb.velocity.y < -0.01f)
            {
                rb.AddForce(Physics.gravity * rb.mass * descensionGravityModifier);
            }
            else
            {
                rb.AddForce(Physics.gravity * rb.mass * ascensionGravityModifier);
            }
        }

        // Logging check
        if (trackApexTime && rb.velocity.y < -0.01f)
        {
            trackApexTime = false;

            Debug.Log("Apex time: " + (Time.time - lastJumpExecute));
        }

        GetInput();

        bool isMoving = !(Mathf.Abs(input.x) == 0 && Mathf.Abs(input.y) == 0);
        animator.SetBool(runAnimBool, isMoving && !isCustomTargetRotationSet);

        if (!isMoving)
        {
            return;
        }

        CalculateDirection();
        Rotate();

        if (!isCustomTargetRotationSet)
            Move();

        if (isCustomTargetPositionSet && Vector3.Distance(this.transform.position, customTargetPosition) < 0.1f)
        {
            isCustomTargetPositionSet = false;
            customTargetOnComplete?.Invoke();
        }

        float angleToTarget = Vector3.Angle(this.transform.forward, customRotationPosition - this.transform.position);
        if (isCustomTargetRotationSet && angleToTarget < 30.0f)
        {
            isCustomTargetRotationSet = false;
            customRotateOnComplete?.Invoke();
        }
    }

    /// <summary>
    /// Checks if the player is on the ground by raycasting a box downwards and checking
    /// if the hit object has a "Ground" layer
    /// </summary>
    public bool Grounded()
    {
        bool groundBelow = Physics.CheckBox(this.transform.position + Vector3.up * jumpBoxHeightOffset, jumpBoxDimensions, Quaternion.identity, ground);
        bool isFalling = Mathf.Abs(rb.velocity.y) > groundedVelocityThreshold;
        bool grounded = groundBelow && !isFalling;

        if (grounded && trackJumpTime && (Time.time - lastJumpExecute) > 0.1f)
        {
            Debug.Log("Jump time: " + (Time.time - lastJumpExecute));
            trackJumpTime = false;
        }

        return grounded;
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
    public bool RequestLanternUse(Vector3 source, Action onComplete = null)
    {
        if (lanternUses == 0)
        {
            return false;
        }

        lanternUses--;
        animator.SetTrigger(lightAnimTrigger);
        playerOrbitals.OnLanternUsesChanged(lanternUses, source, onComplete);
        return true;
    }

    /// <summary>
    /// Adds to the lantern uses and returns true if possible, otherwise, false;
    /// </summary>
    public bool RequestLanternAddition(Vector3 source, Action onComplete = null)
    {
        if (lanternUses == maxLanternUses)
        {
            return false;
        }

        lanternUses++;
        animator.SetTrigger(lightAnimTrigger);
        playerOrbitals.OnLanternUsesChanged(lanternUses, source, onComplete);
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
        else if (isCustomTargetPositionSet)
        {
            Vector3 screenDirection = levelCamera.WorldToScreenPoint(customTargetPosition) - levelCamera.WorldToScreenPoint(this.transform.position);
            input = new Vector2(screenDirection.x, screenDirection.y);
            input.Normalize();
        }
        else if (isCustomTargetRotationSet)
        {
            Vector3 screenDirection = levelCamera.WorldToScreenPoint(customRotationPosition) - levelCamera.WorldToScreenPoint(this.transform.position);
            input = new Vector2(screenDirection.x, screenDirection.y);
            input.Normalize();
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
        angle += levelCamera.transform.eulerAngles.y;
    }

    /// <summary>
    /// Rotates the character transform
    /// </summary>
    private void Rotate()
    {
        targetRotation = Quaternion.Euler(0, angle, 0);
        Vector3 vecA = transform.rotation * Vector3.forward;
        Vector3 vecB = targetRotation * Vector3.forward;
        var angleA = Mathf.Atan2(vecA.x, vecA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(vecB.x, vecB.z) * Mathf.Rad2Deg;
        float angleBetween = Mathf.DeltaAngle(angleA, angleB);
        targetAngular = Mathf.Clamp(angleBetween / 30.0f, -1.0f, 1.0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Moves the character transform with respect to its velocity
    /// </summary>
    private void Move()
    {
        Vector3 moveDirection = targetRotation * Vector3.forward;
        transform.position += moveDirection * currVelocity * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop angular velocity when colliding with an object or else the player
        // will continue to rotate
        rb.angularVelocity = new Vector3(0, 0, 0);

        RaycastHit hit;
        LayerMask layerMask = ~LayerMask.GetMask("Player");
        Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask);

        if (collision.gameObject.CompareTag(StringConstants.Tags.Platform) && hit.collider.CompareTag(StringConstants.Tags.Platform))
        {
            transform.SetParent(collision.gameObject.transform, true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Stop angular velocity when colliding with an object or else the player
        // will continue to rotate
        rb.angularVelocity = new Vector3(0, 0, 0);

        RaycastHit hit;
        LayerMask layerMask = ~LayerMask.GetMask("Player");
        Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask);

        if (collision.gameObject.CompareTag(StringConstants.Tags.Platform) && hit.collider.CompareTag(StringConstants.Tags.Platform))
        {
            transform.SetParent(collision.gameObject.transform, true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(StringConstants.Tags.Platform))
        {
            transform.parent = null;
            this.transform.localScale = startingScale; // Make sure scale doesnt drift off
        }
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
        targetVelocity = currVelocity / maxVelocity;
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
                lanternUses += (int)crystalScript.GetCharge();
                playerOrbitals.OnLanternUsesChanged(lanternUses, crystalScript.transform.position);
                other.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("No Crystal Script attached to an object labeled as a crystal! " + other.name);
            }
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.fire2);
        }

        //if the player is touching water
        if (other.CompareTag(StringConstants.Tags.Water))
        {
            checkWaterStatus = true;
            currentWaterCollision = other.GetComponent<Water>();
            playerTouchingWater = true;
            this.SetVelocity(inWaterSpeed);
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.water4);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if the player is getting out of the water
        if (other.CompareTag(StringConstants.Tags.Water))
        {
            playerTouchingWater = false;
            audioController.GetComponent<AudioController>().PlayAudioClip(AudioController.AudioClips.water2);
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

    public void RecordCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    /// <summary>
    /// Reset the player back to their last checkpoint (ie, last pedestal lit)
    /// </summary>
    public void GoToLastCheckpoint()
    {
        FlameKeeper.Get().dataminingController.OnPlayerRespawn();

        animator.SetBool(drownAnimBool, false);
        animator.SetTrigger("StopDrown");

        this.transform.position = checkpointPosition;
        this.rb.velocity = Vector3.zero;

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
    /// Enables player controlled camera rotation
    /// </summary>
    public void EnableRotation()
    {
        enableRotation = true;
    }

    /// <summary>
    /// Disables player controlled camera rotation
    /// </summary>
    public void DisableRotation()
    {
        enableRotation = false;
    }

    /// <summary>
    /// Returns value of enableInput
    /// </summary>
    public bool IsInputEnabled()
    {
        return enableInput;
    }

    /// <summary>
    /// Returns value of enableRotations
    /// </summary>
    public bool IsCameraRotationEnabled()
    {
        return enableRotation;
    }

    /// <summary>
    /// Checks if the player is not touching the water and is grounded
    /// </summary>
    bool OutOfWater()
    {
        return !playerTouchingWater && this.Grounded();
    }
}
