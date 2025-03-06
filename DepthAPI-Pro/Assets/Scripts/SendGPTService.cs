using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;
using RosMessageTypes.XarmMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System;
using OpenAI;

public class ChatRecord
{
    public string content;
    public int type = 0; //0 = me, 1 = gpt
    public float time = 0.0f;
}

public class SendGPTService : MonoBehaviour
{

    ROSConnection ros;
    public TTSManager ttsObject;

    public string serviceName = "DirectInstruction_server";
    float awaitingResponseUntilTimestamp = -1;

    public float sendInterval = 1.0f;
    public Text responseText;
    public RectTransform responseContent;
    public RectTransform responseBackground;
    public Text highLevelPlanText;
    public RectTransform highLevelPlanContent;
    public RectTransform highLevelPlanBackground;

    public GameObject posIndicatorPrefab;
    public GameObject objIndicatorPrefab;
    public GameObject alignmentIndicatorPrefab;

    public RecenterHelper recenterHelper;
    public UnityDocumentSaveService saveService;

    [HideInInspector] public byte[] textureData;
    [HideInInspector] public int textureWidth;
    [HideInInspector] public int textureHeight;

    private float timer = 0;

    private List<ChatRecord> chatHistory = new List<ChatRecord>();

    private int stage = 1;
    private int step = 1;

    private List<GameObject> posIndicators = new List<GameObject>();

    private Vector3 lastSystemPosition;
    private Quaternion lastSystemRotation;

    private GameObject selectObject;

