using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraShotEditor : MonoBehaviour
{
    public RectTransform imageTransform;
    public GameObject pointPrefab;

    public GameObject rectPointPrefab;
    public GameObject rectCoverPrefab;

    public Text editText;

    public float speed;

    public SendGPTService service;

    private bool isEditing = false;
    private int editingMode = 0; //0 = point, 1 = rect
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> rects = new List<GameObject>();
    private GameObject tempPoint;
    private List<GameObject> tempRectPoints = new List<GameObject>();


    void Start()
    {
        
    }

    void Update()
    {
        if (isEditing)
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                SwitchEditingMode();
            }
            Vector2 input = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            if(editingMode == 0)
            {
                RectTransform pointTransform = tempPoint.GetComponent<RectTransform>();
                pointTransform.anchoredPosition += input * speed * Time.deltaTime;

                if (pointTransform.anchoredPosition.x < 0)
                {
                    pointTransform.anchoredPosition = new Vector2(0, pointTransform.anchoredPosition.y);
                }
                if (pointTransform.anchoredPosition.x > imageTransform.sizeDelta.x)
                {
                    pointTransform.anchoredPosition = new Vector2(imageTransform.sizeDelta.x, pointTransform.anchoredPosition.y);
                }
                if (pointTransform.anchoredPosition.y > 0)
                {
                    pointTransform.anchoredPosition = new Vector2(pointTransform.anchoredPosition.x, 0);
                }
                if (pointTransform.anchoredPosition.y < -imageTransform.sizeDelta.y)
                {
                    pointTransform.anchoredPosition = new Vector2(pointTransform.anchoredPosition.x, -imageTransform.sizeDelta.y);
                }

                if (OVRInput.GetDown(OVRInput.Button.Two))
                {
                    points.Add(tempPoint);
                    tempPoint = Instantiate(pointPrefab, imageTransform);
                }
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    if (points.Count > 0)
                    {
                        Destroy(points[points.Count - 1]);
                        points.RemoveAt(points.Count - 1);
                    }
                }
            }
            else if(editingMode == 1)
            {
                RectTransform pointTransform = tempRectPoints[tempRectPoints.Count - 1].GetComponent<RectTransform>();
                pointTransform.anchoredPosition += input * speed * Time.deltaTime;

                if (pointTransform.anchoredPosition.x < 0)
                {
                    pointTransform.anchoredPosition = new Vector2(0, pointTransform.anchoredPosition.y);
                }
                if (pointTransform.anchoredPosition.x > imageTransform.sizeDelta.x)
                {
                    pointTransform.anchoredPosition = new Vector2(imageTransform.sizeDelta.x, pointTransform.anchoredPosition.y);
                }
                if (pointTransform.anchoredPosition.y > 0)
                {
                    pointTransform.anchoredPosition = new Vector2(pointTransform.anchoredPosition.x, 0);
                }
                if (pointTransform.anchoredPosition.y < -imageTransform.sizeDelta.y)
                {
                    pointTransform.anchoredPosition = new Vector2(pointTransform.anchoredPosition.x, -imageTransform.sizeDelta.y);
                }
                if (OVRInput.GetDown(OVRInput.Button.Two))
                {
                    if(tempRectPoints.Count == 2)
                    {
                        //Create a rect
                        GameObject tempRect = Instantiate(rectCoverPrefab, imageTransform);
                        tempRect.GetComponent<RectTransform>().anchoredPosition = (tempRectPoints[0].GetComponent<RectTransform>().anchoredPosition + tempRectPoints[1].GetComponent<RectTransform>().anchoredPosition) / 2;
                        float rectX = Mathf.Abs(tempRectPoints[0].GetComponent<RectTransform>().anchoredPosition.x - tempRectPoints[1].GetComponent<RectTransform>().anchoredPosition.x);
                        float rectY = Mathf.Abs(tempRectPoints[0].GetComponent<RectTransform>().anchoredPosition.y - tempRectPoints[1].GetComponent<RectTransform>().anchoredPosition.y);
                        tempRect.GetComponent<RectTransform>().sizeDelta = new Vector2(rectX, rectY);
                        rects.Add(tempRect);
                        for (int i = 0; i < tempRectPoints.Count; i++)
                        {
                            Destroy(tempRectPoints[i]);
                        }
                        tempRectPoints.Clear();
                    }
                    GameObject tempRectPoint = Instantiate(rectPointPrefab, imageTransform);
                    tempRectPoints.Add(tempRectPoint);
                }
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    if (rects.Count > 0)
                    {
                        Destroy(rects[rects.Count - 1]);
                        rects.RemoveAt(rects.Count - 1);
                    }
                }
            }
        }
    }

    public void PressButton()
    {
        if (isEditing)
        {
            EndEdit();
        }
        else
        {
            StartEdit();
        }
    }
    
    public void StartEdit()
    {
        isEditing = true;
        editingMode = 0;
        tempPoint = Instantiate(pointPrefab, imageTransform);
        editText.text = "Finish";
    }

    public void EndEdit()
    {
        isEditing = false;
        Destroy(tempPoint);
        for (int i = 0; i < tempRectPoints.Count; i++)
        {
            Destroy(tempRectPoints[i]);
        }
        tempRectPoints.Clear();
        editText.text = "Edit";
    }

    public void SwitchEditingMode()
    {
        if(editingMode == 0)
        {
            Destroy(tempPoint);
            GameObject tempRectPoint = Instantiate(rectPointPrefab, imageTransform);
            tempRectPoints.Add(tempRectPoint);
        }
        else if (editingMode == 1)
        {
            for(int i = 0; i < tempRectPoints.Count; i++)
            {
                Destroy(tempRectPoints[i]);
            }
            tempRectPoints.Clear();
            tempPoint = Instantiate(pointPrefab, imageTransform);
        }
        editingMode = (editingMode == 0) ? 1 : 0;
    }

    public void SendPoints()
    {
        service.call_service_stage_points(points);
    }

    public void SendAlignmentPoints()
    {
        service.call_service_alignment_points(points);
    }

    public void SendBoxes()
    {
        service.call_service_stage_boxes(rects);
    }
}
