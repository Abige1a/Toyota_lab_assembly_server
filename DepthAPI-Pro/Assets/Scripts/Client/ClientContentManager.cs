using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;
using SFB;

public class ClientContentManager : MonoBehaviour
{
    public static ClientContentManager instance;

    public ServerAPI api;

    public bool isAdmin = false;
    public GameObject taskItemPrefab;
    public GameObject stationItemPrefab;
    public List<GameObject> pagePrefabs = new List<GameObject>();

    public Transform taskItemContainer;
    public Transform stationItemContainer;
    public Transform pageItemContainer;

    public GameObject loginPanel;
    public GameObject taskPanel;
    public GameObject stationPanel;
    public GameObject pagePanel;
    public Text pageNumberText;
    public GameObject pagePreviousButton;
    public GameObject pageNextButton;
    public GameObject pageFinishButton;
    public GameObject nextStepPanel;

    public Text stationPanelTitle;
    public Text pagePanelTitle;

    private List<GameObject> taskItems = new List<GameObject>();
    private List<GameObject> stationItems = new List<GameObject>();
    private List<GameObject> pageItems = new List<GameObject>();

    [HideInInspector] public TaskDatabase taskDatabase = new TaskDatabase();
    [HideInInspector] public int selectedTaskID;
    [HideInInspector] public int selectedStationID;
    [HideInInspector] public int selectedPageID;



    [SerializeField] private string baseUrl = "http://127.0.0.1:5000";

    public enum VisualizeOption
    {
        Login,
        Tasks,
        Stations,
        Pages
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public void SetAdmin()
    {
        isAdmin = true;
    }

    public void SetUser()
    {
        isAdmin = false;
    }

    public void FetchAllTasks()
    {
        StartCoroutine(GetAllTasksCoroutine(VisualizeOption.Tasks));
    }

    public void FetchAllTasks(VisualizeOption option = VisualizeOption.Tasks)
    {
        StartCoroutine(GetAllTasksCoroutine(option));
    }

    private IEnumerator GetAllTasksCoroutine(VisualizeOption option)
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
                    // Visualize the appropriate panel after data synchronization.
                    switch (option)
                    {
                        case VisualizeOption.Login:
                            ShowLoginPanel();
                            break;
                        case VisualizeOption.Tasks:
                            ShowTaskPanel();
                            break;
                        case VisualizeOption.Stations:
                            ShowStationPanel();
                            break;
                        case VisualizeOption.Pages:
                            ShowPagePanel();
                            break;
                        default:
                            ShowTaskPanel();
                            break;
                    }
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

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        taskPanel.SetActive(false);
        stationPanel.SetActive(false);
        pagePanel.SetActive(false);
        nextStepPanel.SetActive(false);
    }

    public void ShowTaskPanel()
    {
        loginPanel.SetActive(false);
        taskPanel.SetActive(true);
        stationPanel.SetActive(false);
        pagePanel.SetActive(false);
        nextStepPanel.SetActive(false);
        VisualizeTasks();
    }

    public void ShowStationPanel()
    {
        loginPanel.SetActive(false);
        taskPanel.SetActive(false);
        stationPanel.SetActive(true);
        pagePanel.SetActive(false);
        nextStepPanel.SetActive(false);
        VisualizeStations(selectedTaskID);
        stationPanelTitle.text = "Production " + selectedTaskID + ": " + FindTaskById(selectedTaskID).name;
    }

    public void ShowPagePanel()
    {
        loginPanel.SetActive(false);
        taskPanel.SetActive(false);
        stationPanel.SetActive(false);
        pagePanel.SetActive(true);
        nextStepPanel.SetActive(false);
        VisualizePages(selectedTaskID, selectedStationID);
        pagePanelTitle.text = "Production " + selectedTaskID + ": " + FindTaskById(selectedTaskID).name + " Station " + selectedStationID;
    }

    public void ShowNextStepPanel()
    {

        loginPanel.SetActive(false);
        taskPanel.SetActive(false);
        stationPanel.SetActive(false);
        pagePanel.SetActive(false);
        nextStepPanel.SetActive(true);
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
        foreach (var taskItem in taskItems)
        {
            Destroy(taskItem);
        }
        taskItems.Clear();

        for (int i = 0; i < taskDatabase.tasks.Length; i++)
        {
            GameObject taskItem = Instantiate(taskItemPrefab, taskItemContainer);
            taskItem.GetComponent<UserTaskItem>().SetItem(taskDatabase.tasks[i].id, taskDatabase.tasks[i].name);
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
            stationItem.GetComponent<UserStationItem>().SetItem(task.stations[i].id, task.name);
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
            PageData page = station.pages[i];
            Debug.LogWarning("Layout " + page.layout_template_index);
            GameObject pageItem = Instantiate(pagePrefabs[page.layout_template_index], pageItemContainer);
            for (int j = 0; j < pageItem.GetComponent<PagePreviewItem>().texts.Count; j++)
            {
                pageItem.GetComponent<PagePreviewItem>().texts[j].text = page.texts[j].content;
            }

            for (int j = 0; j < pageItem.GetComponent<PagePreviewItem>().images.Count; j++)
            {
                if (page.images != null && j < page.images.Length && !string.IsNullOrEmpty(page.images[j].data))
                {
                    byte[] imgBytes = System.Convert.FromBase64String(page.images[j].data);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(imgBytes))
                    {
                        pageItem.GetComponent<PagePreviewItem>().images[j].texture = texture;
                    }
                }
            }
            pageItems.Add(pageItem);
        }

        ShowPage(0);
    }

    public void ShowPage(int index)
    {
        if(pageItems.Count == 0)
        {
            pagePreviousButton.SetActive(false);
            pageNextButton.SetActive(false);
            pageFinishButton.SetActive(true);
            pageNumberText.text = "0 / 0";
            return;
        }

        for(int i = 0; i < pageItems.Count; i++)
        {
            if(index == i)
            {
                pageItems[i].SetActive(true);
            }
            else
            {
                pageItems[i].SetActive(false);
            }
        }

        selectedPageID = index;
        pageNumberText.text = (selectedPageID + 1).ToString() + " / " + pageItems.Count.ToString();
        if(selectedPageID == 0)
        {
            pagePreviousButton.SetActive(false);
        }
        else
        {
            pagePreviousButton.SetActive(true);
        }
        if(selectedPageID == pageItems.Count - 1)
        {

            pageNextButton.SetActive(false);
            pageFinishButton.SetActive(true);
        }
        else
        {
            pageNextButton.SetActive(true);
            pageFinishButton.SetActive(false);
        }
    }

    public void ShowPreviousPage()
    {
        if(selectedPageID > 0)
        {
            ShowPage(selectedPageID - 1);
        }
    }

    public void ShowNextPage()
    {
        if (selectedPageID < pageItems.Count - 1)
        {
            ShowPage(selectedPageID + 1);
        }
    }
}
