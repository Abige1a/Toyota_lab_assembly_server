using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingTaskUIManager : MonoBehaviour
{
    public List<GameObject> stationPages = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowStationPage(int stationIndex)
    {
        for (int i = 0; i < stationPages.Count; i++)
        {
            if(i == stationIndex)
            {
                stationPages[i].SetActive(true);
            }
            else
            {
                stationPages[i].SetActive(false);
            }
        }
    }

    public void HideStationPages()
    {
        for (int i = 0; i < stationPages.Count; i++)
        {
            stationPages[i].SetActive(false);
        }
    }
}
