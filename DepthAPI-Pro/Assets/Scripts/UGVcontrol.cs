using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.XarmMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class UGVcontrol : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/managed/joy";
    public float publishMessageFrequency = 0.5f;
    private float timeElapsed;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void publish_UGV_speed(Vector2 twistinput)
    {


        TwistMsg twistinfo = new TwistMsg
        {
            linear = new Vector3Msg(twistinput[0],0,0),
            angular= new Vector3Msg(0,0,twistinput[1])
        };

        // Finally send the message to server_endpoint.py running in ROS
        ros.Publish(topicName, twistinfo);
    }
}
