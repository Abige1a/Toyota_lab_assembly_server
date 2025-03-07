//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.XarmMoveit
{
    [Serializable]
    public class VehicleControlMsg : Message
    {
        public const string k_RosMessageName = "xarm_moveit/VehicleControl";
        public override string RosMessageName => k_RosMessageName;

        public Geometry.TwistMsg twist_value;
        public Geometry.PoseMsg head_pose;

        public VehicleControlMsg()
        {
            this.twist_value = new Geometry.TwistMsg();
            this.head_pose = new Geometry.PoseMsg();
        }

        public VehicleControlMsg(Geometry.TwistMsg twist_value, Geometry.PoseMsg head_pose)
        {
            this.twist_value = twist_value;
            this.head_pose = head_pose;
        }

        public static VehicleControlMsg Deserialize(MessageDeserializer deserializer) => new VehicleControlMsg(deserializer);

        private VehicleControlMsg(MessageDeserializer deserializer)
        {
            this.twist_value = Geometry.TwistMsg.Deserialize(deserializer);
            this.head_pose = Geometry.PoseMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.twist_value);
            serializer.Write(this.head_pose);
        }

        public override string ToString()
        {
            return "VehicleControlMsg: " +
            "\ntwist_value: " + twist_value.ToString() +
            "\nhead_pose: " + head_pose.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
