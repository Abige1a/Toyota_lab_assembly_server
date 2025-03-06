using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRecenter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            OVRManager.display.RecenterPose();
            Debug.LogWarning("Recentered!");
        }
    }
}
