using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectionDataManager : MonoBehaviour
{
    public DistributeTCPClient tcpClient;


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
    public List<Tuple<Vector3, Quaternion>> getDetectionPositionbyName(string user_name, List<int> mode_list, Dictionary<string, object> request_info)
    {
        // 创建消息对象
        var message = new Distribute_TCP_Message(user_name, mode_list, request_info);

        // 调用 TCP 客户端的发送方法并接收回复
        Distribute_TCP_Message one_message = tcpClient.SendMessageToServer(message);

        // 从 receivedMessage 中获取 additional_info 字典
        Dictionary<string, object> additional_information = one_message.additional_info;

        // 检查是否包含 "position" 键并手动转换
        if (additional_information.ContainsKey("position") && additional_information["position"] is List<object> rawPosition)
        {
            // 创建一个新的 List<Tuple<Vector3, Quaternion>> 来存储解析后的 Unity 坐标和方向
            List<Tuple<Vector3, Quaternion>> positionData = new List<Tuple<Vector3, Quaternion>>();

            foreach (var innerList in rawPosition)
            {
                // 检查 innerList 是否是 List<object> 类型
                if (innerList is List<double> innerObjectList )
                {
                    // 将 List<object> 转换为 List<double>
                    List<double> doubleList = innerObjectList.Select(item => Convert.ToDouble(item)).ToList();
                    if (doubleList.Count ==7) 
                    {
                        // 解析前 3 个元素为 Vector3（位置坐标）
                        Vector3 position = Vec3FLUToRUF((float)doubleList[0], (float)doubleList[1], (float)doubleList[2]);

                        // 解析后 4 个元素为 Quaternion（旋转）
                        Quaternion rotation = QuaternionFLUtoRUF(new Quaternion(
                        (float)doubleList[3],
                        (float)doubleList[4],
                        (float)doubleList[5],
                        (float)doubleList[6]));

                        Vector3 eulerAngles = rotation.eulerAngles;

                        Debug.LogWarning(eulerAngles.ToString());

                        // 将位置和方向作为一个元组添加到列表
                        positionData.Add(Tuple.Create(position, rotation));

                    }
                    
                }
                else
                {
                    Debug.LogWarning("Skipping an entry in position data: not a list of 7 doubles.");
                }
            }

            Debug.LogError(positionData.Count);

            return positionData;
        }

        // 如果 "position" 不存在或类型不匹配，返回一个空列表
        return new List<Tuple<Vector3, Quaternion>>();
    }
}
