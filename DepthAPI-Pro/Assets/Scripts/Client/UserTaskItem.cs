using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserTaskItem : MonoBehaviour
{
    public int taskID;
    public string taskName;

    public void SetItem(int id, string name)
    {
        taskID = id;
        taskName = name;
        //GetComponentInChildren<Text>().text = "Production " + id + "\n" + name;
        GetComponentInChildren<Text>().text = name;
        GetComponent<VRButton>().events.AddListener(SelectTask);
    }

    public void SelectTask()
    {
        ClientContentManager.instance.selectedTaskID = taskID;
        ClientContentManager.instance.ShowStationPanel();
    }
}
