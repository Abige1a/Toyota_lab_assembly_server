using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerAPI : MonoBehaviour
{
    // Change this base URL if your server is hosted elsewhere
    private string baseUrl = "http://localhost:5000";

    // ---------------------- Task Methods ----------------------

    // GET /tasks - Retrieve all tasks
    public IEnumerator GetTasks(System.Action<string> callback)
    {
        string url = baseUrl + "/tasks";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // POST /tasks - Create a new Task
    public IEnumerator CreateTask(string taskName, System.Action<string> callback)
    {
        string url = baseUrl + "/tasks";
        string jsonData = "{\"name\":\"" + taskName + "\"}";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // DELETE /tasks/{taskId} - Delete a Task
    public IEnumerator DeleteTask(int taskId, System.Action<string> callback)
    {
        string url = baseUrl + "/tasks/" + taskId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // ---------------------- Station Methods ----------------------

    // POST /tasks/{taskId}/stations - Create a Station under a Task
    public IEnumerator CreateStation(int taskId, string stationName, System.Action<string> callback)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations";
        string jsonData = "{\"name\":\"" + stationName + "\"}";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // DELETE /tasks/{taskId}/stations/{stationId} - Delete a Station under a Task
    public IEnumerator DeleteStation(int taskId, int stationId, System.Action<string> callback)
    {
        string url = baseUrl + "/tasks/" + taskId + "/stations/" + stationId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // ---------------------- Page Methods ----------------------

    // POST /stations/{stationId}/pages - Create a Page under a Station
    public IEnumerator CreatePage(int stationId, int layoutTemplateIndex, System.Action<string> callback)
    {
        string url = baseUrl + "/stations/" + stationId + "/pages";
        string jsonData = "{\"layout_template_index\":" + layoutTemplateIndex + "}";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // DELETE /stations/{stationId}/pages/{pageId} - Delete a Page under a Station
    public IEnumerator DeletePage(int stationId, int pageId, System.Action<string> callback)
    {
        string url = baseUrl + "/stations/" + stationId + "/pages/" + pageId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // ---------------------- Page Text Methods ----------------------

    // POST /pages/{pageId}/texts - Add a text segment to a Page
    public IEnumerator AddText(int pageId, string content, int order, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/texts";
        string jsonData = "{\"content\":\"" + content + "\", \"order\":" + order + "}";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // PUT /pages/{pageId}/texts/{textId} - Update a text segment in a Page
    public IEnumerator UpdateText(int pageId, int textId, string content, int order, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/texts/" + textId;
        string jsonData = "{\"content\":\"" + content + "\", \"order\":" + order + "}";
        using (UnityWebRequest www = UnityWebRequest.Put(url, jsonData))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // DELETE /pages/{pageId}/texts/{textId} - Delete a text segment in a Page
    public IEnumerator DeleteText(int pageId, int textId, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/texts/" + textId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // ---------------------- Page Image Methods ----------------------

    // POST /pages/{pageId}/images - Add an image segment to a Page
    public IEnumerator AddImage(int pageId, string data, int order, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/images";
        string jsonData = "{\"data\":\"" + data + "\", \"order\":" + order + "}";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // PUT /pages/{pageId}/images/{imageId} - Update an image segment in a Page
    public IEnumerator UpdateImage(int pageId, int imageId, string data, int order, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/images/" + imageId;
        string jsonData = "{\"data\":\"" + data + "\", \"order\":" + order + "}";
        using (UnityWebRequest www = UnityWebRequest.Put(url, jsonData))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // DELETE /pages/{pageId}/images/{imageId} - Delete an image segment in a Page
    public IEnumerator DeleteImage(int pageId, int imageId, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId + "/images/" + imageId;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }

    // ---------------------- Page Layout Method ----------------------

    // PUT /pages/{pageId} - Update layout_template_index of a Page
    public IEnumerator UpdatePageLayout(int pageId, int layoutTemplateIndex, System.Action<string> callback)
    {
        string url = baseUrl + "/pages/" + pageId;
        string jsonData = "{\"layout_template_index\":" + layoutTemplateIndex + "}";
        using (UnityWebRequest www = UnityWebRequest.Put(url, jsonData))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError(www.error);
            else
                callback(www.downloadHandler.text);
        }
    }
}
