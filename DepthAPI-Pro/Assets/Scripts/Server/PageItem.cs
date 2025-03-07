using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageItem : MonoBehaviour
{
    public int pageID;
    public string taskName;

    public void SetItem(int id, string name)
    {
        pageID = id;
        taskName = name;
        GetComponentInChildren<Text>().text = "Page " + id + "\n" + name;
        GetComponent<Button>().onClick.AddListener(SelectPage);
    }

    public void SelectPage()
    {
        ServerContentManager.instance.selectedPageID = pageID;
        ServerContentManager.instance.ShowPageEditor();
    }
}
