//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.XarmMoveit
{
    [Serializable]
    public class UnityInformationRequest : Message
    {
        public const string k_RosMessageName = "xarm_moveit/UnityInformation";
        public override string RosMessageName => k_RosMessageName;

        public int[] mode;
        public int object_index;
        public int phase_index;
        public string system_content;
        public string user_content;
        public UnityImageMsg prompt_image;
        public Geometry.PoseMsg[] user_pose;
        public int[] points;
        public int[] boxes;
        public int user_action_verify;
        public int user_verify_verify;

        public UnityInformationRequest()
        {
            this.mode = new int[0];
            this.object_index = 0;
            this.phase_index = 0;
            this.system_content = "";
            this.user_content = "";
            this.prompt_image = new UnityImageMsg();
            this.user_pose = new Geometry.PoseMsg[0];
            this.points = new int[0];
            this.boxes = new int[0];
            this.user_action_verify = 0;
            this.user_verify_verify = 0;
        }

        public UnityInformationRequest(int[] mode, int object_index, int phase_index, string system_content, string user_content, UnityImageMsg prompt_image, Geometry.PoseMsg[] user_pose, int[] points, int[] boxes, int user_action_verify, int user_verify_verify)
        {
            this.mode = mode;
            this.object_index = object_index;
            this.phase_index = phase_index;
            this.system_content = system_content;
            this.user_content = user_content;
            this.prompt_image = prompt_image;
            this.user_pose = user_pose;
            this.points = points;
            this.boxes = boxes;
            this.user_action_verify = user_action_verify;
            this.user_verify_verify = user_verify_verify;
        }

        public static UnityInformationRequest Deserialize(MessageDeserializer deserializer) => new UnityInformationRequest(deserializer);

        private UnityInformationRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.mode, sizeof(int), deserializer.ReadLength());
            deserializer.Read(out this.object_index);
            deserializer.Read(out this.phase_index);
            deserializer.Read(out this.system_content);
            deserializer.Read(out this.user_content);
            this.prompt_image = UnityImageMsg.Deserialize(deserializer);
            deserializer.Read(out this.user_pose, Geometry.PoseMsg.Deserialize, deserializer.ReadLength());
            deserializer.Read(out this.points, sizeof(int), deserializer.ReadLength());
            deserializer.Read(out this.boxes, sizeof(int), deserializer.ReadLength());
            deserializer.Read(out this.user_action_verify);
            deserializer.Read(out this.user_verify_verify);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.mode);
            serializer.Write(this.mode);
            serializer.Write(this.object_index);
            serializer.Write(this.phase_index);
            serializer.Write(this.system_content);
            serializer.Write(this.user_content);
            serializer.Write(this.prompt_image);
            serializer.WriteLength(this.user_pose);
            serializer.Write(this.user_pose);
            serializer.WriteLength(this.points);
            serializer.Write(this.points);
            serializer.WriteLength(this.boxes);
            serializer.Write(this.boxes);
            serializer.Write(this.user_action_verify);
            serializer.Write(this.user_verify_verify);
        }

        public override string ToString()
        {
            return "UnityInformationRequest: " +
            "\nmode: " + System.String.Join(", ", mode.ToList()) +
            "\nobject_index: " + object_index.ToString() +
            "\nphase_index: " + phase_index.ToString() +
            "\nsystem_content: " + system_content.ToString() +
            "\nuser_content: " + user_content.ToString() +
            "\nprompt_image: " + prompt_image.ToString() +
            "\nuser_pose: " + System.String.Join(", ", user_pose.ToList()) +
            "\npoints: " + System.String.Join(", ", points.ToList()) +
            "\nboxes: " + System.String.Join(", ", boxes.ToList()) +
            "\nuser_action_verify: " + user_action_verify.ToString() +
            "\nuser_verify_verify: " + user_verify_verify.ToString();
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
