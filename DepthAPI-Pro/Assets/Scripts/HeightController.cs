using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightController : MonoBehaviour
{
    public OVRInput.Controller controller; // Set this to Left Hand Controller in the Unity Inspector
    public Transform objectToControl;
    public float joystickSensitivity = 0.1f; // Sensitivity of the joystick control
    public float triggerSensitivity = 0.1f; // Sensitivity of the trigger control

    // Update is called once per frame
    void Update()
    {
        // Check if the controller is connected
        if (!OVRInput.IsControllerConnected(controller))
        {
            Debug.LogWarning("Controller not connected.");
            return;
        }

        // Get input from the left joystick for x and y axis movement
        float joystickXInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller).x;
        float joystickYInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller).y;
        float xMovement = joystickXInput * joystickSensitivity * Time.deltaTime;
        float yMovement = joystickYInput * joystickSensitivity * Time.deltaTime;

        // Get input from the index trigger for forward movement in z-axis
        float indexTriggerInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        float forwardMovement = indexTriggerInput * triggerSensitivity * Time.deltaTime;

        // Get input from the hand trigger for backward movement in z-axis
        float handTriggerInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        float backwardMovement = -handTriggerInput * triggerSensitivity * Time.deltaTime;

        // Update the object's position
        Vector3 newPosition = objectToControl.position + new Vector3(-xMovement, -yMovement, forwardMovement + backwardMovement);
        objectToControl.position = newPosition;
    }
}
