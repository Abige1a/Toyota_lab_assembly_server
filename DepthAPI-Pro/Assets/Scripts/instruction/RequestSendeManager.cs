using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class RequestSendeManager : MonoBehaviour
{
    public DetectionDataManager detectionDataManager;
    public GameObject ballPrefab;
    public GameObject panelPerfab;
    public List<GameObject> spheres = new List<GameObject>();
    public List<string> target_object_list= new List<string>();
    public GameObject cncTaskObject;
    private PageManager cnc_pagemanager;
    public Transform centerEyeAnchor;
    private List<GameObject> arraws= new List<GameObject>();
    public float adjustmentx;
    public float adjustmenty;
    public float adjustmentz;
    public GameObject arrowPrefab;

    // Start is called before the first frame update
    void Start()
    {
        find_cnc_task();
        target_object_list.Add("doll");
        target_object_list.Add("rectangle");
        target_object_list.Add("silver");
        target_object_list.Add("silver");
        target_object_list.Add("rectangle");
        target_object_list.Add("allen key");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void cleararrow()
    {
        foreach (GameObject arr in arraws)
        {
            if (arr != null)
            {
                Destroy(arr);
            }
        }

        // Clear the list
        arraws.Clear();
        Debug.Log("All spheres cleared.");

    }

    public void clearall()
    {
        cleararrow();
        ClearSpheres();
    }
    public void generate_one_sphere()
    {
        ClearSpheres();
        Vector3 positionInCameraSpace = new Vector3 (0.0f,0.0f,1.0f);
        Vector3 worldPosition_ball = centerEyeAnchor.TransformPoint(positionInCameraSpace);
        Quaternion worldRotation = centerEyeAnchor.rotation;
        GameObject onesphere = Instantiate(ballPrefab, worldPosition_ball, worldRotation);
        spheres.Add(onesphere);
        generate_arrow(worldPosition_ball);

    }

    public void generate_arrow(Vector3 referencePosition)
    {
        cleararrow();
        Vector3 arrowPosition = referencePosition + new Vector3(adjustmentx, adjustmenty, adjustmentz);

        // 实例化箭头
        GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);

        // 设置箭头朝向，可以使箭头面向参考位置
        arrow.transform.LookAt(referencePosition);
        arraws.Add(arrow);
    }
    public void find_cnc_task()
    {
        //GameObject cncTaskObject = GameObject.Find("cnctask");
        if (cncTaskObject != null)
        {
            Debug.LogError("find cnc task object");
            cnc_pagemanager = cncTaskObject.GetComponent<PageManager>();
            if (cnc_pagemanager != null)
            {
                int currentPageIndex = cnc_pagemanager.pageIndex;
                Debug.Log("Current Page Index: " + currentPageIndex);
            }
            else
            {
                Debug.LogWarning("PageManager component not found on CNCtask.");
            }
        }
        else
        {
            Debug.LogWarning("CNCtask GameObject not found.");
        }
    }

    public void hint_request_pose()
    {
        if (cnc_pagemanager != null)
        {
            Debug.LogError(cnc_pagemanager.pageList.Count);
            if (cnc_pagemanager.pageList.Count == target_object_list.Count)
            {
                int pageindex= cnc_pagemanager.pageIndex;

                string target_object= target_object_list[pageindex];
                generate_sphere_on_pose(target_object);

            }
        }
        

    
    }

    public void ClearSpheres()
    {
        // Loop through each sphere in the list and destroy it
        foreach (GameObject sphere in spheres)
        {
            if (sphere != null)
            {
                Destroy(sphere);
            }
        }

        // Clear the list
        spheres.Clear();
        Debug.Log("All spheres cleared.");
    }

    public void generate_info_panel(Transform spheretransform, float heightoffset)
    {
        GameObject panel = Instantiate(panelPerfab);
        panel.transform.SetParent(spheretransform); // Attach panel to sphere
        panel.transform.localPosition = new Vector3(0, heightoffset, 0); // Offset above sphere

        // Set the panel's text (assuming you have a TextMeshPro or Text component in the panel)
        TMP_Text panelText = panel.GetComponentInChildren<TMP_Text>();
        if (panelText != null)
        {
            panelText.text = "You have selected the correct object"; // Set any relevant information here
        }

        // Initially hide the panel
        panel.SetActive(false);
    }

    
    public void generate_sphere_on_pose(string target_object) 
    {
        ClearSpheres();

        string text = "user_0";
        List<int> numberList = new List<int> { 10,10};
        //List<string> object_list= new List<string>();
        //object_list.Add(target_object);
        Dictionary<string, object> additionalInfo= new Dictionary<string, object> { { "object_name", target_object } };

        List<Tuple<Vector3, Quaternion>> positionList = detectionDataManager.getDetectionPositionbyName(text, numberList, additionalInfo);

        // 处理返回的 positionList，生成小球
        if (positionList != null && positionList.Count > 0)
        {
            foreach (var position in positionList)
            {
                // 创建小球

                Vector3 positionInCameraSpace = position.Item1;
                Vector3 worldPosition_ball = centerEyeAnchor.TransformPoint(positionInCameraSpace);
                Quaternion worldRotation = centerEyeAnchor.rotation;
                GameObject onesphere = Instantiate(ballPrefab, worldPosition_ball, position.Item2);

                generate_info_panel(onesphere.transform, 0.1f);
                spheres.Add(onesphere);

            }
        }
        else
        {
            Debug.Log("No positions returned.");
        }
    }
}
