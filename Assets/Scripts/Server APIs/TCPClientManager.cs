using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;

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
    [SerializeField] private string serverIP = "192.168.2.243";
    private int _serverPort = 12345;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private byte[] _receiveBuffer = new byte[4096]; // 4KB buffer
    private bool _isConnected = false;
    private bool _isRunning = true;
    private bool _userDisconnected = true;
    private CancellationTokenSource _cts;

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

    // DEBUG
    private async void Start()
    {
        // Initialize the TCP client
        await ConnectToServer();
    }

    public async Task ConnectToServer()
    {
        if (_isConnected) return;

        try
        {
            _cts = new CancellationTokenSource();
            Debug.Log($"Connecting to server at {serverIP}:{_serverPort}...");
            _userDisconnected = false; // Reset biến khi người dùng chủ động kết nối
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(serverIP, _serverPort);
            _stream = _tcpClient.GetStream();
            _isConnected = true;
            Debug.Log("Connected to server!");

            // Start receiving data
            _ = ReceiveDataLoop(_cts.Token);

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
            Debug.Log("Disconnecting from server...");
            _userDisconnected = true;
            _isConnected = false; // Đặt cờ này trước để loop dừng lại

            // Tạo bản sao của các đối tượng để tránh race condition
            var cts = _cts;
            var client = _tcpClient;
            var stream = _stream;

            // Reset các biến để ngăn các thao tác khác sử dụng chúng
            _cts = null;
            _tcpClient = null;
            _stream = null;

            // Hủy CancellationToken 
            try { cts?.Cancel(); } catch { }

            // Chờ một chút để đảm bảo lệnh cancel được xử lý
            Task.Delay(50).Wait();

            // Đóng stream trước
            try { stream?.Close(); } catch { }

            // Sau đó đóng client
            try { client?.Close(); } catch { }

            OnDisconnected?.Invoke();
            Debug.Log("Disconnected from server");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during disconnect: {e.Message}");
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
        if (!_isConnected || _stream == null || _tcpClient == null || !_tcpClient.Connected)
        {
            Debug.LogWarning("Cannot send request - not connected");
            _isConnected = false;
            OnDisconnected?.Invoke();
            return;
        }

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

    private async Task ReceiveDataLoop(CancellationToken cancellationToken)
    {
        while (_isRunning && _isConnected)
        {
            try
            {
                // Kiểm tra đầy đủ trước khi đọc
                if (_stream == null || _tcpClient == null || !_tcpClient.Connected)
                {
                    Debug.Log("Connection is closed or invalid, exiting receive loop");
                    _isConnected = false;
                    OnDisconnected?.Invoke();
                    break;
                }

                // Sử dụng timeout để tránh treo vô hạn
                using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 giây timeout

                    int bytesRead = await _stream.ReadAsync(
                        _receiveBuffer,
                        0,
                        _receiveBuffer.Length,
                        timeoutCts.Token
                    );

                    if (bytesRead > 0)
                    {
                        string response = Encoding.UTF8.GetString(_receiveBuffer, 0, bytesRead);
                        ProcessData(response);
                    }
                    else
                    {
                        Debug.Log("Connection closed by server (zero bytes)");
                        _isConnected = false;
                        OnDisconnected?.Invoke();
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Data receiving operation was cancelled");
                break;
            }
            catch (IOException ioEx)
            {
                // Xử lý riêng lỗi IO - thường xảy ra khi socket bị đóng
                Debug.LogWarning($"IO Exception: {ioEx.Message}");
                break;
            }
            catch (ObjectDisposedException dispEx)
            {
                // Stream đã bị dispose
                Debug.LogWarning($"Object disposed: {dispEx.Message}");
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving data: {e.Message}");
                if (_isConnected) // Chỉ cập nhật trạng thái nếu chưa disconnect
                {
                    _isConnected = false;
                    OnDisconnected?.Invoke();
                }
                break;
            }
        }

        Debug.Log("Receive loop ended");
    }

    // private void ProcessData(string data)
    // {
    //     try
    //     {
    //         // Parse the JSON data
    //         JObject jsonObject = JObject.Parse(data);

    //         // Check if this is a real-time update message
    //         if (jsonObject["type"]?.ToString() == "real_time_update")
    //         {
    //             Debug.Log($"Processing data: {data}");
    //             // Extract the cup data from the message
    //             JObject cupDataObject = jsonObject["data"] as JObject;
    //             if (cupDataObject != null && cupDataObject["type"]?.ToString() == "cup")
    //             {
    //                 // Trigger the event with the cup data
    //                 OnCupDataReceived?.Invoke(cupDataObject);
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError($"Error parsing data: {e.Message}");
    //     }
    // }
    private void ProcessData(string data)
    {
        int braceCount = 0;
        int startIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == '{')
            {
                if (braceCount == 0)
                {
                    startIndex = i; // Đánh dấu bắt đầu của một JSON object mới
                }
                braceCount++;
            }
            else if (data[i] == '}')
            {
                braceCount--;
                if (braceCount == 0 && startIndex >= 0) // Đã tìm thấy một JSON object hoàn chỉnh
                {
                    string jsonString = data.Substring(startIndex, i - startIndex + 1);
                    try
                    {
                        JObject jsonObject = JObject.Parse(jsonString);
                        // Xử lý từng JSON object riêng lẻ
                        ProcessSingleJsonObject(jsonObject);
                    }
                    catch (JsonReaderException jsonEx)
                    {
                        Debug.LogError($"Error parsing JSON segment: {jsonEx.Message}\nSegment: {jsonString}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error processing JSON segment: {e.Message}\nSegment: {jsonString}");
                    }
                    startIndex = -1; // Reset startIndex để tìm object tiếp theo
                }
                else if (braceCount < 0)
                {
                    // Lỗi cú pháp JSON (dấu } thừa)
                    Debug.LogError($"Invalid JSON structure detected near index {i}. Resetting brace count.");
                    braceCount = 0; // Cố gắng phục hồi bằng cách reset
                    startIndex = -1;
                }
            }
        }

        if (braceCount != 0)
        {
            Debug.LogWarning($"Incomplete JSON data received or parsing error. Remaining brace count: {braceCount}");
            // Có thể bạn muốn lưu trữ phần dữ liệu chưa hoàn chỉnh này để ghép với lần nhận tiếp theo
        }
    }

    // Hàm mới để xử lý một JSON object đã được parse
    private void ProcessSingleJsonObject(JObject jsonObject)
    {
        try
        {
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
            // Thêm các kiểu message khác nếu cần
            else if (jsonObject["result"]?["status"] != null)
            {
                Debug.Log($"Processing result message: {jsonObject.ToString(Formatting.None)}");
                // Xử lý các message kết quả (ví dụ: xác nhận subscribe)
                string status = jsonObject["result"]["status"].ToString();
                string message = jsonObject["result"]["message"]?.ToString();
                Debug.Log($"Server Result: Status='{status}', Message='{message}'");
            }
            else
            {
                Debug.LogWarning($"Received unknown JSON object type: {jsonObject.ToString(Formatting.None)}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing parsed JSON object: {e.Message}\nObject: {jsonObject.ToString(Formatting.None)}");
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

    private void OnDestroy()
    {
        Debug.Log("TCPClientManager is being destroyed");
        _isRunning = false;
        Disconnect();
    }
}