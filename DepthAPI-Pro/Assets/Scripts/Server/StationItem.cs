using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationItem : MonoBehaviour
{
    public int stationID;
    public string taskName;

    public void SetItem(int id, string name)
    {
        stationID = id;
        taskName = name;
        GetComponentInChildren<Text>().text = "Station " + id + "\n" + name;
        GetComponent<Button>().onClick.AddListener(SelectStation);
    }

    public void SelectStation()
    {
        ServerContentManager.instance.selectedStationID = stationID;
        ServerContentManager.instance.ShowPagePanel();
    }
}
