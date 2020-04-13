using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);

    private Transform playerBody;

    private Animator animator;

    // Store vertical rotation so we can clamp
    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = transform.parent;
        animator = playerBody.GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Late update lets the camera wait for movement calculations to occur first to reduce jittering
    void LateUpdate()
    {
        if (PauseMenu.isOn) { return; }

        // Get mouse up/down and right/left movement
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Smoothing stuff

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        var mouseDelta = new Vector2(mouseX, mouseY);
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        mouseX = Mathf.Lerp(mouseX, mouseDelta.x, 1f / smoothing.x);
        mouseY = Mathf.Lerp(mouseY, mouseDelta.y, 1f / smoothing.y);

        // End smoothing stuff

        // Rotate player left/right, camera will follow automatically
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate only camera up/down and arms but clamp it to prevent looking behind
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate arms/weapon up/down
        animator.SetFloat("LookAngle", xRotation);
    }
}
