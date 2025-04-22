using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TCPClientManager : MonoBehaviour
{
    private static TCPClientManager _instance;
    public static TCPClientManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("TCPClientManager");
                _instance = go.AddComponent<TCPClientManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Server Connection")]
    [SerializeField] private string serverIP = "127.0.0.1";
    private int _serverPort = 12345;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private byte[] _receiveBuffer = new byte[4096]; // 4KB buffer
    private bool _isConnected = false;
    private bool _isRunning = true;
    private bool _userDisconnected = false;

    // Events
    public delegate void MessageReceivedHandler(JObject jsonData);
    public event MessageReceivedHandler OnCupDataReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    // Public properties
    public bool IsConnected => _isConnected;
    public string ServerIP { get => serverIP; set => serverIP = value; }
    public int ServerPort { get => _serverPort; set => _serverPort = value; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Auto reconnect if disconnected
        if (!_isConnected && _isRunning && !_userDisconnected)
        {
            // Attempt to reconnect every 3 seconds
            _ = ReconnectAsync();
        }
    }

    public async Task ConnectToServer()
    {
        if (_isConnected) return;

        try
        {
            Debug.Log($"Connecting to server at {serverIP}:{_serverPort}...");
            _userDisconnected = false; // Reset biến khi người dùng chủ động kết nối
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(serverIP, _serverPort);
            _stream = _tcpClient.GetStream();
            _isConnected = true;
            Debug.Log("Connected to server!");

            // Start receiving data
            _ = ReceiveDataLoop();

            // Subscribe to real-time updates
            SubscribeToRealTimeUpdates();

            // Trigger event
            OnConnected?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
            _isConnected = false;
        }
    }

    public void Disconnect()
    {
        if (!_isConnected) return;

        try
        {
            _userDisconnected = true; // Đánh dấu là người dùng chủ động ngắt kết nối
            _isConnected = false;
            _tcpClient?.Close();
            _tcpClient = null;
            _stream = null;

            OnDisconnected?.Invoke();
            Debug.Log("Disconnected from server");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error disconnecting: {e.Message}");
        }
    }


    private async Task ReconnectAsync()
    {
        Debug.Log("Attempting to reconnect...");
        await Task.Delay(3000); // Wait 3 seconds before retrying
        await ConnectToServer();
    }

    private void SubscribeToRealTimeUpdates()
    {
        if (!_isConnected) return;

        try
        {
            var subscribeRequest = new Dictionary<string, string>
            {
                { "action", "subscribe_realtime" }
            };

            SendRequest(subscribeRequest);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error subscribing to real-time updates: {e.Message}");
        }
    }

    public void SendRequest(object requestData)
    {
        if (!_isConnected) return;

        try
        {
            string jsonRequest = JsonConvert.SerializeObject(requestData);
            byte[] requestBytes = Encoding.UTF8.GetBytes(jsonRequest);
            _stream.Write(requestBytes, 0, requestBytes.Length);
            Debug.Log($"Sent request: {jsonRequest}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending request: {e.Message}");
            _isConnected = false;
            OnDisconnected?.Invoke();
        }
    }

    private async Task ReceiveDataLoop()
    {
        while (_isRunning && _isConnected)
        {
            try
            {
                int bytesRead = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);
                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(_receiveBuffer, 0, bytesRead);
                    Debug.Log($"Received data: {response}");
                    ProcessData(response);
                }
                else
                {
                    // Connection closed by server
                    Debug.Log("Connection closed by server");
                    _isConnected = false;
                    OnDisconnected?.Invoke();
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving data: {e.Message}");
                _isConnected = false;
                OnDisconnected?.Invoke();
                break;
            }
        }
    }

    private void ProcessData(string data)
    {
        try
        {
            // Parse the JSON data
            JObject jsonObject = JObject.Parse(data);

            // Check if this is a real-time update message
            if (jsonObject["type"]?.ToString() == "real_time_update")
            {
                // Extract the cup data from the message
                JObject cupDataObject = jsonObject["data"] as JObject;
                if (cupDataObject != null && cupDataObject["type"]?.ToString() == "cup")
                {
                    // Trigger the event with the cup data
                    OnCupDataReceived?.Invoke(cupDataObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing data: {e.Message}");
        }
    }

    // API methods for server communication
    public void DiffuseSmell(List<string> smells, int duration)
    {
        Debug.Log($"Diffusing smells: {string.Join(", ", smells)} for {duration} ms");
        var request = new Dictionary<string, object>
        {
            { "action", "diffuse_smell" },
            { "smells", smells },
            { "duration", duration }
        };

        SendRequest(request);
    }

    public void StopSmell(List<string> smells)
    {
        var request = new Dictionary<string, object>
        {
            { "action", "stop_smell" },
            { "smells", smells }
        };

        SendRequest(request);
    }

    public void DiffuseTaste(List<string> tastes, int duration, int speed)
    {
        var request = new Dictionary<string, object>
        {
            { "action", "diffuse_taste" },
            { "tastes", tastes },
            { "duration", duration },
            { "speed", speed }
        };

        SendRequest(request);
    }

    public void StopTaste(List<string> tastes)
    {
        var request = new Dictionary<string, object>
        {
            { "action", "stop_taste" },
            { "tastes", tastes }
        };

        SendRequest(request);
    }

    public void ChangeDetector(string modelPath, int? cupClassId, bool detectHandles)
    {
        var request = new Dictionary<string, object>
        {
            { "action", "change_detector" },
            { "model_path", modelPath }
        };

        if (cupClassId.HasValue)
        {
            request.Add("cup_class_id", cupClassId.Value);
        }

        request.Add("detect_handles", detectHandles);

        SendRequest(request);
    }

    private void OnApplicationQuit()
    {
        _isRunning = false;
        _tcpClient?.Close();
    }
}