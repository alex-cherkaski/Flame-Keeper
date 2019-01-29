using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed;
    public float sidewaysSpeed;
    public float jumpForce;

    private Rigidbody rb;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (Input.GetKey("d"))
        {
            this.rb.AddForce(0, 0,-forwardSpeed);
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
}
