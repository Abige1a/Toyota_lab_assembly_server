using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayPlaneIntersection : MonoBehaviour
{
    public Transform rayOrigin; // 射线的起始点

    public Transform planeObject;

    private Vector3 planePoint1; // 平面左下角的点
    private Vector3 planePoint2; // 平面左上角的点
    private Vector3 planePoint3; // 平面右上角的点
    private Vector3 planePoint4; // 平面右下角的点

    public int screenWidth = 1920; // 屏幕宽度（像素）
    public int screenHeight = 1080; // 屏幕高度（像素）

    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            FindIntersectionPoint();
        }

    }

    public Vector2 FindIntersectionPoint()
    {
        Vector3 intersectionPoint;

        planePoint1 = planeObject.position - planeObject.right * planeObject.localScale.x / 2.0f - planeObject.up * planeObject.localScale.y / 2.0f;
        planePoint2 = planeObject.position - planeObject.right * planeObject.localScale.x / 2.0f + planeObject.up * planeObject.localScale.y / 2.0f;
        planePoint3 = planeObject.position + planeObject.right * planeObject.localScale.x / 2.0f + planeObject.up * planeObject.localScale.y / 2.0f;
        planePoint4 = planeObject.position + planeObject.right * planeObject.localScale.x / 2.0f - planeObject.up * planeObject.localScale.y / 2.0f;

        if (RayIntersectsPlane(rayOrigin.position, rayOrigin.forward, planePoint1, planePoint2, planePoint3, planePoint4, out intersectionPoint))
        {
            Debug.Log("Intersection Point: " + intersectionPoint);
            Vector2 pixelPosition = GetPixelPosition(intersectionPoint, planePoint1, planePoint2, planePoint3, planePoint4, screenWidth, screenHeight);
            Debug.Log("Pixel Position: " + pixelPosition);
            return pixelPosition;
        }
        else
        {
            Debug.Log("No Intersection");
            return new Vector2(-1, -1);
        }
    }

    bool RayIntersectsPlane(Vector3 rayOrigin, Vector3 rayDirection, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.zero;

        // 计算平面的法向量
        Vector3 planeNormal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        // 射线和平面是否平行
        float denominator = Vector3.Dot(rayDirection, planeNormal);
        if (Mathf.Abs(denominator) < 1e-6f)
        {
            return false; // 射线和平面平行
        }

        // 计算射线和平面的交点
        float t = Vector3.Dot(p1 - rayOrigin, planeNormal) / denominator;
        if (t < 0)
        {
            return false; // 射线和平面不相交
        }

        intersectionPoint = rayOrigin + t * rayDirection;

        // 检查交点是否在平面内
        if (PointInQuad(intersectionPoint, p1, p2, p3, p4))
        {
            return true;
        }

        return false;
    }

    bool PointInQuad(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // 将四边形分成两个三角形，检查点是否在任一三角形内
        return PointInTriangle(point, p1, p2, p3) || PointInTriangle(point, p1, p3, p4);
    }

    bool PointInTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // 使用重心坐标法判断点是否在三角形内
        Vector3 v2v1 = v2 - v1;
        Vector3 v3v1 = v3 - v1;
        Vector3 pv1 = point - v1;

        float dot00 = Vector3.Dot(v3v1, v3v1);
        float dot01 = Vector3.Dot(v3v1, v2v1);
        float dot02 = Vector3.Dot(v3v1, pv1);
        float dot11 = Vector3.Dot(v2v1, v2v1);
        float dot12 = Vector3.Dot(v2v1, pv1);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // 检查点是否在三角形内
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    Vector2 GetPixelPosition(Vector3 intersectionPoint, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int screenWidth, int screenHeight)
    {
        // 计算平面左下角到右上角的向量
        Vector3 planeXDir = p4 - p1; // 左下到右下
        Vector3 planeYDir = p2 - p1; // 左下到左上

        // 计算交点在平面上的相对位置
        Vector3 relativePos = intersectionPoint - p1;
        float xFraction = Vector3.Dot(relativePos, planeXDir.normalized) / planeXDir.magnitude;
        float yFraction = Vector3.Dot(relativePos, planeYDir.normalized) / planeYDir.magnitude;

        // 转换为屏幕像素位置
        float pixelX = xFraction * screenWidth;
        float pixelY = yFraction * screenHeight;

        return new Vector2(pixelX, pixelY);
    }
}
