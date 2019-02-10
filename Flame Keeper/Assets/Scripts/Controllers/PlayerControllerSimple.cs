using UnityEngine;

public class PlayerControllerSimple : MonoBehaviour, DynamicLightSource
{
    [Header("Parameters")]
    public int startingLanternUses = 3;
    public int maxLanternUses = 6;
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
    private Vector2 input;
    private float angle;
    private Quaternion targetRotation;
    private Transform levelCamera;

    public float jumpForce = 1.0f;
    public float gravityModifier = 1.0f;
    public LayerMask ground;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        levelCamera = Camera.main.transform;

        lanternUses = startingLanternUses;

        if (playerLightController)
        {
            playerLightController.Setup(this);
        }
    }


    // Update is called once per frame
    void Update()
    {

    }



    #region LanternFunctions

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

    #endregion



    private void GetInput()
    {
        input.x = Input.GetAxisRaw(StringConstants.Input.HorizontalMovement);
        input.y = Input.GetAxisRaw(StringConstants.Input.VerticalMovement);
    }

    private void CalculateDirection()
    {
        angle = Mathf.Atan2(input.x, input.y);
        angle = Mathf.Rad2Deg * angle;
        angle += levelCamera.eulerAngles.y;
    }

    private void Rotate()
    {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void Move()
    {
        transform.position += transform.forward * velocity * Time.deltaTime;
    }

    void FixedUpdate()
    {
        GetInput();

        if (Mathf.Abs(input.x) == 0 && Mathf.Abs(input.y) == 0)
        {
            return;
        }

        CalculateDirection();
        Rotate();
        Move();

        if (rb.useGravity)
        {
            rb.AddForce(Physics.gravity * rb.mass * gravityModifier);
        }

        if (Input.GetButton(StringConstants.Input.JumpButton) && Grounded())
        {
            Debug.Log("Pressed A");
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
        }
    }

    private bool Grounded()
    {
        return Physics.CheckCapsule(capsuleCollider.bounds.center,
                                    new Vector3(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y, capsuleCollider.center.z),
                                    capsuleCollider.radius,
                                    ground);
    }
}
