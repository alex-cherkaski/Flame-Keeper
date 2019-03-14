using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    public float rotateSpeed = 1.0f;
    public float maxAngularOffset = 45.0f; // The most you can rotate the camera from the starting position in degrees

    private Quaternion startingRotation;
    private Vector3 startingViewDirection;
    private float viewRadius; // Distance from player on the x/z plane
    private PlayerControllerSimple player;

    private void Start()
    {
        player = FlameKeeper.Get().levelController.GetPlayer();

        float xDist = (player.transform.position.x - mainCamera.transform.position.x);
        float zDist = (player.transform.position.z - mainCamera.transform.position.z);
        viewRadius = Mathf.Sqrt((xDist * xDist) + (zDist * zDist));

        startingRotation = this.transform.rotation;
        startingViewDirection = Vector3.Normalize(this.transform.position - mainCamera.transform.position);
    }

    // Late update so we know that player has already calculated their position
    void LateUpdate()
    {

        // Keep the Camera holder where the player is.
        this.transform.position = player.transform.position;

        // Check if we are allowing input
        float viewInput = Input.GetAxisRaw(StringConstants.Input.CameraView);
        if (player.IsInputEnabled() && Mathf.Abs(viewInput) > 0.15f) // A little buffer for numerical stability, otherwise camera will drift
        {
            // Rotate camera
            float currentAngle = Vector3.SignedAngle(startingViewDirection, Vector3.Normalize(this.transform.position - mainCamera.transform.position), Vector3.up);

            // Don't let them rotate past the max angular offset
            if (Mathf.Abs(currentAngle) <= maxAngularOffset || Mathf.Sign(currentAngle) != Mathf.Sign(-viewInput))
            {
                this.transform.Rotate(Vector3.up, rotateSpeed * -viewInput * Time.deltaTime);
            }


            /* -- Snap back to default with no input --
             * Not using this right now cuase I found it to make jumping difficult still
            Quaternion currentRotation = this.transform.rotation;
            this.transform.rotation = startingRotation;
            this.transform.Rotate(Vector3.up, maxAngularOffset * -viewInput);
            Quaternion targetRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotateSpeed * Time.deltaTime);
            */
        }

        mainCamera.transform.LookAt(player.transform);
    }
}
