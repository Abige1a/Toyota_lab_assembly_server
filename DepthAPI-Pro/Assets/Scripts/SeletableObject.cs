using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeletableObject : MonoBehaviour
{
    public int index;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectObject()
    {
        if(FindObjectOfType<SendGPTService>() != null)
        {
            FindObjectOfType<SendGPTService>().SelectObject(index);
        }
    }
}
