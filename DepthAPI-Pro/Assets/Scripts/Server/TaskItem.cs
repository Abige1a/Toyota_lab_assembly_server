using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskItem : MonoBehaviour
{
    public int taskID;
    public string taskName;

    public void SetItem(int id, string name)
    {
        taskID = id;
        taskName = name;
        GetComponentInChildren<Text>().text = "Task " + id + "\n" + name;
        GetComponent<Button>().onClick.AddListener(SelectTask);
    }

    public void SelectTask()
    {
        ServerContentManager.instance.selectedTaskID = taskID;
        ServerContentManager.instance.ShowStationPanel();
    }
}
