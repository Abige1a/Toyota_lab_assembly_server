using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;

public class ServerContentManager : MonoBehaviour
{
    public static ServerContentManager instance;

    public ServerAPI api;

    public GameObject taskItemPrefab;
    public GameObject stationItemPrefab;
    public GameObject pageItemPrefab;
    public GameObject textInputItemPrefab;
    public GameObject imageInputItemPrefab;

    public Transform taskItemContainer;
    public Transform stationItemContainer;
    public Transform pageItemContainer;
    public Transform inputItemContainer;

    public InputField ipInputField;

    public GameObject taskPanel;
    public GameObject stationPanel;
    public GameObject pagePanel;
    public GameObject pageEditor;

    public Text pageIndexText;
    public Dropdown pageTemplateDropdown;
    public List<PagePreviewItem> pagePreviewItems = new List<PagePreviewItem>();

    [HideInInspector] public TaskDatabase taskDatabase = new TaskDatabase();
    [HideInInspector] public int selectedTaskID;
    [HideInInspector] public int selectedStationID;
    [HideInInspector] public int selectedPageID;

    private List<GameObject> taskItems = new List<GameObject>();
    private List<GameObject> stationItems = new List<GameObject>();
    private List<GameObject> pageItems = new List<GameObject>();
    private List<GameObject> inputItems = new List<GameObject>();

    private string baseUrl = "http://127.0.0.1:5000";


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //FetchAllTasks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectToServer()
    {
        baseUrl = ipInputField.text;
        FetchAllTasks();
    }

    public void FetchAllTasks()
    {
        StartCoroutine(GetAllTasksCoroutine());
    }

