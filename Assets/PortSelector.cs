using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.Serialization;

public class PortSelector : MonoBehaviour
{
    [SerializeField] private OdorController odorController;

    [HideInInspector] public List<string> portNames = new List<string>();
    [HideInInspector] public int selectedPortIndex = 0;

    public void RefreshPortList()
    {
        portNames.Clear();
        portNames.AddRange(SerialPort.GetPortNames());
    }

    public void Connect()
    {
        if (portNames.Count == 0 || selectedPortIndex >= portNames.Count) return;

        string selectedPort = portNames[selectedPortIndex];
        odorController.ConnectToPort(selectedPort);
        Debug.Log("Connected to port: " + selectedPort);
    }

    public void Disconnect()
    {
        odorController.Disconnect();
        Debug.Log("Disconnected from port.");
    }
}