using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using LitJson;
//using UnityEditor.Experimental.GraphView;
using System.Net;

public class Distribute_TCP_Message
{
    public string text { get; set; }
    public List<int> number_list { get; set; }
    public Dictionary<string, object> additional_info { get; set; }

    public Distribute_TCP_Message()
    {
        // 初始化属性，防止空引用
        text = string.Empty;
        number_list = new List<int>();
        additional_info = new Dictionary<string, object>();
    }
    public Distribute_TCP_Message(string text, List<int> numberList, Dictionary<string, object> additionalInfo)
    {
        text = text;
        number_list = numberList;
        additional_info = additionalInfo;
    }
}

public class DistributeTCPClient : MonoBehaviour
{
    public string ip;
    public int port_num;
    private TcpClient client;
    private NetworkStream stream;
    

    void Start()
    {
        // 确保 dataManager 不为空
        

        

    }

    private void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            client = new TcpClient(ipAddress, port);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
        }
    }

    public Distribute_TCP_Message SendMessageToServer(Distribute_TCP_Message message)
    {

        ConnectToServer(ip, port_num);
        // 将消息对象序列化为 JSON 并添加换行符
        string jsonMessage = JsonMapper.ToJson(message) + "\n";
        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);

        try
        {
            // 发送消息到服务器
            stream.Write(data, 0, data.Length);
            Debug.Log("Message sent: " + jsonMessage);

            // 接收并处理服务器的回复
            return ReceiveReplyFromServer(); 
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e.Message}");
            return null;
        }
    }

    private Distribute_TCP_Message Analyze_reply_json_from_server(string jsonMessage)
    {
        Debug.Log("start analyze json from server");
        // 将 reply 转换为 JsonData 对象，以便动态解析
        JsonData receivedData = JsonMapper.ToObject(jsonMessage);

        // 手动创建 Distribute_TCP_Message 对象
        Distribute_TCP_Message receivedMessage = new Distribute_TCP_Message();

        // 检查 "Text" 键是否存在
        if (receivedData.Keys.Contains("text"))
        {
            receivedMessage.text = receivedData["text"].ToString();
            Debug.Log("Received message text: " + receivedMessage.text);
        }

        // 检查 "NumberList" 键是否存在并且是数组
        if (receivedData.Keys.Contains("number_list") && receivedData["number_list"].IsArray)
        {
            receivedMessage.number_list = new List<int>();
            foreach (JsonData number in receivedData["number_list"])
            {
                receivedMessage.number_list.Add((int)number);
            }
        }

        // 检查 "AdditionalInfo" 键是否存在并且是对象
        if (receivedData.Keys.Contains("additional_info") && receivedData["additional_info"].IsObject)
        {
            receivedMessage.additional_info = new Dictionary<string, object>();
            foreach (string key in receivedData["additional_info"].Keys)
            {
                JsonData value = receivedData["additional_info"][key];


                if (value.IsString)
                {
                    receivedMessage.additional_info.Add(key, value.ToString());
                }
                else if (value.IsInt)
                {
                    receivedMessage.additional_info.Add(key, (int)value);
                    
                }
                else if (value.IsBoolean)
                {
                    receivedMessage.additional_info.Add(key, (bool)value);
                }
                else if (value.IsArray)
                {
                    List<object> arrayValues = new List<object>();

                    foreach (JsonData item in value)
                    {
                        if (item.IsArray)
                        {
                            // 处理二维数组的情况
                            List<double> innerArray = new List<double>();
                            foreach (JsonData innerItem in item)
                            {
                                innerArray.Add((double)innerItem);  // 直接解析为 double
                            }

                            //Debug.LogError("Inner Array: [" + string.Join(", ", innerArray) + "]");
                            arrayValues.Add(innerArray);  // 将内层数组添加到外层列表
                        }
                        else
                        {
                            // 处理普通数组的情况
                            arrayValues.Add((double)item);  // 解析为 double 并添加到 arrayValues
                        }
                    }
                    receivedMessage.additional_info.Add(key, arrayValues);
                }
                else if (value.IsObject)
                {
                    Dictionary<string, object> nestedDict = new Dictionary<string, object>();
                    foreach (string nestedKey in value.Keys)
                    {

                        nestedDict.Add(nestedKey, value[nestedKey].ToString()); // 或者进一步递归解析
                    }
                    receivedMessage.additional_info.Add(key, nestedDict);
                }
            }
        }

        return receivedMessage;

    }

    public Distribute_TCP_Message ReceiveReplyFromServer()
    {
        // 读取服务器返回的信息
        try
        {
            byte[] buffer = new byte[1024]; // 创建一个缓冲区
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string reply = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Distribute_TCP_Message Message_from_server=Analyze_reply_json_from_server(reply);
            Debug.Log("Message parsed successfully!");
            return Message_from_server;
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving reply: {e.Message}");
            return null;
        }
        finally
        {
            // 关闭连接
            CloseConnection();

        }
    }

    private void CloseConnection()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Connection closed.");
    }
}
