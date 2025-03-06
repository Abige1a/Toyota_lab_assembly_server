using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewController : MonoBehaviour
{
    public VRScrollView targetView;
    public float aimThreshold = 30.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Angle(targetView.transform.position - transform.position, transform.forward) < aimThreshold)
        {
            targetView.isHighlighted = true;
            targetView.background.color = targetView.highlightedColor;
            //targetView.ScrollDown();
            if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y > 0)
            {
                targetView.ScrollUp();
            }
            else if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y < 0)
            {
                targetView.ScrollDown();
            }
        }

    }
}
