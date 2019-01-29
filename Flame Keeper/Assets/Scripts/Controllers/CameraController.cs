using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public GameObject playerCamera;
    public float rotationSpeed;

    private Vector3 offset;

    private void Start()
    {
        // Place the camera holder at the same location as the player.
        this.transform.position = player.transform.position;

        offset = this.transform.position - player.transform.position;
    }

    void Update()
    {
        // Keep the Camera holder where the player is.
        this.transform.position = player.transform.position;

        // Keep the player camera behind the player.
        playerCamera.transform.position = playerCamera.transform.position + offset;

        if (Input.GetKey("e"))
        {
            // Rotate the camera right about the local y axis.
            transform.Rotate(0, 90 * Time.deltaTime, 0);
        }   
        if (Input.GetKey("q"))
        {
            // Rotate the camera left about the local y axis.
            transform.Rotate(0, -90 * Time.deltaTime, 0);
        }
    }
}
