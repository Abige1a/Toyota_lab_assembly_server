using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserStationItem : MonoBehaviour
{
    public int stationID;
    public string taskName;

    public void SetItem(int id, string name)
    {
        stationID = id;
        taskName = name;
        GetComponentInChildren<Text>().text = "Station " + id + "\n" + name;
        GetComponent<VRButton>().events.AddListener(SelectStation);
    }

    public void SelectStation()
    {
        ClientContentManager.instance.selectedStationID = stationID;
        ClientContentManager.instance.ShowPagePanel();
    }
}
