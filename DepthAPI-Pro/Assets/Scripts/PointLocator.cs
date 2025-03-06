using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointLocator : MonoBehaviour
{
    public Transform locateIndicator;
    public float moveSpeed = 2.0f;
    public Vector2 moveRange = new Vector2(0, 10);

    public UnityEvent locateEvents;


    void Start()
    {
        
    }

    void Update()
    {
        float verticalInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        Vector3 moveDirection = new Vector3(0, 0, verticalInput) * moveSpeed * Time.deltaTime;
        locateIndicator.transform.Translate(moveDirection);
        if(locateIndicator.transform.localPosition.z < moveRange.x)
        {
            locateIndicator.transform.localPosition = new Vector3(0, 0, moveRange.x);
        }
        if(locateIndicator.transform.localPosition.z > moveRange.y)
        {
            locateIndicator.transform.localPosition = new Vector3(0, 0, moveRange.y);
        }

            if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            locateEvents.Invoke();
        }
    }
}
