using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.XarmMoveit;
using UnityEngine;
using UnityEngine.UI;

public class BehaviourTreeNodesDisplayer : MonoBehaviour
{
    public Text firstLayerNodeText;
    public GameObject firstLayerNode;
    public Text secondLayerNode1Text;
    public GameObject secondLayerNode1;
    public Text secondLayerNode2Text;
    public GameObject secondLayerNode2;
    public Text thirdLayerNode1Text;
    public GameObject thirdLayerNode1;
    public Text thirdLayerNode2Text;
    public GameObject thirdLayerNode2;
    public GameObject line1;
    public GameObject line2;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTree(BehaviorTreeMsg msg)
    {
        firstLayerNode.SetActive(true);
        firstLayerNodeText.text = msg.First_layer[0].node_name;
        secondLayerNode1.SetActive(true);
        secondLayerNode1Text.text = msg.Second_layer[0].node_name;
        if(msg.Second_layer.Length > 1)
        {
            secondLayerNode2.SetActive(true);
            secondLayerNode2Text.text = msg.Second_layer[1].node_name;
        }
        else
        {
            secondLayerNode2.SetActive(false);
        }

        if(msg.Third_layer.Length == 0)
        {
            thirdLayerNode1.SetActive(false);
            thirdLayerNode2.SetActive(false);
        }
        else if(msg.Third_layer.Length == 1)
        {
            thirdLayerNode1.SetActive(true);
            thirdLayerNode1Text.text = msg.Third_layer[0].node_name;
            thirdLayerNode2.SetActive(false);
        }
        else if (msg.Third_layer.Length == 2)
        {
            thirdLayerNode1.SetActive(true);
            thirdLayerNode1Text.text = msg.Third_layer[0].node_name;
            thirdLayerNode2.SetActive(true);
            thirdLayerNode2Text.text = msg.Third_layer[1].node_name;
            if (msg.Third_layer[1].parent == msg.Second_layer[0].node_name)
            {
                line1.SetActive(true);
                line2.SetActive(false);
            }
            else if (msg.Second_layer.Length > 1 && msg.Third_layer[1].parent == msg.Second_layer[1].node_name)
            {
                line1.SetActive(false);
                line2.SetActive(true);
            }
        }
    }
}
