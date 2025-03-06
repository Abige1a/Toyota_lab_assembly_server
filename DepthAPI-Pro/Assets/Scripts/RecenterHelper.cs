using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecenterHelper : MonoBehaviour
{
    public Transform CameraOffset; // set to parent of main Camera

    Vector3[] mBoundary;

    void Start()
    {
        OVRManager.display.RecenteredPose += OculusRecenter;
    }

    void RotateXZ(Vector3 a, float rot, out Vector3 b)
    {
        // rot is clockwise radians
        var sin = Mathf.Sin(rot);
        var cos = Mathf.Cos(rot);
        b = new Vector3(a.x * cos + a.z * sin, a.y, -a.x * sin + a.z * cos);
    }

    // Instead of recentering, don't recenter!
    void OculusRecenter()
    {
        var points = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);

        if (mBoundary != null && points.Length == mBoundary.Length)
        {
            var deltaPos0 = mBoundary[0] - points[0];
            var newPointMid = points[points.Length / 2] + deltaPos0;
            var vec0ToOldMid = mBoundary[points.Length / 2] - mBoundary[0];
            var vec0ToNewMid = newPointMid - mBoundary[0];
            var angOldMid = Mathf.Atan2(vec0ToOldMid.z, vec0ToOldMid.x);
            var angNewMid = Mathf.Atan2(vec0ToNewMid.z, vec0ToNewMid.x);
            var deltaRot = angOldMid - angNewMid;

            while (deltaRot > Mathf.PI)
                deltaRot -= 2 * Mathf.PI;
            while (deltaRot < -Mathf.PI)
                deltaRot += 2 * Mathf.PI;

            var reverse0 = -points[0];
            RotateXZ(reverse0, -deltaRot, out reverse0);
            reverse0 += points[0];
            var pos = CameraOffset.TransformPoint(deltaPos0 + reverse0);
            pos = CameraOffset.parent.InverseTransformPoint(pos);
            CameraOffset.localPosition = pos;

            var angs = CameraOffset.localEulerAngles;
            angs.y -= deltaRot * Mathf.Rad2Deg;
            CameraOffset.localEulerAngles = angs;
        }

        mBoundary = points;
    }

    public void Recenter()
    {
        mBoundary = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);

        Transform cam = Camera.main.transform;
        float yawOffset = -cam.localEulerAngles.y;
        float ang = -yawOffset * Mathf.Deg2Rad;
        float sinAng = Mathf.Sin(ang);
        float cosAng = Mathf.Cos(ang);
        float xOffset = cam.localPosition.z * sinAng - cam.localPosition.x * cosAng;
        float zOffset = -cam.localPosition.z * cosAng - cam.localPosition.x * sinAng;

        CameraOffset.localPosition = new Vector3(xOffset, 0, zOffset);
        CameraOffset.localEulerAngles = new Vector3(0, yawOffset, 0);
    }
}