    private List<string> systemPositionsRecord = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<UnityInformationRequest, UnityInformationResponse>(serviceName);
        timer = 0;
        //call_service();
        //call_service_alignment();
    }

    // Update is called once per frame
    void Update()
    {
        //call_service();
        //timer += Time.deltaTime;
        //if(timer >= sendInterval)
        //{
        //    timer = 0;
        //    call_service();
        //}

        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
        {
            //OVRManager.display.RecenterPose();
            //UnityEngine.XR.InputTracking.Recenter();
            //OVRPlugin.RecenterTrackingOrigin(OVRPlugin.RecenterFlags.Default);
            recenterHelper.Recenter();
        }
    }

    public void call_service(string content)
    {
        string system_content = "help me to analyze the instructions about moving and rotating.";
        string user_content = content;
        ChatRecord rec = new ChatRecord();
        rec.content = content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[1];
        request.mode[0] = 0;
        request.system_content = system_content;
        request.user_content = user_content;
        //request.prompt_image = new UnityImageMsg();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;


    }
    void Callback_service(UnityInformationResponse response)
    {
        awaitingResponseUntilTimestamp = -1;

        Debug.Log("GPT server said " + response.question_text);
        ChatRecord rec = new ChatRecord();
        rec.content = response.question_text;
        rec.type = 1;
        rec.time = Time.time;
        chatHistory.Add(rec);
        ttsObject.SynthesizeAndPlay(response.question_text);
        UpdateChatHistory();
        //reponseText.text = response.text;
    }

    public void call_service_stage(string content)
    {
        string system_content = "";
        string user_content = content;
        ChatRecord rec = new ChatRecord();
        rec.content = content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = stage;
        request.mode[1] = step;
        request.system_content = system_content;
        request.user_content = user_content;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_stage_verify(int verify)
    {
        string system_content = "";
        string user_content = verify == 0 ? "Not correct." : "Correct.";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = stage;
        request.mode[1] = step;
        request.system_content = system_content;
        request.user_content = user_content;
        request.user_action_verify = verify;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_stage_locate(Transform location)
    {
        string system_content = "";
        PointMsg newLocation = location.position.To<FLU>();
        string user_content = "";
        if(stage == 2)
        {
            user_content += "Not correct. ";
            //generate a positions record
            string posRecord = lastSystemPosition.x + ", " + lastSystemPosition.y + ", " + lastSystemPosition.z + ", "
                + lastSystemRotation.x + ", " + lastSystemRotation.y + ", " + lastSystemRotation.z + ", " + lastSystemRotation.w + ", "
                + location.position.x + ", " + location.position.y + ", " + location.position.z + ", "
                + location.rotation.x + ", " + location.rotation.y + ", " + location.rotation.z + ", " + location.rotation.w + ",\n";
            systemPositionsRecord.Add(posRecord);
        }
        user_content += "Location: (" + newLocation.x.ToString("F3") + ", " + newLocation.y.ToString("F3") + ", " + newLocation.z.ToString("F3") + ")";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = stage;
        request.mode[1] = step;
        request.system_content = system_content;
        request.user_content = user_content;
        request.user_pose = new PoseMsg[1];
        request.user_pose[0] = new PoseMsg();
        request.user_pose[0].position = location.position.To<FLU>();
        request.user_pose[0].orientation = (location.rotation * Quaternion.Euler(0, 180, 180)).To<FLU>();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }
    public void call_service_stage_points(List<GameObject> points)
    {
        string system_content = "";
        string user_content = "Not Correct. Detect point at ";
        if(points.Count > 0)
        {
            user_content += "[" + (int)(points[0].GetComponent<RectTransform>().anchoredPosition.x * 10) + ", " + (int)(-points[0].GetComponent<RectTransform>().anchoredPosition.y * 10) + "]";
        }
        else
        {
            Debug.LogError("There is no point!");
            return;
        }
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = stage;
        request.mode[1] = step;
        request.system_content = system_content;
        request.user_content = user_content;
        
        request.points = new int[2];
        request.points[0] = (int)(1280 - points[0].GetComponent<RectTransform>().anchoredPosition.x * 10);
        request.points[1] = (int)(720 + points[0].GetComponent<RectTransform>().anchoredPosition.y * 10);
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }
    public void call_service_stage_boxes(List<GameObject> boxes)
    {
        string system_content = "";
        string user_content = "Detect box at ";
        if (boxes.Count > 0)
        {
            user_content += "[" + (int)(boxes[0].GetComponent<RectTransform>().anchoredPosition.x * 10) + ", " + (int)(-boxes[0].GetComponent<RectTransform>().anchoredPosition.y * 10) + "], ";
            user_content += "Width: " + (int)boxes[0].GetComponent<RectTransform>().sizeDelta.x + ", Height: " + (int)boxes[0].GetComponent<RectTransform>().sizeDelta.y;
        }
        else
        {
            Debug.LogError("There is no box!");
            return;
        }
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = stage;
        request.mode[1] = step;
        request.system_content = system_content;
        request.user_content = user_content;
        
        request.boxes = new int[4];
        request.boxes[0] = (int)(1280 - boxes[0].GetComponent<RectTransform>().anchoredPosition.x * 10 - (int)(boxes[0].GetComponent<RectTransform>().sizeDelta.x/2));
        request.boxes[1] = (int)(720 + boxes[0].GetComponent<RectTransform>().anchoredPosition.y * 10 - (int)(boxes[0].GetComponent<RectTransform>().sizeDelta.y/2));
        request.boxes[2] = (int)(1280 - boxes[0].GetComponent<RectTransform>().anchoredPosition.x * 10 + (int)(boxes[0].GetComponent<RectTransform>().sizeDelta.x/2));
        request.boxes[3] = (int)(720 + boxes[0].GetComponent<RectTransform>().anchoredPosition.y * 10 + (int)(boxes[0].GetComponent<RectTransform>().sizeDelta.y/2));
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }
    void Callback_service_stage(UnityInformationResponse response)
    {
        awaitingResponseUntilTimestamp = -1;
        for(int i = 0; i < posIndicators.Count; i++)
        {
            Destroy(posIndicators[i]);
        }
        posIndicators.Clear();

        step = response.mode;
        if(response.highlevel_plan.Length != 0 && stage == 1)
        {
            //End stage 1
            UpdateHighLevelPlan(response.highlevel_plan);
            string plan = "High Level Plan: \n";
            for (int i = 0; i < response.highlevel_plan.Length; i++)
            {
                plan += (i + 1).ToString() + ". " + response.highlevel_plan[i].action + " "
                    + response.highlevel_plan[i].object_name + ": "
                    + "(" + response.highlevel_plan[i].object_location.position.x.ToString("F3") + ", " + response.highlevel_plan[i].object_location.position.y.ToString("F3") + ", " + response.highlevel_plan[i].object_location.position.z.ToString("F3") + ")"
                    + " to " + "(" + response.highlevel_plan[i].place_location.position.x.ToString("F3") + ", " + response.highlevel_plan[i].place_location.position.y.ToString("F3") + ", " + response.highlevel_plan[i].place_location.position.z.ToString("F3") + ")\n";
            }
            call_service_stage1_to_stage2(plan);
            Debug.Log("GPT server said " + response.question_text);
            ChatRecord rec = new ChatRecord();
            rec.content = response.question_text;
            rec.type = 1;
            rec.time = Time.time;
            ttsObject.SynthesizeAndPlay(response.question_text);
            chatHistory.Add(rec);
            UpdateChatHistory();
        }
        // else if(response.question_text.ToLower().Contains("plan") && stage == 1)
        // {
        //     highLevelPlanText.text = "High Level Plan: \n";
        //     highLevelPlanText.text += response.question_text;
        //     float targetHeight = highLevelPlanText.preferredHeight > highLevelPlanBackground.sizeDelta.y ? highLevelPlanText.preferredHeight : highLevelPlanBackground.sizeDelta.y;
        //     Vector2 size = new Vector2(highLevelPlanText.GetComponent<RectTransform>().sizeDelta.x, targetHeight);
        //     highLevelPlanText.GetComponent<RectTransform>().sizeDelta = size;
        //     highLevelPlanContent.sizeDelta = new Vector2(highLevelPlanContent.sizeDelta.x, targetHeight);
        //     highLevelPlanText.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        // }
        else
        {
            if(response.server_poses.Length > 0) { 
                if(response.server_verify == 0)
                {
                    //single object, spawn gripper
                    GameObject go = Instantiate(posIndicatorPrefab, Vec3FLUToRUF((float)response.server_poses[0].position.x, (float)response.server_poses[0].position.y, (float)response.server_poses[0].position.z),
                    QuaternionFLUtoRUF(new Quaternion((float)response.server_poses[0].orientation.x, (float)response.server_poses[0].orientation.y, (float)response.server_poses[0].orientation.z, (float)response.server_poses[0].orientation.w)));
                    posIndicators.Add(go);
                    selectObject = go;
                    lastSystemPosition = go.transform.position;
                    lastSystemRotation = go.transform.rotation;
                }
                if (response.server_verify == 1)
                {
                    //detect, spawn balls
                    for (int i = 0; i < response.server_poses.Length; i++)
                    {
                        GameObject go = Instantiate(objIndicatorPrefab, Vec3FLUToRUF((float)response.server_poses[i].position.x, (float)response.server_poses[i].position.y, (float)response.server_poses[i].position.z),
                    QuaternionFLUtoRUF(new Quaternion((float)response.server_poses[i].orientation.x, (float)response.server_poses[i].orientation.y, (float)response.server_poses[i].orientation.z, (float)response.server_poses[i].orientation.w)));
                        go.GetComponent<SeletableObject>().index = posIndicators.Count;
                        posIndicators.Add(go);
                    }
                }
            }

            //Ask question
            Debug.Log("GPT server said " + response.question_text);
            ChatRecord rec = new ChatRecord();
            rec.content = response.question_text;
            rec.type = 1;
            rec.time = Time.time;
            ttsObject.SynthesizeAndPlay(response.question_text);
            chatHistory.Add(rec);
            UpdateChatHistory();
        }
        //reponseText.text = response.text;
    }

    public void call_service_stage1_to_stage2(string plan)
    {
        string system_content = "";
        string user_content = plan;
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 1;
        request.mode[1] = 2;
        stage = 2;
        step = 1;
        request.system_content = system_content;
        request.user_content = user_content;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_stage1_to_stage2);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    void Callback_service_stage1_to_stage2(UnityInformationResponse response)
    {
        awaitingResponseUntilTimestamp = -1;

        Debug.Log("GPT server said " + response.question_text);
        ChatRecord rec = new ChatRecord();
        rec.content = response.question_text;
        rec.type = 1;
        rec.time = Time.time;
        chatHistory.Add(rec);
        ttsObject.SynthesizeAndPlay(response.question_text);
        UpdateChatHistory();
        //reponseText.text = response.text;
    }

    public void call_service_alignment()
    {
        string system_content = "";
        string user_content = "Request Alignment Object.";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 3;
        request.mode[1] = 0;
        request.system_content = system_content;
        request.user_content = user_content;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_alignment);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_alignment_points(List<GameObject> points)
    {
        string system_content = "";
        string user_content = "Detect point at ";
        if (points.Count > 0)
        {
            user_content += "[" + (int)(points[0].GetComponent<RectTransform>().anchoredPosition.x * 10) + ", " + (int)(-points[0].GetComponent<RectTransform>().anchoredPosition.y * 10) + "]";
        }
        else
        {
            Debug.LogError("There is no point!");
            return;
        }
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 3;
        request.mode[1] = 0;
        request.system_content = system_content;
        request.user_content = user_content;
        request.points = new int[2];
        request.points[0] = (int)(1280 - points[0].GetComponent<RectTransform>().anchoredPosition.x * 10);
        request.points[1] = (int)(720 + points[0].GetComponent<RectTransform>().anchoredPosition.y * 10);
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_alignment);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    void Callback_service_alignment(UnityInformationResponse response)
    {
        awaitingResponseUntilTimestamp = -1;
        for(int i = 0; i < posIndicators.Count; i++)
        {
            Destroy(posIndicators[i]);
        }
        posIndicators.Clear();
        if (response.server_poses.Length > 0)
        {
            GameObject go = Instantiate(alignmentIndicatorPrefab, Vec3FLUToRUF((float)response.server_poses[0].position.x, (float)response.server_poses[0].position.y, (float)response.server_poses[0].position.z),
                QuaternionFLUtoRUF(new Quaternion((float)response.server_poses[0].orientation.x, (float)response.server_poses[0].orientation.y, (float)response.server_poses[0].orientation.z, (float)response.server_poses[0].orientation.w)));
        
            posIndicators.Add(go);
            selectObject = go;
            Debug.Log("Alignment " + (float)response.server_poses[0].position.x + ", " + (float)response.server_poses[0].position.y + ", " + (float)response.server_poses[0].position.z);
        }

        
        ChatRecord rec = new ChatRecord();
        rec.content = response.question_text;
        rec.type = 1;
        rec.time = Time.time;
        ttsObject.SynthesizeAndPlay(response.question_text);
        chatHistory.Add(rec);
        UpdateChatHistory();


        //reponseText.text = response.text;
    }

    void UpdateChatHistory()
    {
        responseText.text = "";
        int startIndex = (chatHistory.Count - 12) < 0 ? 0 : chatHistory.Count - 12;
        for (int i = chatHistory.Count - 1; i >= startIndex; i--)
        //for(int i = chatHistroy.Count - 1; i >= 0 ; i--) descending
        {
            if(chatHistory[i].type == 0)//me
            {
                responseText.text += "Me: " + chatHistory[i].content + "\n";
            }
            else//gpt
            {
                responseText.text += "Assistant: " + chatHistory[i].content + "\n";
            }
        }

        float targetHeight = responseText.preferredHeight > responseBackground.sizeDelta.y ? responseText.preferredHeight : responseBackground.sizeDelta.y;
        Vector2 size = new Vector2(responseText.GetComponent<RectTransform>().sizeDelta.x, targetHeight);
        responseText.GetComponent<RectTransform>().sizeDelta = size;
        responseContent.sizeDelta = new Vector2(responseContent.sizeDelta.x, targetHeight);
        responseText.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
    }

    void UpdateHighLevelPlan(HighlevelPlanMsg[] plan)
    {
        highLevelPlanText.text = "High Level Plan: \n";
        for(int i = 0; i < plan.Length; i++)
        {
            highLevelPlanText.text += (i + 1).ToString() + ". " + plan[i].action + " "
                + plan[i].object_name + ": "
                + "(" + plan[i].object_location.position.x.ToString("F3") + ", " + plan[i].object_location.position.y.ToString("F3") + ", " + plan[i].object_location.position.z.ToString("F3") + ")"
                + " to " + "(" + plan[i].place_location.position.x.ToString("F3") + ", " + plan[i].place_location.position.y.ToString("F3") + ", " + plan[i].place_location.position.z.ToString("F3") + ")\n";
        }

        float targetHeight = highLevelPlanText.preferredHeight > highLevelPlanBackground.sizeDelta.y ? highLevelPlanText.preferredHeight : highLevelPlanBackground.sizeDelta.y;
        Vector2 size = new Vector2(highLevelPlanText.GetComponent<RectTransform>().sizeDelta.x, targetHeight);
        highLevelPlanText.GetComponent<RectTransform>().sizeDelta = size;
        highLevelPlanContent.sizeDelta = new Vector2(highLevelPlanContent.sizeDelta.x, targetHeight);
        highLevelPlanText.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
    }

    public void ClearChatHistory()
    {
        chatHistory.Clear();
        responseText.text = "";
        float targetHeight = responseText.preferredHeight > responseBackground.sizeDelta.y ? responseText.preferredHeight : responseBackground.sizeDelta.y;
        Vector2 size = new Vector2(responseText.GetComponent<RectTransform>().sizeDelta.x, targetHeight);
        responseText.GetComponent<RectTransform>().sizeDelta = size;
        responseContent.sizeDelta = new Vector2(responseContent.sizeDelta.x, targetHeight);
        responseText.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
    }

    public Vector3 Vec3FLUToRUF(float x, float y, float z)
    {
        return new Vector3(-y, z, x);
    }

    public Quaternion QuaternionFLUtoRUF(Quaternion fluQuaternion)
    {
        // Convert FLU rotation to RUF by applying a corrective rotation
        //Quaternion conversionRotation = Quaternion.Euler(-180, 0, 0);
        return new Quaternion(fluQuaternion.w, -fluQuaternion.y, -fluQuaternion.z, fluQuaternion.x);
    }

    public void call_service_test(int mode)
    {
        string system_content = "";
        string user_content = "Gear";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = mode % 10;
        request.mode[1] = mode / 10;
        request.system_content = system_content;
        request.user_content = user_content;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_test);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_test(int mode, string content)
    {
        string system_content = "";
        string user_content = content;
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = mode % 10;
        request.mode[1] = mode / 10;
        request.system_content = system_content;
        request.user_content = user_content;
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_test);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_test_locate(Transform location)
    {
        string system_content = "";
        PointMsg newLocation = location.position.To<FLU>();
        string user_content = "";
        user_content += "Location: (" + newLocation.x.ToString("F3") + ", " + newLocation.y.ToString("F3") + ", " + newLocation.z.ToString("F3") + ")";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 4;
        request.mode[1] = 1;
        request.system_content = system_content;
        request.user_content = user_content;
        request.user_pose = new PoseMsg[1];
        request.user_pose[0] = new PoseMsg();
        request.user_pose[0].position = location.position.To<FLU>();
        request.user_pose[0].orientation = (location.rotation * Quaternion.Euler(0, 180, 180)).To<FLU>();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_test);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_test2_locate(Transform location)
    {
        string system_content = "";
        PointMsg newLocation = location.position.To<FLU>();
        string user_content = "";
        user_content += "Location: (" + newLocation.x.ToString("F3") + ", " + newLocation.y.ToString("F3") + ", " + newLocation.z.ToString("F3") + ")";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 4;
        request.mode[1] = 7;
        request.system_content = system_content;
        request.user_content = user_content;
        request.user_pose = new PoseMsg[1];
        request.user_pose[0] = new PoseMsg();
        request.user_pose[0].position = location.position.To<FLU>();
        request.user_pose[0].orientation = (location.rotation * Quaternion.Euler(0, 180, 180)).To<FLU>();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_test);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    public void call_service_test2_place(Transform location)
    {
        string system_content = "";
        PointMsg newLocation = location.position.To<FLU>();
        string user_content = "";
        user_content += "Location: (" + newLocation.x.ToString("F3") + ", " + newLocation.y.ToString("F3") + ", " + newLocation.z.ToString("F3") + ")";
        ChatRecord rec = new ChatRecord();
        rec.content = user_content;
        rec.type = 0;
        rec.time = Time.time;
        chatHistory.Add(rec);
        UpdateChatHistory();

        UnityInformationRequest request = new UnityInformationRequest();
        request.mode = new int[2];
        request.mode[0] = 4;
        request.mode[1] = 8;
        request.system_content = system_content;
        request.user_content = user_content;
        request.user_pose = new PoseMsg[1];
        request.user_pose[0] = new PoseMsg();
        request.user_pose[0].position = location.position.To<FLU>();
        request.user_pose[0].orientation = (location.rotation * Quaternion.Euler(0, 180, 180)).To<FLU>();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnityInformationResponse>(serviceName, request, Callback_service_test);
        awaitingResponseUntilTimestamp = Time.time + 1.0f;
    }

    void Callback_service_test(UnityInformationResponse response)
    {
        awaitingResponseUntilTimestamp = -1;
        for (int i = 0; i < posIndicators.Count; i++)
        {
            Destroy(posIndicators[i]);
        }
        posIndicators.Clear();

        
        if (response.server_poses.Length > 0)
        {
            if (response.server_verify == 0)
            {
                //single object, spawn gripper
                Debug.LogWarning(response.server_poses[0].orientation.x + ", " + response.server_poses[0].orientation.y + ", " + response.server_poses[0].orientation.z + ", " + response.server_poses[0].orientation.w);
                GameObject go = Instantiate(posIndicatorPrefab, Vec3FLUToRUF((float)response.server_poses[0].position.x, (float)response.server_poses[0].position.y, (float)response.server_poses[0].position.z),
                QuaternionFLUtoRUF(new Quaternion((float)response.server_poses[0].orientation.x, (float)response.server_poses[0].orientation.y, (float)response.server_poses[0].orientation.z, (float)response.server_poses[0].orientation.w)));
                posIndicators.Add(go);
                selectObject = go;
            }
            if (response.server_verify == 1)
            {
                //detect, spawn balls
                for (int i = 0; i < response.server_poses.Length; i++)
                {
                    GameObject go = Instantiate(objIndicatorPrefab, Vec3FLUToRUF((float)response.server_poses[i].position.x, (float)response.server_poses[i].position.y, (float)response.server_poses[i].position.z),
                QuaternionFLUtoRUF(new Quaternion((float)response.server_poses[i].orientation.x, (float)response.server_poses[i].orientation.y, (float)response.server_poses[i].orientation.z, (float)response.server_poses[i].orientation.w)));
                    go.GetComponent<SeletableObject>().index = posIndicators.Count;
                    posIndicators.Add(go);
                }
            }
        }

        //Ask question
        Debug.Log("GPT server said " + response.question_text);
        ChatRecord rec = new ChatRecord();
        rec.content = response.question_text;
        rec.type = 1;
        rec.time = Time.time;
        ttsObject.SynthesizeAndPlay(response.question_text);
        chatHistory.Add(rec);
        UpdateChatHistory();
        //reponseText.text = response.text;
    }


    public void SelectObject(int index)
    {
        if(index < posIndicators.Count)
        {
            selectObject = posIndicators[index];
            for (int i = 0; i < posIndicators.Count; i++)
            {
                if (i != index)
                {
                    posIndicators[i].SetActive(false);
                }
            }
        }
    }

    public void SendSelectedPos()
    {
        if (selectObject != null)
        {
            call_service_stage_locate(selectObject.transform);
        }
        else
        {
            Debug.LogWarning("Didn't select any object!");
        }
    }

    public void SendSelectedPosTest()
    {
        if (selectObject != null)
        {
            selectObject.transform.position += new Vector3(0.0f, 0.02f, 0.0f);
            call_service_test2_locate(selectObject.transform);
        }
        else
        {
            Debug.LogWarning("Didn't select any object!");
        }
    }

    public void GenerateAndSendRecords()
    {
        ChatRecord rec = new ChatRecord();
        rec.content = "Save Chat History.";
        rec.type = 0;
        rec.time = Time.time;
        ttsObject.SynthesizeAndPlay(rec.content);
        chatHistory.Add(rec);
        UpdateChatHistory();
        saveService.call_service(GetChatRecordsString(), GetSystemPositionsString());
    }

    public string GetSystemPositionsString()
    {
        string result = "";
        for(int i = 0; i < systemPositionsRecord.Count; i++)
        {
            result += systemPositionsRecord[i];
        }
        return result;
    }

    public string GetChatRecordsString()
    {
        string result = "";
        for(int i = 0; i < chatHistory.Count; i++)
        {
            string role = chatHistory[i].type == 0 ? "Me" : "Assistant";
            result += chatHistory[i].time + "# " + role + "# " + chatHistory[i].content + "#\n";
        }
        return result;
    }



}
