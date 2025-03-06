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

public class UnityDocumentSaveService : MonoBehaviour
{
    ROSConnection ros;

    public string serviceName = "document_save";

    void Start()
    {

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<UnitySaveRequest, UnitySaveResponse>(serviceName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void call_service(string chatHistory, string positionRecord)
    {

        UnitySaveRequest request = new UnitySaveRequest();
        request.mode = 0;
        request.foldername = "";
        request.chathistory = chatHistory;
        request.statistics = positionRecord;
        //request.prompt_image = new UnityImageMsg();
        //if(texture != null)
        //{
        //    request.prompt_image = new UnityImageMsg(texture.height, texture.width, texture.EncodeToJPG());
        //}


        Debug.LogWarning("sending service!");

        ros.SendServiceMessage<UnitySaveResponse>(serviceName, request, Callback_service);


    }

    void Callback_service(UnitySaveResponse response)
    {
        Debug.Log(response.result);
    }
}
