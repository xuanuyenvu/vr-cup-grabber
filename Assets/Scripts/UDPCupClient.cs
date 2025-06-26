using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using com.rfilkov.kinect;

public class UDPCupClient : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;

    [SerializeField] private Transform cupTransform;
    [SerializeField] private string serverIP = "192.168.2.55";
    [SerializeField] private int serverPort = 5555;
    [SerializeField] private Kinect4AzureInterface kinectInterface;

    private string latestMessage = null;
    private bool hasNewMessage = false;

    private bool isConnected = false;
    private float lastMessageTime;

    private void Start()
    {
        LoadKinectConfigFromFile();
        LoadServerConfigFromFile();
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);


        StartReceiveLoop();

        StartCoroutine(ManageConnection());
    }

    private void LoadServerConfigFromFile()
    {
        string serverConfigPath = Path.Combine(Application.streamingAssetsPath, "server_config.json");
        if (File.Exists(serverConfigPath))
        {
            string json = File.ReadAllText(serverConfigPath);
            JObject config = JObject.Parse(json);

            serverIP = config["serverIP"]?.ToString();
            serverPort = config["serverPort"]?.ToObject<int>() ?? 5555;
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

            Debug.Log($"Server configuration loaded: IP={serverIP}, Port={serverPort}");
        }
        else
        {
            Debug.LogWarning("Config file not found, using default settings.");
        }
    }

    private void LoadKinectConfigFromFile()
    {
        string kinectConfigPath = Path.Combine(Application.streamingAssetsPath, "kinect_config.json");

        if (File.Exists(kinectConfigPath))
        {
            string json = File.ReadAllText(kinectConfigPath);
            JObject config = JObject.Parse(json);
            kinectInterface.deviceIndex = config["deviceIndex"]?.ToObject<int>() ?? 0;
        }
        else
        {
            Debug.LogWarning("Kinect config file not found, using default settings.");
        }
    }

    private async void StartReceiveLoop()
    {
        while (true)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                if (!isConnected)
                {
                    isConnected = true;
                    Debug.Log("Successfully connected to server!");
                }

                // Ghi message mới
                latestMessage = message;
                hasNewMessage = true;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error receiving data: " + ex.Message);
            }
        }
    }

    private void Update()
    {
        if (isConnected && Time.time - lastMessageTime > 5f)
        {
            Debug.LogWarning("Connection to server timed out. Reconnecting...");
            isConnected = false;
            StartCoroutine(ManageConnection());
        }

        if (hasNewMessage)
        {
            hasNewMessage = false;
            lastMessageTime = Time.time;
            Debug.Log("Received message: " + latestMessage);
            ProcessReceivedMessage(latestMessage);
        }
    }

    private System.Collections.IEnumerator ManageConnection()
    {
        while (!isConnected)
        {
            Debug.Log("Attempting to connect to server...");
            SendData("CONNECT_REQUEST");
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        if (message == "CONNECT_ACK") return;

        try
        {
            var data = JObject.Parse(message);

            Vector3 position = new Vector3(
                (float)data["x"],
                (float)data["y"],
                (float)data["z"]
            );

            Quaternion rotation = new Quaternion(
                (float)data["rx"],
                (float)data["ry"],
                (float)data["rz"],
                (float)data["rw"]
            );

            if (cupTransform != null)
            {
                cupTransform.localPosition = position;
                cupTransform.localRotation = rotation;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not parse received message: " + e.Message);
        }
    }

    private void SendData(string message)
    {
        try
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            udpClient.Send(sendBytes, sendBytes.Length, serverEndPoint);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending data: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}