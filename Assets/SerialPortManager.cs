using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class SerialPortManager
{
    private SerialPort _serialPort;

    public void Connect(string portName, int baudRate = 115200)
    {
        _serialPort = new SerialPort(portName, baudRate);
        _serialPort.Open();
    }
    
    public void Disconnect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }
    
    public void SendCommand(string command)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.WriteLine(command);
        }
        else
        {
            Debug.LogError("Serial port is not open.");
        }
    }
    
    public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
}
