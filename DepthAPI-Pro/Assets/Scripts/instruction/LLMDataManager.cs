using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLMDataManager : MonoBehaviour
{
    public DistributeTCPClient tcpClient;

    

    public string SendMessageToServer(string text, List<int> numberList, Dictionary<string, object> additionalInfo)
    {
        var message = new Distribute_TCP_Message(text, numberList, additionalInfo);
        Distribute_TCP_Message one_message= tcpClient.SendMessageToServer(message); // 调用 TCP 客户端的发送方法
        List<object> arrayValues = new List<object>();

        return "one sentence from server";
    }
}
