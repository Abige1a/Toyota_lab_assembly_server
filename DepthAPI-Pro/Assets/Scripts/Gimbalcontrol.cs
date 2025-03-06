using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.XarmMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class Gimbalcontrol : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/turtle1/cmd_vel";
    public float publishMessageFrequency = 0.5f;
    private float timeElapsed;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            

            timeElapsed = 0;
        }

    }

    public void publish_gimbal_pose(Vector3 target_position, Quaternion rotation)
    {
        

        PoseMsg cubePos = new PoseMsg
        {
            position = target_position.To<FLU>(),
            orientation = rotation.To<FLU>()
        };

        // Finally send the message to server_endpoint.py running in ROS
        ros.Publish(topicName, cubePos);
    }
}
