using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 300f;

    public Transform playerBody;

    [HideInInspector] public float zRotation;
    private float rotationYVelocity, cameraXVelocity;
    public float yRotationSpeed, xCameraSpeed;

    [HideInInspector] public float wantedYRotation;
    [HideInInspector] public float currentYRotation;

    [HideInInspector] public float wantedCameraXRotation;
    [HideInInspector] public float currentCameraXRotation;

    public float topAngleView = 60;
    public float buttonAngleView = - 45;

    float deltaTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        mouseControl();

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void mouseControl()
    {
        wantedYRotation += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        wantedCameraXRotation -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        wantedCameraXRotation = Mathf.Clamp(wantedCameraXRotation, buttonAngleView, topAngleView);

        currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, yRotationSpeed);
        currentCameraXRotation = Mathf.SmoothDamp(currentCameraXRotation, wantedCameraXRotation, ref cameraXVelocity, xCameraSpeed);

        playerBody.rotation = Quaternion.Euler(0, currentYRotation, 0);
        transform.localRotation = Quaternion.Euler(currentCameraXRotation, 0, zRotation);
    }
}
