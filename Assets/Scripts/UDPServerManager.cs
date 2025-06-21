using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class UDPServerManager : MonoBehaviour
{
    private UdpClient udpServer;
    private IPEndPoint clientEndPoint;

    [SerializeField] private Transform cupTransform;
    [SerializeField] private int serverPort = 5555;

    private bool isClientConnected = false;
    private bool isRunning = true;

    private Vector3 lastCupPosition;
    private Quaternion lastCupRotation;

    private void Start()
    {
        lastCupPosition = cupTransform.localPosition;
        lastCupRotation = cupTransform.localRotation;

        StartUDPServerAsync(serverPort);
    }

    private void LateUpdate()
    {
        if (!isClientConnected || cupTransform == null) return;

        bool positionChanged =
            lastCupPosition != cupTransform.localPosition ||
            lastCupRotation != cupTransform.localRotation;

        if (positionChanged)
        {
            lastCupPosition = cupTransform.localPosition;
            lastCupRotation = cupTransform.localRotation;
            SendCupData();
        }
    }

    private async void StartUDPServerAsync(int port)
    {
        try
        {
            udpServer = new UdpClient(port);
            Debug.Log("Server started on port " + port + ". Waiting for client...");

            while (isRunning)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint senderEndPoint = result.RemoteEndPoint;

                if (receivedMessage == "CONNECT_REQUEST")
                {
                    Debug.Log("Client connection request received from " + senderEndPoint);
                    clientEndPoint = senderEndPoint;
                    isClientConnected = true;

                    SendData("CONNECT_ACK", clientEndPoint);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            Debug.Log("Socket closed, stopping server loop.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Server error: " + ex.Message);
        }
    }

    private void SendCupData()
    {
        if (clientEndPoint == null) return;

        var data = new JObject
        {
            ["x"] = cupTransform.localPosition.x,
            ["y"] = cupTransform.localPosition.y,
            ["z"] = cupTransform.localPosition.z,
            ["rx"] = cupTransform.localRotation.x,
            ["ry"] = cupTransform.localRotation.y,
            ["rz"] = cupTransform.localRotation.z,
            ["rw"] = cupTransform.localRotation.w
        };

        SendData(data.ToString(Newtonsoft.Json.Formatting.None), clientEndPoint);
    }

    private void SendData(string message, IPEndPoint endPoint)
    {
        try
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            udpServer.Send(sendBytes, sendBytes.Length, endPoint);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending data: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        isRunning = false;

        if (udpServer != null)
        {
            udpServer.Close();
            udpServer = null;
        }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }
}
