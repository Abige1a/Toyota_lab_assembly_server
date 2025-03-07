//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.XarmMoveit
{
    [Serializable]
    public class HighlevelPlanMsg : Message
    {
        public const string k_RosMessageName = "xarm_moveit/HighlevelPlan";
        public override string RosMessageName => k_RosMessageName;

        public string action;
        public string object_name;
        public Geometry.PoseMsg object_location;
        public Geometry.PoseMsg place_location;

        public HighlevelPlanMsg()
        {
            this.action = "";
            this.object_name = "";
            this.object_location = new Geometry.PoseMsg();
            this.place_location = new Geometry.PoseMsg();
        }

        public HighlevelPlanMsg(string action, string object_name, Geometry.PoseMsg object_location, Geometry.PoseMsg place_location)
        {
            this.action = action;
            this.object_name = object_name;
            this.object_location = object_location;
            this.place_location = place_location;
        }

        public static HighlevelPlanMsg Deserialize(MessageDeserializer deserializer) => new HighlevelPlanMsg(deserializer);

        private HighlevelPlanMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.action);
            deserializer.Read(out this.object_name);
            this.object_location = Geometry.PoseMsg.Deserialize(deserializer);
            this.place_location = Geometry.PoseMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action);
            serializer.Write(this.object_name);
            serializer.Write(this.object_location);
            serializer.Write(this.place_location);
        }

        public override string ToString()
        {
            return "HighlevelPlanMsg: " +
            "\naction: " + action.ToString() +
            "\nobject_name: " + object_name.ToString() +
            "\nobject_location: " + object_location.ToString() +
            "\nplace_location: " + place_location.ToString();
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
