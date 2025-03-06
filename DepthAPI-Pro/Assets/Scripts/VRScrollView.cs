using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRScrollView : MonoBehaviour
{
    public bool isHighlighted = false;
    public float scrollSpeed;
    public Color normalColor;
    public Color highlightedColor;
    public Image background;
    public RectTransform content;
    public RectTransform scrollView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        if (isHighlighted)
        {
            background.color = highlightedColor;
        }
        else
        {
            background.color = normalColor;
        }
        isHighlighted = false;
    }

    public void ScrollUp()
    {
        content.anchoredPosition3D -= new Vector3(0, scrollSpeed * Time.deltaTime, 0);
        if(content.anchoredPosition3D.y < 0)
        {
            content.anchoredPosition3D = Vector3.zero;
        }
    }

    public void ScrollDown()
    {
        content.anchoredPosition3D += new Vector3(0, scrollSpeed * Time.deltaTime, 0);
        if (content.anchoredPosition3D.y > content.sizeDelta.y - scrollView.sizeDelta.y)
        {
            content.anchoredPosition3D = new Vector3(content.anchoredPosition3D.x, content.sizeDelta.y - scrollView.sizeDelta.y, content.anchoredPosition3D.z);
        }
    }


}
