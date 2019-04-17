using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Vector2 anchorView = Vector2.left;
    public float rotateSpeed = 1.0f;
    public float rotationalDegrees = 90.0f; // The most you can rotate the camera from the anchor in degrees

    private Quaternion startingRotation;
    private Vector3 startingViewDirection;
    private float viewRadius; // Distance from player on the x/z plane
    private PlayerControllerSimple player;

    private void Start()
    {
        player = FlameKeeper.Get().levelController.GetPlayer();

        startingRotation = this.transform.rotation;
    }

    // Late update so we know that player has already calculated their position
    void Update()
    {
        // Keep the Camera holder where the player is.
        this.transform.position = player.transform.position;

        // Check if we are allowing input
        float viewInput = Input.GetAxisRaw(StringConstants.Input.CameraView);
        if (player.IsInputEnabled() && player.IsCameraRotationEnabled() && Mathf.Abs(viewInput) > 0.15f) // A little buffer for numerical stability, otherwise camera will drift
        {
            Vector3 toCamera = mainCamera.transform.position - player.transform.position;
            float cameraAngle = Vector2.SignedAngle(new Vector2(toCamera.x, toCamera.z), Vector2.left);
            float inputAngle = rotateSpeed * -viewInput * Time.deltaTime;

            // Don't let them rotate past the max angular offset
            if (Mathf.Sign(inputAngle) != Mathf.Sign(cameraAngle) || Mathf.Abs(cameraAngle + inputAngle) <= rotationalDegrees)
            {
                this.transform.Rotate(Vector3.up, inputAngle);
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
