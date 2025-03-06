using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyotaTrainingUIManager : MonoBehaviour
{
    public bool isAdmin = false;
    public int taskID = 0;
    public int stationID = 0;

    public List<TrainingTaskUIManager> tasks = new List<TrainingTaskUIManager>();

    public GameObject loginPage;
    public GameObject taskSelectionPage;
    public GameObject stationSelectionPage;
    public GameObject stationNextStepPage;


    // Start is called before the first frame update
    void Start()
    {
        ShowLoginPage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRole(bool isAdmin)
    {
        this.isAdmin = isAdmin;
    }

    public void SetTask(int id)
    {
        taskID = id;
    }

    public void SetStation(int id)
    {
        stationID = id;
    }

    public void ShowLoginPage()
    {
        loginPage.SetActive(true);
        taskSelectionPage.SetActive(false);
        stationSelectionPage.SetActive(false);
        stationNextStepPage.SetActive(false);
        for(int i = 0; i < tasks.Count; i++)
        {
            tasks[i].HideStationPages();
        }
    }

    public void ShowTaskSelectionPage()
    {
        loginPage.SetActive(false);
        taskSelectionPage.SetActive(true);
        stationSelectionPage.SetActive(false);
        stationNextStepPage.SetActive(false);
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].HideStationPages();
        }
    }

    public void ShowStationSelectionPage()
    {
        loginPage.SetActive(false);
        taskSelectionPage.SetActive(false);
        stationSelectionPage.SetActive(true);
        int numberOfStation = tasks[taskID].stationPages.Count;
        SetStationVisibility(numberOfStation);
        stationNextStepPage.SetActive(false);
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].HideStationPages();
        }
    }

    public void ShowStationPage()
    {
        loginPage.SetActive(false);
        taskSelectionPage.SetActive(false);
        stationSelectionPage.SetActive(false);
        stationNextStepPage.SetActive(false);
        for (int i = 0; i < tasks.Count; i++)
        {
            if (taskID == i)
            {
                tasks[i].ShowStationPage(stationID);
            }
            else
            {
                tasks[i].HideStationPages();
            }
        }
    }

    public void ShowTaskNextStepPage() 
    {
        loginPage.SetActive(false);
        taskSelectionPage.SetActive(false);
        stationSelectionPage.SetActive(false);
        stationNextStepPage.SetActive(true);
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].HideStationPages();
        }
    }


    public void HideAllStations()
    {
        for (int i = 0; i < stationSelectionPage.transform.childCount; i++)
        {
            GameObject child = stationSelectionPage.transform.GetChild(i).gameObject;
            if (child.name.Contains("station"))
            {
                child.SetActive(false);
            }
        }
    }
    public void SetStationVisibility(int numberToShow)
    {
        HideAllStations();  // 先隐藏所有
        ShowStations(numberToShow);  // 再显示指定数量
    }
    
    public void ShowStations(int count)
    {
        int shownCount = 0;

        // 遍历所有子物体
        for (int i = 0; i < stationSelectionPage.transform.childCount; i++)
        {
            // 只处理名称包含"station"的子物体
            GameObject child = stationSelectionPage.transform.GetChild(i).gameObject;
            if (child.name.Contains("station"))
            {
                if (shownCount < count)
                {
                    child.SetActive(true);  // 显示该station
                    shownCount++;
                }
                else
                {
                    child.SetActive(false); // 超出指定数量时隐藏
                }
            }
            else
            {
                child.SetActive(true); // set non-station object as true
            }
        }
    }
}
