using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image statusIndicator;

    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;
    [SerializeField] private Color connectingColor = Color.yellow;
    
    private bool _isConnecting = false;

    private void Start()
    {
        // Khởi tạo các giá trị mặc định
        ipInputField.text = TCPClientManager.Instance.ServerIP;
        
        // Gắn sự kiện cho các nút nhấn
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        
        // Đăng ký lắng nghe sự kiện kết nối
        TCPClientManager.Instance.OnConnected += OnConnected;
        TCPClientManager.Instance.OnDisconnected += OnDisconnected;
        
        // Cập nhật UI ban đầu
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        // Hủy đăng ký lắng nghe sự kiện khi component bị hủy
        if (TCPClientManager.Instance != null)
        {
            TCPClientManager.Instance.OnConnected -= OnConnected;
            TCPClientManager.Instance.OnDisconnected -= OnDisconnected;
        }
    }
    
    // Phương thức trung gian không bất đồng bộ để gắn vào button
    public void OnConnectButtonClicked()
    {
        // Cập nhật các tham số kết nối
        TCPClientManager.Instance.ServerIP = ipInputField.text;
        
        // Hiển thị trạng thái đang kết nối
        _isConnecting = true;
        statusText.text = "Connecting...";
        statusIndicator.color = connectingColor;
        connectButton.interactable = false;
        
        // Gọi phương thức bất đồng bộ mà không chờ đợi kết quả
        _ = TCPClientManager.Instance.ConnectToServer();
    }
    
    public void OnDisconnectButtonClicked()
    {
        TCPClientManager.Instance.Disconnect();
    }
    
    private void OnConnected()
    {
        _isConnecting = false;
        UpdateUI();
    }
    
    private void OnDisconnected() 
    {
        _isConnecting = false;
        UpdateUI();
    }
        
    private void UpdateUI()
    {
        bool isConnected = TCPClientManager.Instance.IsConnected;
        
        // Cập nhật trạng thái hiển thị
        if (_isConnecting)
        {
            statusText.text = "Connecting...";
            statusIndicator.color = connectingColor;
        }
        else if (isConnected)
        {
            statusText.text = "Connected";
            statusIndicator.color = connectedColor;
        }
        else
        {
            statusText.text = "Disconnected";
            statusIndicator.color = disconnectedColor;
        }
        
        // Cập nhật trạng thái các nút nhấn
        connectButton.interactable = !isConnected && !_isConnecting;
        disconnectButton.interactable = isConnected;
        
        // Chỉ cho phép chỉnh sửa IP và port khi không kết nối
        ipInputField.interactable = !isConnected && !_isConnecting;
    }
}