    private IEnumerator GetAllTasksCoroutine()
    {
        string url = baseUrl + "/all_tasks";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching tasks: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text.Trim();

            // Optionally remove BOM if present
            if (jsonResponse.Length > 0 && jsonResponse[0] == '\uFEFF')
                jsonResponse = jsonResponse.Substring(1);

            Debug.Log("Server Response: " + jsonResponse);

            try
            {
                // Parse JSON using LitJson
                taskDatabase = JsonMapper.ToObject<TaskDatabase>(jsonResponse);

                // Check and log the data for debugging
                if (taskDatabase != null && taskDatabase.tasks != null)
                {
                    foreach (TaskData task in taskDatabase.tasks)
                    {
                        Debug.Log("Task ID: " + task.id + ", Name: " + task.name);
                        if (task.stations != null)
                        {
                            foreach (StationData station in task.stations)
                            {
                                Debug.Log("  Station ID: " + station.id + ", Name: " + station.name);
                                if (station.pages != null)
                                {
                                    foreach (PageData page in station.pages)
                                    {
                                        Debug.Log("    Page ID: " + page.id + ", Layout Template Index: " + page.layout_template_index);
                                        if (page.texts != null)
                                        {
                                            foreach (TextSegmentData text in page.texts)
                                            {
                                                Debug.Log("      Text ID: " + text.id + ", Content: " + text.content);
                                            }
                                        }
                                        if (page.images != null)
                                        {
                                            foreach (ImageData image in page.images)
                                            {
                                                Debug.Log("      Image ID: " + image.id + ", Order: " + image.order);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // Now that the data is loaded, visualize the tasks.
                    VisualizeTasks();
                }
                else
                {
                    Debug.LogError("Failed to parse task data with LitJson.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("LitJson parsing error: " + ex.Message);
            }
        }
    }

    public void ShowTaskPanel()
    {
        taskPanel.SetActive(true);
        stationPanel.SetActive(false);
        pagePanel.SetActive(false);
        pageEditor.SetActive(false);
        VisualizeTasks();
    }

    public void ShowStationPanel()
    {
        taskPanel.SetActive(false);
        stationPanel.SetActive(true);
        pagePanel.SetActive(false);
        pageEditor.SetActive(false);
        VisualizeStations(selectedTaskID);
    }

    public void ShowPagePanel()
    {
        taskPanel.SetActive(false);
        stationPanel.SetActive(false);
        pagePanel.SetActive(true);
        pageEditor.SetActive(false);
        VisualizePages(selectedTaskID, selectedStationID);
    }

    public void ShowPageEditor()
    {
        taskPanel.SetActive(false);
        stationPanel.SetActive(false);
        pagePanel.SetActive(false);
        pageEditor.SetActive(true);
        VisualizePageDetail(selectedTaskID, selectedStationID, selectedPageID);
    }

    public TaskData FindTaskById(int taskId)
    {
        if (taskDatabase != null && taskDatabase.tasks != null)
        {
            foreach (TaskData task in taskDatabase.tasks)
            {
                if (task.id == taskId)
                {
                    return task;
                }
            }
        }
        return null; // Return null if no matching task is found
    }

    public StationData FindStationById(int taskId, int stationId)
    {
        // First, find the task using the existing method
        TaskData task = FindTaskById(taskId);
        if (task != null && task.stations != null)
        {
            foreach (StationData station in task.stations)
            {
                if (station.id == stationId)
                {
                    return station;
                }
            }
        }
        // Return null if not found
        return null;
    }

    public PageData FindPageById(int taskId, int stationId, int pageId)
    {
        // First, find the task by its id.
        TaskData task = FindTaskById(taskId);
        if (task != null && task.stations != null)
        {
            // Now, find the station within the task.
            foreach (StationData station in task.stations)
            {
                if (station.id == stationId)
                {
                    if (station.pages != null)
                    {
                        // Finally, search for the page within the station.
                        foreach (PageData page in station.pages)
                        {
                            if (page.id == pageId)
                            {
                                return page;
                            }
                        }
                    }
                }
            }
        }
        // Return null if no matching page is found.
        return null;
    }


    private void VisualizeTasks()
    {
        foreach(var taskItem in taskItems)
        {
            Destroy(taskItem);
        }
        taskItems.Clear();

        for (int i = 0; i < taskDatabase.tasks.Length; i++)
        {
            GameObject taskItem = Instantiate(taskItemPrefab, taskItemContainer);
            taskItem.GetComponent<TaskItem>().SetItem(taskDatabase.tasks[i].id, taskDatabase.tasks[i].name);
            taskItems.Add(taskItem);
        }
    }

    private void VisualizeStations(int taskID)
    {
        foreach (var stationItem in stationItems)
        {
            Destroy(stationItem);
        }
        stationItems.Clear();

        TaskData task = FindTaskById(taskID);
        for (int i = 0; i < task.stations.Length; i++)
        {
            GameObject stationItem = Instantiate(stationItemPrefab, stationItemContainer);
            stationItem.GetComponent<StationItem>().SetItem(task.stations[i].id, task.name);
            stationItems.Add(stationItem);
        }
    }


    private void VisualizePages(int taskID, int stationID)
    {
        foreach (var pageItem in pageItems)
        {
            Destroy(pageItem);
        }
        pageItems.Clear();

        TaskData task = FindTaskById(taskID);
        StationData station = FindStationById(taskID, stationID);
        for (int i = 0; i < station.pages.Length; i++)
        {
            GameObject pageItem = Instantiate(pageItemPrefab, pageItemContainer);
            pageItem.GetComponent<PageItem>().SetItem(station.pages[i].id, task.name);
            pageItems.Add(pageItem);
        }
    }

    private void VisualizePageDetail(int taskID, int stationID, int pageID)
    {
        PageData page = FindPageById(taskID, stationID, pageID);
        pageIndexText.text = "Page Index: " + page.id;
        pageTemplateDropdown.value = page.layout_template_index;

        for(int i = 0; i < pagePreviewItems.Count; i++)
        {
            if(pagePreviewItems[i].layoutIndex == page.layout_template_index)
            {
                pagePreviewItems[i].gameObject.SetActive(true);
                foreach (var inputItem in inputItems)
                {
                    Destroy(inputItem);
                }
                inputItems.Clear();

                for (int j = 0; j < pagePreviewItems[i].texts.Count; j++)
                {
                    GameObject textInputItem = Instantiate(textInputItemPrefab, inputItemContainer);
                    textInputItem.GetComponent<TextInputItem>().textID = j;
                    inputItems.Add(textInputItem);

                    pagePreviewItems[i].texts[j].text = page.texts[j].content;
                }

                for (int j = 0; j < pagePreviewItems[i].images.Count; j++)
                {
                    GameObject imageInputItem = Instantiate(imageInputItemPrefab, inputItemContainer);
                    imageInputItem.GetComponent<ImageInputItem>().imageID = j;
                    inputItems.Add(imageInputItem);

                    //Visualize Images
                }
            }
            else
            {
                pagePreviewItems[i].gameObject.SetActive(false);
            }
        }
    }
}
