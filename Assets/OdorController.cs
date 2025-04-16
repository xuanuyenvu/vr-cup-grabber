using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OdorController : MonoBehaviour
{
    private SerialPortManager _serialPortManager;
    public SmellAPI smellAPI;
    public TasteAPI tasteAPI;

    public void ConnectToPort(string portName)
    {
        _serialPortManager = new SerialPortManager();
        _serialPortManager.Connect(portName);

        smellAPI = new SmellAPI(_serialPortManager);
        tasteAPI = new TasteAPI(_serialPortManager);
    }
    public void Disconnect()
    {
        _serialPortManager?.Disconnect();
        _serialPortManager = null;
        smellAPI = null;
        tasteAPI = null;
    }
    
    public bool IsConnected => _serialPortManager != null && _serialPortManager.IsConnected;
    
    void OnDestroy()
    {
        _serialPortManager?.Disconnect();
    }
}
