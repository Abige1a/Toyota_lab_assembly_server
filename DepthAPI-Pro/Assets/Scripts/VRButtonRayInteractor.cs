using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRButtonRayInteractor : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform rayOrigin;
    public float defaultLength = 5.0f;
    public LayerMask interactableLayer;

    void Update()
    {
        // Set up raycast
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        float lineLength = defaultLength;

        // Perform raycast
        if (Physics.Raycast(ray, out hit, defaultLength, interactableLayer))
        {
            lineLength = hit.distance; // Adjust line length to hit distance

            // Check if hit object has a VRButton component and if the trigger button is pressed
            VRButton vrButton = hit.collider.GetComponent<VRButton>();
            if (vrButton != null && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                vrButton.Invoke(); // Invoke the VRButton action
            }
            if (vrButton != null)
            {
                vrButton.HangingInvoke(); // Invoke the hanging action
            }
           
        }

        // Set the length of the line renderer
        lineRenderer.SetPosition(0, rayOrigin.position);
        lineRenderer.SetPosition(1, rayOrigin.position + rayOrigin.forward * lineLength);
    }
}
