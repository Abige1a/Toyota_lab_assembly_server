using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.XarmMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class UnityRobotClient : MonoBehaviour
{
    ROSConnection ros;
    public string serviceName = "xarm_unity_service";
    float awaitingResponseUntilTimestamp = -1;
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<UnityRobotServiceRequest, UnityRobotServiceResponse>(serviceName);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Robot_client(int mode, double gripper_status,Vector3 position, Quaternion rotation, double[] velocity_control)
    {
        UnityRobotServiceRequest robot_request = new UnityRobotServiceRequest();
        robot_request.mode = mode;
        robot_request.gripper_status = gripper_status;

        PoseMsg pose = new PoseMsg();
        pose.position = position.To<FLU>();
        pose.orientation = rotation.To<FLU>();

        PoseMsg[] pose_list = new PoseMsg[1];
        pose_list[0] = pose;
        robot_request.target_pose = pose_list;
        robot_request.velocity_control = velocity_control;
        ros.SendServiceMessage<UnityRobotServiceResponse>(serviceName, robot_request, Robot_response);
    }

    public void Robot_response(UnityRobotServiceResponse response)
    {
        Debug.LogWarning("current pose is " + response.current_pose);
    }
}
