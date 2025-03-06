using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BehaviourTreeDisplayer : MonoBehaviour
{
    public List<Text> textList = new List<Text>();
    public List<Image> nodeList = new List<Image>();
    public List<string> contentList = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        InitText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitText()
    {
        for (int i = 0; i < contentList.Count; i++)
        {
            textList[i].text = contentList[i];
            nodeList[i].color = Color.white;
        }
    }

    public void HighlightNode(string text, Color color)
    {
        for (int i = 0; i < contentList.Count; i++)
        {
            if (contentList[i].Equals(text))
            {
                nodeList[i].color = color;
            }
            else if (nodeList[i].color.Equals(color))
            {
                nodeList[i].color = Color.white;
            }
        }
    }

}
