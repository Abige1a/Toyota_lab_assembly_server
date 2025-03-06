using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLimitation : MonoBehaviour
{
    public Vector3 maxRange;
    public Vector3 minRange;

    private Vector3 lastPos;

    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(IsWithinCube(transform.position, Vector3.zero, minRange) || !IsWithinCube(transform.position, Vector3.zero, maxRange))
        {
            transform.position = lastPos;
        }
        lastPos = transform.position;
    }

    bool IsWithinCube(Vector3 position, Vector3 cubeCenter, Vector3 cubeSize)
    {
        Vector3 minPoint = cubeCenter - cubeSize;
        Vector3 maxPoint = cubeCenter + cubeSize;

        return (position.x >= minPoint.x && position.x <= maxPoint.x &&
                position.y >= minPoint.y && position.y <= maxPoint.y &&
                position.z >= minPoint.z && position.z <= maxPoint.z);
    }
}
