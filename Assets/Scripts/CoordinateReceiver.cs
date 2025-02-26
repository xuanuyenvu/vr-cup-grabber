using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class CupData
{
    public string type; // "cup"
    public float x;
    public float y;
    public float z;
}

public class CoordinateReceiver : MonoBehaviour
{
    private UdpClient udpServer; // Thay TcpListener bằng UdpClient
    private bool isRunning = true;

    [Header("References")]
    public GameObject cupCube;

    [Header("Transform Settings")]
    public float scaleFactor = 0.001f;

    private CupData latestCupData;

    async void Start()
    {
        await Task.Run(() => StartServer());
    }

    async Task StartServer()
    {
        try
        {
            udpServer = new UdpClient(65432); // Tạo UDP server trên port 65432
            Debug.Log("UDP Server started on port 65432...");

            while (isRunning)
            {
                // Nhận dữ liệu bất đồng bộ từ bất kỳ client nào
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                byte[] buffer = result.Buffer;
                string data = Encoding.UTF8.GetString(buffer);

                Debug.Log("Received data: " + data);
                ProcessData(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in UDP server: " + e.Message);
        }
    }

    void ProcessData(string data)
    {
        if (data.Contains("\"type\": \"cup\""))
        {
            try
            {
                CupData cd = JsonUtility.FromJson<CupData>(data);
                if (cd != null)
                {
                    latestCupData = cd;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing cup data: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (latestCupData != null)
        {
            UpdateCupPosition(latestCupData);
            latestCupData = null;
        }
    }

    void UpdateCupPosition(CupData cd)
    {
        Vector3 cupPos = new Vector3(cd.x * scaleFactor, cd.z * scaleFactor, cd.y * scaleFactor);

        if (cupCube != null)
        {
            cupCube.transform.localPosition = cupPos;
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        udpServer?.Close(); // Đóng UDP server
    }
}