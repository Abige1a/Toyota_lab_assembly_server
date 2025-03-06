using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.XarmMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using System;

public class UGVImageService : MonoBehaviour
{
    // Start is called before the first frame update

    ROSConnection ros;
    public string serviceName = "UGVIMG";
    public GameObject UGVImageDisplayer;
    public GameObject RobotImageDisplayer;
    public float countdowntime = 0.1f;
    private float time = 0.0f;



    float awaitingResponseUntilTimestamp = 0.0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<ImageServiceRequest, ImageServiceResponse>(serviceName);




        




    }

    // Update is called once per frame
    void Update()
    {
        awaitingResponseUntilTimestamp += Time.deltaTime;
        if (awaitingResponseUntilTimestamp > 0.08f)
        {
            ImageServiceRequest ImageRequest = new ImageServiceRequest();
            //1-webcam only 2-realsense only 3-realsense and robotmaster 4-realsense and webcam
            ImageRequest.servicenumber = 4;
            ros.SendServiceMessage<ImageServiceResponse>(serviceName, ImageRequest, setimage);


            awaitingResponseUntilTimestamp = 0.0f;

        }

        
    }

    public void setimage(ImageServiceResponse response) 
    {
        UnityImageMsg msg = response.ugv_image;
        int width = msg.width;
        int height = msg.height;
        UnityImageMsg msg2 = response.robot_image;
        int width2 = msg2.width;
        int height2 = msg2.height;
        Debug.Log(msg.width + ", " + msg.height);
        



        //Debug.LogError("length is " + msg.data.Length);

        RobotImageDisplayer.GetComponent<CameraStreamer>().SetTexture(width, height, msg.data);
        //RobotImageDisplayer.GetComponent<CameraStreamer>().SetTexture(width2, height2, msg2.data);

    }


}
