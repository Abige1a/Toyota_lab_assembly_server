using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;
using SFB;

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
    public GameObject newTaskPanel;
    public InputField taskNameInputField;
    public GameObject stationPanel;
    public GameObject copyTaskPanel;
    public InputField copyTaskNameInputField;
    public GameObject pagePanel;
    public GameObject pageEditor;
    public GameObject pageSaveSuccessPanel;

    public Text stationPanelTitle;
    public Text pagePanelTitle;
    public Text pageEditorTitle;

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

    public enum VisualizeOption
    {
        Tasks,
        Stations,
        Pages,
        PageEditor
    }

    public void FetchAndShowPages()
    {
        StartCoroutine(GetAllTasksCoroutine(VisualizeOption.Pages));
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
                        case VisualizeOption.Tasks:
                            ShowTaskPanel();
                            break;
                        case VisualizeOption.Stations:
                            ShowStationPanel();
                            break;
                        case VisualizeOption.Pages:
                            ShowPagePanel();
                            break;
                        case VisualizeOption.PageEditor:
                            ShowPageEditor();
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

    public void ShowTaskPanel()
    {
        taskPanel.SetActive(true);
        newTaskPanel.SetActive(false);
        stationPanel.SetActive(false);
        copyTaskPanel.SetActive(false);
        pagePanel.SetActive(false);
        pageEditor.SetActive(false);
        VisualizeTasks();
    }

    public void ShowCreateNewTaskPanel()
    {
        newTaskPanel.SetActive(true);
        taskNameInputField.text = "";
    }

    public void ShowStationPanel()
    {
        taskPanel.SetActive(false);
        newTaskPanel.SetActive(false);
        stationPanel.SetActive(true);
        copyTaskPanel.SetActive(false);
        pagePanel.SetActive(false);
        pageEditor.SetActive(false);
        pageSaveSuccessPanel.SetActive(false);
        VisualizeStations(selectedTaskID);
        stationPanelTitle.text = "Production " + selectedTaskID + ": " + FindTaskById(selectedTaskID).name;
    }

    public void ShowCopyTaskPanel()
    {
        copyTaskPanel.SetActive(true);
        copyTaskNameInputField.text = "";
    }

    public void ShowPagePanel()
    {
        taskPanel.SetActive(false);
        newTaskPanel.SetActive(false);
        stationPanel.SetActive(false);
        copyTaskPanel.SetActive(false);
        pagePanel.SetActive(true);
        pageEditor.SetActive(false);
        pageSaveSuccessPanel.SetActive(false);
        VisualizePages(selectedTaskID, selectedStationID);
        pagePanelTitle.text = "Production " + selectedTaskID + ": " + FindTaskById(selectedTaskID).name + " Station " + selectedStationID;
    }

    public void ShowPageEditor()
    {
        taskPanel.SetActive(false);
        newTaskPanel.SetActive(false);
        stationPanel.SetActive(false);
        copyTaskPanel.SetActive(false);
        pagePanel.SetActive(false);
        pageEditor.SetActive(true);
        pageSaveSuccessPanel.SetActive(false);
        VisualizePageDetail(selectedTaskID, selectedStationID, selectedPageID);
        pageEditorTitle.text = "Production " + selectedTaskID + ": " + FindTaskById(selectedTaskID).name + " Station " + selectedStationID;
    }

    public void ShowPageSaveSuccessPanel()
    {
        pageSaveSuccessPanel.SetActive(true);
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
                    TextInputItem ti = textInputItem.GetComponent<TextInputItem>();
                    // Assume that textID is 1-indexed. Here, we pass j (0-indexed) to our bind function.
                    ti.textID = j;
                    ti.title.text = "Paragraph " + (j + 1) + ":";
                    ti.textInputField.text = page.texts[j].content;

                    // Bind the onValueChanged event so changes update the page data.
                    BindTextInputItem(ti, j);

                    inputItems.Add(textInputItem);

                    // Also update the preview text if needed.
                    pagePreviewItems[i].texts[j].text = page.texts[j].content;
                }

                for (int j = 0; j < pagePreviewItems[i].images.Count; j++)
                {
                    GameObject imageInputItem = Instantiate(imageInputItemPrefab, inputItemContainer);
                    ImageInputItem ii = imageInputItem.GetComponent<ImageInputItem>();
                    ii.imageID = j + 1;
                    ii.title.text = "Image " + (j + 1) + ":";
                    // Bind the image selection functionality.
                    BindImageInputItem(ii, j);
                    inputItems.Add(imageInputItem);
                    // Optionally, update the preview image if the page data already has image data.
                    if (page.images != null && j < page.images.Length && !string.IsNullOrEmpty(page.images[j].data))
                    {
                        byte[] imgBytes = System.Convert.FromBase64String(page.images[j].data);
                        UpdatePreviewImage(j, imgBytes);
                    }
                }
            }
            else
            {
                pagePreviewItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void BindTextInputItem(TextInputItem textItem, int textIndex)
    {
        textItem.textInputField.onValueChanged.AddListener((string newValue) =>
        {
            // Update the current page data.
            PageData page = FindPageById(selectedTaskID, selectedStationID, selectedPageID);
            if (page != null && page.texts != null && textIndex < page.texts.Length)
            {
                page.texts[textIndex].content = newValue;
                Debug.Log("Updated page text at index " + textIndex + " to: " + newValue);
            }

            // Also update the preview.
            if (page != null)
            {
                // Find the active PagePreviewItem for the current page's layout template.
                foreach (PagePreviewItem preview in pagePreviewItems)
                {
                    if (preview.layoutIndex == page.layout_template_index)
                    {
                        if (textIndex < preview.texts.Count)
                        {
                            preview.texts[textIndex].text = newValue;
                            Debug.Log("Updated preview text at index " + textIndex + " to: " + newValue);
                        }
                        break;
                    }
                }
            }
        });
    }

    public void BindImageInputItem(ImageInputItem imageItem, int imageIndex)
    {
        imageItem.imageSelectionButton.onClick.AddListener(() =>
        {
            // Open a file dialog to choose an image file (png, jpg, jpeg)
            var extensions = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select an Image", "", extensions, false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string filePath = paths[0];
                try
                {
                    // Read the image file as bytes and convert to base64.
                    byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                    string base64Data = System.Convert.ToBase64String(fileData);

                    // Update the page data.
                    PageData page = FindPageById(selectedTaskID, selectedStationID, selectedPageID);
                    if (page != null && page.images != null && imageIndex < page.images.Length)
                    {
                        page.images[imageIndex].data = base64Data;
                        Debug.Log("Updated page image at index " + imageIndex);
                        // Update the preview RawImage.
                        UpdatePreviewImage(imageIndex, fileData);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error reading image file: " + ex.Message);
                }
            }
        });
    }


    public void UpdatePreviewImage(int imageIndex, byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            // Find the active PagePreviewItem for the current page's layout template.
            PageData currentPage = FindPageById(selectedTaskID, selectedStationID, selectedPageID);
            if (currentPage != null)
            {
                foreach (PagePreviewItem preview in pagePreviewItems)
                {
                    if (preview.layoutIndex == currentPage.layout_template_index)
                    {
                        if (imageIndex < preview.images.Count)
                        {
                            preview.images[imageIndex].texture = texture;
                            Debug.Log("Preview RawImage updated at index " + imageIndex);
                        }
                        break;
                    }
                }
            }
        }
    }

    public void CreateTask()
    {
        string newTaskName = taskNameInputField.text;
        if (string.IsNullOrEmpty(newTaskName))
        {
            Debug.LogError("Task name is empty!");
            return;
        }
        StartCoroutine(CreateTaskCoroutine(newTaskName));
    }

    private IEnumerator CreateTaskCoroutine(string newTaskName)
    {
        string url = baseUrl + "/tasks";
        string jsonData = "{\"name\":\"" + newTaskName + "\"}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error creating task: " + request.error);
        }
        else
        {
            Debug.Log("Task created: " + request.downloadHandler.text);
            // Refresh tasks to update UI after creation.
            FetchAllTasks(VisualizeOption.Tasks);
        }
    }

    public void DeleteTask()
    {
        if (selectedTaskID <= 0)
        {
            Debug.LogError("No valid task selected for deletion.");
            return;
        }
        StartCoroutine(DeleteTaskCoroutine(selectedTaskID));
    }

    private IEnumerator DeleteTaskCoroutine(int taskId)
    {
        string url = baseUrl + "/tasks/" + taskId;
        UnityWebRequest request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error deleting task: " + request.error);
        }
        else
        {
            Debug.Log("Task deleted: " + FindTaskById(taskId).name);
            // Refresh tasks to update UI after deletion.
            FetchAllTasks(VisualizeOption.Tasks);
        }
    }

    public void CreateStation()
    {
        if (selectedTaskID <= 0)
        {
            Debug.LogError("No valid task selected.");
            return;
        }
        string newStationName = ""; // Leave station name empty.
        StartCoroutine(CreateStationCoroutine(selectedTaskID, newStationName));
    }

    private IEnumerator CreateStationCoroutine(int taskId, string newStationName)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations";
        string jsonData = "{\"name\":\"" + newStationName + "\"}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error creating station: " + request.error);
        }
        else
        {
            string responseText = (request.downloadHandler != null) ? request.downloadHandler.text : "No response";
            Debug.Log("Station created: " + responseText);
            // Refresh the station view for the selected task.
            FetchAllTasks(VisualizeOption.Stations);
        }
    }

    public void DeleteStation()
    {
        if (selectedTaskID <= 0 || selectedStationID <= 0)
        {
            Debug.LogError("No valid task or station selected for deletion.");
            return;
        }
        StartCoroutine(DeleteStationCoroutine(selectedTaskID, selectedStationID));
    }

    private IEnumerator DeleteStationCoroutine(int taskId, int stationId)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations/" + stationId;
        UnityWebRequest request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error deleting station: " + request.error);
        }
        else
        {
            string responseText = (request.downloadHandler != null) ? request.downloadHandler.text : "No response";
            Debug.Log("Station deleted: " + responseText);
            // Refresh the station view for the selected task.
            FetchAllTasks(VisualizeOption.Stations);
        }
    }

    public void CreatePage()
    {
        if (selectedTaskID <= 0 || selectedStationID <= 0)
        {
            Debug.LogError("No valid task or station selected.");
            return;
        }
        int layoutTemplateIndex = 0; // Default value.
        StartCoroutine(CreatePageCoroutine(selectedTaskID, selectedStationID, layoutTemplateIndex));
    }

    private IEnumerator CreatePageCoroutine(int taskId, int stationId, int layoutTemplateIndex)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations/" + stationId + "/pages";
        // Build JSON data to include a default texts array with two empty text segments.
        string jsonData = "{\"layout_template_index\": " + layoutTemplateIndex +
                          ", \"texts\": [" +
                              "{\"id\": 1, \"content\": \"\", \"order\": 0}," +
                              "{\"id\": 2, \"content\": \"\", \"order\": 1}" +
                          "], \"images\": []}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error creating page: " + request.error);
        }
        else
        {
            string responseText = (request.downloadHandler != null) ? request.downloadHandler.text : "No response";
            Debug.Log("Page created: " + responseText);
            // Refresh the page view for the selected task and station after the data is synchronized.
            FetchAllTasks(VisualizeOption.Pages);
        }
    }

    public void DeletePage()
    {
        if (selectedTaskID <= 0 || selectedStationID <= 0 || selectedPageID <= 0)
        {
            Debug.LogError("No valid task, station, or page selected for deletion.");
            return;
        }
        StartCoroutine(DeletePageCoroutine(selectedTaskID, selectedStationID, selectedPageID));
    }

    private IEnumerator DeletePageCoroutine(int taskId, int stationId, int pageId)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations/" + stationId + "/pages/" + pageId;
        UnityWebRequest request = UnityWebRequest.Delete(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error deleting page: " + request.error);
        }
        else
        {
            string responseText = (request.downloadHandler != null) ? request.downloadHandler.text : "No response";
            Debug.Log("Page deleted: " + responseText);
            // Refresh the page view for the selected task and station.
            FetchAllTasks(VisualizeOption.Pages);
        }
    }

    public void SwitchPageTemplate()
    {
        int newTemplateIndex = pageTemplateDropdown.value;
        if (newTemplateIndex < 0 || newTemplateIndex >= pagePreviewItems.Count)
        {
            Debug.LogError("Invalid page template index.");
            return;
        }

        // Get the preview counts for texts and images from the selected template.
        PagePreviewItem preview = pagePreviewItems[newTemplateIndex];
        int newTextsCount = preview.texts.Count;
        int newImagesCount = preview.images.Count;

        PageData currentPage = FindPageById(selectedTaskID, selectedStationID, selectedPageID);
        if (currentPage == null)
        {
            Debug.LogError("Current page not found in taskDatabase.");
            return;
        }

        // Update the layout template index.
        currentPage.layout_template_index = newTemplateIndex;

        // Inherit old texts if available.
        int oldTextsCount = (currentPage.texts != null) ? currentPage.texts.Length : 0;
        TextSegmentData[] newTexts = new TextSegmentData[newTextsCount];
        for (int i = 0; i < newTextsCount; i++)
        {
            newTexts[i] = new TextSegmentData();
            newTexts[i].id = i + 1;  // IDs start at 1
            newTexts[i].order = i;
            if (i < oldTextsCount)
            {
                // Inherit the content from the old text.
                newTexts[i].content = currentPage.texts[i].content;
            }
            else
            {
                newTexts[i].content = "";
            }
        }
        currentPage.texts = newTexts;

        // Inherit old images if available.
        int oldImagesCount = (currentPage.images != null) ? currentPage.images.Length : 0;
        ImageData[] newImages = new ImageData[newImagesCount];
        for (int i = 0; i < newImagesCount; i++)
        {
            newImages[i] = new ImageData();
            newImages[i].id = i + 1;
            newImages[i].order = i;
            if (i < oldImagesCount)
            {
                newImages[i].data = currentPage.images[i].data;
            }
            else
            {
                newImages[i].data = "";
            }
        }
        currentPage.images = newImages;

        VisualizePageDetail(selectedTaskID, selectedStationID, selectedPageID);
        Debug.Log("Switched page template to " + newTemplateIndex +
                  " (texts: inherited " + Mathf.Min(oldTextsCount, newTextsCount) + " of " + newTextsCount +
                  ", images: inherited " + Mathf.Min(oldImagesCount, newImagesCount) + " of " + newImagesCount + ").");
    }

    public void SavePage()
    {
        PageData currentPage = FindPageById(selectedTaskID, selectedStationID, selectedPageID);
        if (currentPage == null)
        {
            Debug.LogError("Current page not found for saving.");
            return;
        }
        StartCoroutine(SavePageCoroutine(selectedTaskID, selectedStationID, selectedPageID, currentPage));
    }

    private IEnumerator SavePageCoroutine(int taskId, int stationId, int pageId, PageData page)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations/" + stationId + "/pages/" + pageId;
        // Convert the current page to JSON using LitJson.
        string jsonData = JsonMapper.ToJson(page);

        UnityWebRequest request = UnityWebRequest.Put(url, jsonData);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error saving page: " + request.error);
        }
        else
        {
            string responseText = (request.downloadHandler != null) ? request.downloadHandler.text : "No response";
            Debug.Log("Page saved successfully: " + responseText);
            //FetchAllTasks(VisualizeOption.Pages);
            ShowPageSaveSuccessPanel();
        }
    }

    public void CopyTask()
    {
        if(copyTaskNameInputField.text == "")
        {
            return;
        }
        StartCoroutine(CopyTaskCoroutine(selectedTaskID, copyTaskNameInputField.text));
    }

    private IEnumerator CopyTaskCoroutine(int taskId, string newTaskName)
    {
        // Build the URL using the provided taskId.
        string url = baseUrl + "/tasks/" + taskId + "/copy";
        // Build JSON payload with the new task name.
        string jsonData = "{\"name\":\"" + newTaskName + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error copying task: " + request.error);
        }
        else
        {
            Debug.Log("Task copied successfully: " + request.downloadHandler.text);
            FetchAllTasks(VisualizeOption.Tasks);
        }
    }

}
