 using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CupData
{
    public string type; // "cup"
    public float x;
    public float y;
    public float z;
    public int rotation;
    public string handleDirection;
}

public class CoordinateReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cupGameObject;

    [Header("Transform Settings")]
    [SerializeField] private float scaleFactor;

    private CupData _latestCupData;
    private UdpClient _udpServer;
    private bool _isRunning = true;
    private bool _hasNewCupData = false;

    async void Start()
    {
        await Task.Run(StartServer);
    }

    async Task StartServer()
    {
        try
        {
            _udpServer = new UdpClient(65432);
            // Debug.Log("UDP Server started on port 65432...");

            while (_isRunning)
            {
                UdpReceiveResult result = await _udpServer.ReceiveAsync();
                byte[] buffer = result.Buffer;
                string data = Encoding.UTF8.GetString(buffer);

                // Debug.Log("Received data: " + data);
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
        if (!data.Contains("\"type\": \"cup\"")) return;
        try
        {
            CupData cd = JsonUtility.FromJson<CupData>(data);
            _latestCupData = cd;
            _hasNewCupData = true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing cup data: " + e.Message);
        }
    }

    void Update()
    {
        if (!_hasNewCupData) return;
        UpdateCupTransform(_latestCupData);
        _hasNewCupData = false;
    }

    void UpdateCupTransform(CupData cd)
    {
        Vector3 cupPos = new Vector3(cd.x * scaleFactor, cd.z * scaleFactor, cd.y * scaleFactor);

        if (!cupGameObject) return;
        cupGameObject.transform.localPosition = cupPos;
        
        if (cd.handleDirection == null) return;
        cupGameObject.transform.localEulerAngles = new Vector3(0, cd.rotation, 0);   
    }

    void OnApplicationQuit()
    {
        _isRunning = false;
        _udpServer?.Close();
    }
}